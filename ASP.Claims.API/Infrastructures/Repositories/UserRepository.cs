using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using FluentResults;
using Cosmos = Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class CosmosDbUserRepository(Cosmos.Container container) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var query = container.GetItemQueryIterator<User>(
            $"SELECT * FROM c WHERE c.Username = '{username}'");
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            return response.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<Result<User>> SaveAsync(User user)
    {
        try
        {
            user.Id ??= Guid.NewGuid().ToString();
            await container.CreateItemAsync(user, new Cosmos.PartitionKey(user.Id));
            return Result.Ok(user);
        }
        catch (Exception)
        {
            return Result.Fail<User>("Failed to create user.");
        }
    }
}
