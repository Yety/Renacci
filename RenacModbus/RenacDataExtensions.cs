namespace RenacModbus;

public static class RenacDataExtensions
{
    public static double ToDouble(this Span<byte> bytes, int startIndex, bool b = true)
    {
        if(b)
            return (short)(bytes[startIndex] * 256.0 + bytes[startIndex + 1] * 1.0);
        
        return bytes[startIndex] * 256.0 + bytes[startIndex + 1] * 1.0;
        
        
    }
    
    public static int ToInt(this byte[] bytes) => bytes[0] * 256 + bytes[1];

    public static byte[] ToBytes(this short value)
    {
        return BitConverter.GetBytes(value).Reverse().ToArray();
    }
    
    public static byte[] CalculateModbusCrc(this byte[] data)
    {
        ushort crc = 0xFFFF;

        foreach (byte datum in data)
        {
            crc ^= datum;

            for (int i = 0; i < 8; i++)
            {
                bool lsb = (crc & 0x0001) != 0;
                crc >>= 1;

                if (lsb)
                {
                    crc ^= 0xA001;
                }
            }
        }

        byte[] crcBytes = new byte[2];
        crcBytes[0] = (byte)(crc & 0x00FF);       // Low byte
        crcBytes[1] = (byte)((crc >> 8) & 0x00FF); // High byte

        return crcBytes;
    }
}