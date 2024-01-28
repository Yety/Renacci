using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Renacci;

namespace RenacModbus;

public class TcpClientHandler : IDisposable
{
    enum ReceiveState
    {
        Idle,
        Sending,
        WaitingForDeviceId,
        WaitingForFunctionCode,
        WaitingForTotalLength,
        WaitingForData,
        ReceivingData,
        WaitingForCrc,
        RecevingCrc
    }

    private ReceiveState State
    {
        get => _state;
        set
        {
            _logger.LogDebug("State changed from {OldState} to {NewState}", _state, value);
            _state = value;
        }
    }

    private ReceiveState _state = ReceiveState.Idle;
    private readonly RenacModbusOptions _options;
    private readonly ILogger<TcpClientHandler> _logger;
    private readonly TcpClient _client;
    private readonly NetworkStream _networkStream;

    private readonly CancellationTokenSource _cts;
    private readonly Task _task;
    private int _expectedTotalLengthInBytes;
    private int _receivedTotalLength;
    private byte[] _receiveBuffer = Array.Empty<byte>();
    private byte[] _resultBuffer = Array.Empty<byte>();
    private int _numTotalDataBytesReceived;
    private readonly SemaphoreSlim _waitForResponse = new(0, 1);
    private byte[] _crcBuffer = new byte[2];

    public TcpClientHandler(RenacModbusOptions options, RandomByteGenerator randomByteGenerator,
        ILogger<TcpClientHandler> logger)
    {
        _options = options;
        _logger = logger;
        _client = new TcpClient();
        _client.Connect(options.Host, options.Port);
        _networkStream = _client.GetStream();
        _cts = new CancellationTokenSource();
        _task = ReceiveAsync(_cts.Token);
    }

    public byte[] ReadHoldingRegisters(short startingAddress, short numRegisters)
    {
        _logger.LogInformation("ReadHoldingRegisters Start");
        var request = new byte[8];
        request[0] = _options.DeviceId;
        request[1] = 0x03;
        var startAddress = startingAddress.ToBytes();
        request[2] = startAddress[0];
        request[3] = startAddress[1];
        _expectedTotalLengthInBytes = (numRegisters * 2);
        var lengthBytes = numRegisters.ToBytes();
        request[4] = lengthBytes[0];
        request[5] = lengthBytes[1];

        var crc = request.Take(6).ToArray().CalculateModbusCrc();
        request[6] = crc[0];
        request[7] = crc[1];
        State = ReceiveState.Sending;
        SendBytes(request);
        _receiveBuffer = new byte[_expectedTotalLengthInBytes];
        State = ReceiveState.WaitingForDeviceId;
        var result = _waitForResponse.Wait(2000);
        if (!result)
        {
            _logger.LogWarning("Timeout while waiting for response");
            ResetState();
        }


        return _resultBuffer;
    }

    private void SendBytes(byte[] bytes)
    {
        _logger.LogInformation("Send Start");
        var hexString = HexToDecimalExtensions.ByteArrayToString(bytes, bytes.Length);
        _logger.LogDebug("S -> NumBytes: {NumBytes}, Data : {HexString}", bytes.Length, hexString);
        _networkStream.Write(bytes);
        _logger.LogInformation("Send End");
    }

    private async Task ReceiveAsync(CancellationToken token)
    {
        byte[] buffer = new byte[10000];
        try
        {
            while (true)
            {
                var numBytes = await _networkStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                var hexString = HexToDecimalExtensions.ByteArrayToString(buffer, numBytes);
                _logger.LogDebug("R -> NumBytes: {NumBytes}, Data : {HexString}", numBytes, hexString);
                ProcessBytes(buffer, numBytes);
            }
        }
        catch (OperationCanceledException)
        {
            if (_client.Connected)
            {
                _networkStream.Close();
                _logger.LogInformation("Connection to {@ClientRemoteEndPoint} closed", _client.Client.RemoteEndPoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("{Name}: {ExMessage}", ex.GetType().Name, ex.Message);
        }
    }

    private void ProcessBytes(byte[] buffer, int numBytes)
    {
        for (int i = 0; i < numBytes; ++i)
            ProcessByte(buffer[i]);
    }

    private void ProcessByte(byte b)
    {
        _logger.LogDebug("ProcessByte {Byte}", b);
        switch (State)
        {
            case ReceiveState.Idle:
                _logger.LogDebug("Received byte while idle {Byte}", b);
                break;
            case ReceiveState.Sending:
                _logger.LogWarning("Received byte while sending {Byte}", b);
                break;
            case ReceiveState.WaitingForDeviceId:
                if (b == _options.DeviceId)
                    State = ReceiveState.WaitingForFunctionCode;
                else
                {
                    _logger.LogWarning("Received unexpected byte while waiting for device id {Byte}", b);
                    ResetState();
                }

                break;
            case ReceiveState.WaitingForFunctionCode:
                if (b == 0x03)
                    State = ReceiveState.WaitingForTotalLength;
                else
                {
                    _logger.LogWarning("Received unexpected byte while waiting for function code {Byte}", b);
                    ResetState();
                }

                break;
            case ReceiveState.WaitingForTotalLength:
                _receivedTotalLength = b;
                if (_receivedTotalLength != _expectedTotalLengthInBytes)
                {
                    _logger.LogWarning(
                        "Received unexpected total length {ReceivedTotalLength}, expected {ExpectedTotalLength}",
                        _receivedTotalLength, _expectedTotalLengthInBytes);
                    ResetState();
                }
                else
                    State = ReceiveState.WaitingForData;
                break;
            case ReceiveState.WaitingForData:
                _receiveBuffer[_numTotalDataBytesReceived] = b;
                _numTotalDataBytesReceived++;
                
                State = ReceiveState.ReceivingData;
                break;
            case ReceiveState.ReceivingData:
                _receiveBuffer[_numTotalDataBytesReceived] = b;
                ++_numTotalDataBytesReceived;
                if (_numTotalDataBytesReceived >= _expectedTotalLengthInBytes) 
                    State = ReceiveState.WaitingForCrc;
                break;
            case ReceiveState.WaitingForCrc:
                _crcBuffer[0] = b;
                _state = ReceiveState.RecevingCrc;
                break;
            case ReceiveState.RecevingCrc:
                _crcBuffer[1] = b;
                
                var allBytesReceived = new byte[_numTotalDataBytesReceived + 3];
                allBytesReceived[0] = _options.DeviceId;
                allBytesReceived[1] = 0x03;
                allBytesReceived[2] = (byte) _numTotalDataBytesReceived;
                _receiveBuffer.CopyTo(allBytesReceived, 3);
                var crc = allBytesReceived.CalculateModbusCrc();
                if (crc[0] != _crcBuffer[0] || crc[1] != _crcBuffer[1])
                {
                    _logger.LogWarning("CRC mismatch");
                    ResetState();
                    
                }
                else
                {
                    _resultBuffer = new byte[_expectedTotalLengthInBytes];
                    _receiveBuffer.CopyTo(_resultBuffer, 0);
                    ResetState();    
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetState()
    {
        State = ReceiveState.Idle;
        _expectedTotalLengthInBytes = 0;
        _receivedTotalLength = 0;
        _receiveBuffer = Array.Empty<byte>();
        _crcBuffer = new byte[2];
        _numTotalDataBytesReceived = 0;
        _waitForResponse.Release();
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing");
        _networkStream.Dispose();
        _client.Dispose();
        _cts.Dispose();
        _task.Wait(5000);
        _task.Dispose();
    }
}