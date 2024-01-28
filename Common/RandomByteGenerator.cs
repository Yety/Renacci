using System.Security.Cryptography;

namespace Renacci;

public class RandomByteGenerator
{
    public byte[] GenerateRandomBytes(int numberOfBytes)
    {
        byte[] randomBytes = new byte[numberOfBytes];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomBytes);
        return randomBytes;
    }
}