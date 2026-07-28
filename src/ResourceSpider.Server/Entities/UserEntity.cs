using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Entities;

/// <summary>
/// 用户实体，映射数据库 users 表
/// 表示系统用户，包含用户身份认证和角色授权信息
/// </summary>
[SugarTable("users")]
public class UserEntity
{
    /// <summary>
    /// 数据库自增主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 用户唯一标识符，业务主键
    /// </summary>
    [SugarColumn(Length = 64)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名，用于登录认证，全局唯一
    /// </summary>
    [SugarColumn(Length = 128)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希值，使用 BCrypt 算法加密存储
    /// </summary>
    [SugarColumn(Length = 256)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色：Admin-管理员，Operator-操作员，Viewer-只读用户
    /// </summary>
    [SugarColumn(Length = 64)]
    public string Role { get; set; } = "Operator";

    /// <summary>
    /// 用户状态：0-禁用，1-启用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 用户创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 用户信息最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
