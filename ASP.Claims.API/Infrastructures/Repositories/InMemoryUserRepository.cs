using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using FluentResults;
using System.Collections.Concurrent;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public InMemoryUserRepository()
    {
        var defaultUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = Role.Admin
        };
        _users[defaultUser.Username] = defaultUser;
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        _users.TryGetValue(username, out var user);
        return Task.FromResult(user);
    }

    public Task<Result<User>> SaveAsync(User user)
    {
        if (_users.ContainsKey(user.Username))
            return Task.FromResult(Result.Fail<User>("User already exists."));
        _users[user.Username] = user;
        return Task.FromResult(Result.Ok(user));
    }
}
