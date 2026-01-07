using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using FluentResults;
using Microsoft.Azure.Cosmos.Linq;
using Cosmos = Microsoft.Azure.Cosmos;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class CosmosDbUserRepository(Cosmos.Container container) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var iterator = container.GetItemLinqQueryable<User>()
            .Where(u => u.Username == username)
            .ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            return response.Resource.FirstOrDefault();
        }
        return null;
    }

    public async Task<Result<User>> SaveAsync(User user)
    {
        user.Id ??= Guid.NewGuid().ToString();
        try
        {
            await container.CreateItemAsync(user, new Cosmos.PartitionKey(user.Id));    
            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            // Optionally log the error here if you want
            return Result.Fail<User>(ex.Message);
        }
    }
}
