using Core;
using Microsoft.Extensions.Logging;
using Renacci;

namespace RenacModbus;

public class RenacStatusRepository(TcpClientHandler _tcpClientHandler, ILogger<RenacStatusRepository> _logger) : IStatusRepository
{
    public async Task<InverterStatus> GetStatus()
    {
        var data = _tcpClientHandler.ReadHoldingRegisters(11000, 114);
        if(data.Length == 0)
        {
            _logger.LogError("Failed to get data for inverter");
            return new InverterStatus();
        }
        var factory = new InverterStatusFactory();
        return factory.Create(data);
    }
}