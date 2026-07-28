using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Larpx.PersonalTools.ResourceSpider.Agent.Services;

/// <summary>
/// Agent 加密服务接口，定义 AES 加密/解密操作，用于保护 Agent 与服务端之间的通信数据
/// </summary>
public interface IAgentEncryptionService
{
    /// <summary>
    /// 使用 AES 加密明文字符串
    /// </summary>
    /// <param name="plaintext">待加密的明文字符串</param>
    /// <returns>包含密文和初始化向量的元组</returns>
    (string Ciphertext, string Iv) Encrypt(string plaintext);

    /// <summary>
    /// 使用 AES 解密密文字符串
    /// </summary>
    /// <param name="ciphertext">Base64 编码的密文</param>
    /// <param name="iv">Base64 编码的初始化向量</param>
    /// <returns>解密后的明文字符串</returns>
    string Decrypt(string ciphertext, string iv);

    /// <summary>
    /// 将对象序列化为 JSON 后进行 AES 加密
    /// </summary>
    /// <typeparam name="T">待加密对象的类型</typeparam>
    /// <param name="obj">待加密的对象</param>
    /// <returns>包含密文和初始化向量的元组</returns>
    (string Ciphertext, string Iv) EncryptObject<T>(T obj);

    /// <summary>
    /// 解密密文并反序列化为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="ciphertext">Base64 编码的密文</param>
    /// <param name="iv">Base64 编码的初始化向量</param>
    /// <returns>反序列化后的对象，解密失败返回 null</returns>
    T? DecryptObject<T>(string ciphertext, string iv);
}

/// <summary>
/// Agent 加密服务实现，使用 AES-256-CBC 算法进行数据加密和解密
/// 密钥来源优先级：配置中的 EncryptionKey > AgentId+AgentToken 的 SHA256 哈希
/// </summary>
public class AgentEncryptionService : IAgentEncryptionService
{
    /// <summary>
    /// AES 加密密钥的字节数组
    /// </summary>
    private readonly byte[] _key;

    /// <summary>
    /// 初始化加密服务，根据配置生成 AES 密钥
    /// </summary>
    /// <param name="options">在线模式配置，包含 EncryptionKey 或 AgentId/AgentToken</param>
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

    /// <summary>
    /// 使用 AES 加密明文字符串，自动生成随机 IV
    /// </summary>
    /// <param name="plaintext">待加密的明文字符串</param>
    /// <returns>包含 Base64 编码密文和 IV 的元组</returns>
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

    /// <summary>
    /// 使用 AES 解密密文字符串
    /// </summary>
    /// <param name="ciphertext">Base64 编码的密文</param>
    /// <param name="iv">Base64 编码的初始化向量</param>
    /// <returns>解密后的明文字符串</returns>
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

    /// <summary>
    /// 将对象序列化为 JSON 后进行 AES 加密
    /// </summary>
    /// <typeparam name="T">待加密对象的类型</typeparam>
    /// <param name="obj">待加密的对象</param>
    /// <returns>包含密文和初始化向量的元组</returns>
    public (string Ciphertext, string Iv) EncryptObject<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encrypt(json);
    }

    /// <summary>
    /// 解密密文并反序列化为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="ciphertext">Base64 编码的密文</param>
    /// <param name="iv">Base64 编码的初始化向量</param>
    /// <returns>反序列化后的对象，解密失败返回 null</returns>
    public T? DecryptObject<T>(string ciphertext, string iv)
    {
        var json = Decrypt(ciphertext, iv);
        return JsonSerializer.Deserialize<T>(json);
    }
}
