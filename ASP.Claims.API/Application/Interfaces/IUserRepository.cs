using ASP.Claims.API.Domain.Entities;
using FluentResults;

namespace ASP.Claims.API.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<Result<User>> SaveAsync(User user);
}