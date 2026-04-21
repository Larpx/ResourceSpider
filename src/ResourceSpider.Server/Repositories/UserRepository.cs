using SqlSugar;
using ResourceSpider.Server.Entities;

namespace ResourceSpider.Server.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(string userId);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task AddAsync(UserEntity entity);
    Task UpdateAsync(UserEntity entity);
}

public class UserRepository : IUserRepository
{
    private readonly ISqlSugarClient _db;

    public UserRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<UserEntity?> GetByIdAsync(string userId)
    {
        return await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.UserId == userId);
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        return await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.Username == username);
    }

    public async Task AddAsync(UserEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
    }

    public async Task UpdateAsync(UserEntity entity)
    {
        await _db.Updateable(entity)
            .IgnoreColumns(u => u.CreatedAt)
            .ExecuteCommandAsync();
    }
}
