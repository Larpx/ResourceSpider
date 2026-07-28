using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Larpx.PersonalTools.ResourceSpider.Server.Services;

public interface IDataEncryptionService
{
    string Encrypt(string plaintext, out string ivBase64);

    string Decrypt(string ciphertextBase64, string ivBase64);

    string EncryptObject<T>(T obj, out string ivBase64);

    T? DecryptObject<T>(string ciphertextBase64, string ivBase64);
}

public class DataEncryptionService : IDataEncryptionService
{
    private readonly byte[] _key;

    public DataEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"] ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        _key = Convert.FromBase64String(keyBase64);
    }

    public string Encrypt(string plaintext, out string ivBase64)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        ivBase64 = Convert.ToBase64String(aes.IV);

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string ciphertextBase64, string ivBase64)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(ivBase64);

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(ciphertextBase64);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public string EncryptObject<T>(T obj, out string ivBase64)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encrypt(json, out ivBase64);
    }

    public T? DecryptObject<T>(string ciphertextBase64, string ivBase64)
    {
        var json = Decrypt(ciphertextBase64, ivBase64);
        return JsonSerializer.Deserialize<T>(json);
    }
}
