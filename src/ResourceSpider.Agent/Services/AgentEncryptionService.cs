using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ResourceSpider.Agent.Services;

public interface IAgentEncryptionService
{
    (string Ciphertext, string Iv) Encrypt(string plaintext);

    string Decrypt(string ciphertext, string iv);

    (string Ciphertext, string Iv) EncryptObject<T>(T obj);

    T? DecryptObject<T>(string ciphertext, string iv);
}

public class AgentEncryptionService : IAgentEncryptionService
{
    private readonly byte[] _key;

    public AgentEncryptionService(Agent.Config.OnlineModeOptions options)
    {
        var keyBase64 = options.EncryptionKey ?? string.Empty;
        if (string.IsNullOrEmpty(keyBase64))
        {
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(options.AgentId + options.AgentToken));
        }
        else
        {
            _key = Convert.FromBase64String(keyBase64);
        }
    }

    public (string Ciphertext, string Iv) Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        var ivBase64 = Convert.ToBase64String(aes.IV);

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return (Convert.ToBase64String(cipherBytes), ivBase64);
    }

    public string Decrypt(string ciphertext, string iv)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(iv);

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(ciphertext);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public (string Ciphertext, string Iv) EncryptObject<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encrypt(json);
    }

    public T? DecryptObject<T>(string ciphertext, string iv)
    {
        var json = Decrypt(ciphertext, iv);
        return JsonSerializer.Deserialize<T>(json);
    }
}
