using SqlSugar;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;

namespace Larpx.PersonalTools.ResourceSpider.Server.Repositories;

/// <summary>
/// 用户数据仓储接口，定义用户实体的数据访问操作
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 根据用户 ID 获取用户实体
    /// </summary>
    /// <param name="userId">用户唯一标识符</param>
    /// <returns>用户实体，未找到时返回 null</returns>
    Task<UserEntity?> GetByIdAsync(string userId);

    /// <summary>
    /// 根据用户名获取用户实体
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>用户实体，未找到时返回 null</returns>
    Task<UserEntity?> GetByUsernameAsync(string username);

    /// <summary>
    /// 新增用户实体
    /// </summary>
    /// <param name="entity">用户实体</param>
    Task AddAsync(UserEntity entity);

    /// <summary>
    /// 更新用户实体，忽略 CreatedAt 字段
    /// </summary>
    /// <param name="entity">用户实体</param>
    Task UpdateAsync(UserEntity entity);
}

/// <summary>
/// 用户数据仓储实现，基于 SQLSugar 提供用户数据的增删改查操作
/// </summary>
public class UserRepository : IUserRepository
{
    /// <summary>
    /// SQLSugar 数据库客户端实例
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化用户仓储实例
    /// </summary>
    /// <param name="db">SQLSugar 数据库客户端</param>
    public UserRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetByIdAsync(string userId)
    {
        return await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        return await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.Username == username);
    }

    /// <inheritdoc/>
    public async Task AddAsync(UserEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(UserEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(u => u.CreatedAt)
            .ExecuteCommandAsync();
    }
}
