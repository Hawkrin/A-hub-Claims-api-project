using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Resources;
using FluentResults;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class CosmosDbClaimRepository : IClaimRepository
{
    private readonly Container _container;

    public CosmosDbClaimRepository(Container container)
    {
        _container = container;
    }

    public async Task<IEnumerable<Claim>> GetAll()
    {
        var query = _container.GetItemQueryIterator<Claim>("SELECT * FROM c");
        var results = new List<Claim>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<IEnumerable<Claim>> GetByType(ClaimType type)
    {
        var query = _container.GetItemQueryIterator<Claim>(
            $"SELECT * FROM c WHERE c.Type = {(int)type}");
        var results = new List<Claim>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<Claim?> GetById(Guid id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Claim>(id.ToString(), new PartitionKey(id.ToString()));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Result<Claim>> Save(Claim claim)
    {
        try
        {
            if (claim.Id == Guid.Empty)
                claim.Id = Guid.NewGuid();

            await _container.CreateItemAsync(claim, new PartitionKey(claim.Id.ToString()));
            return Result.Ok(claim);
        }
        catch (Exception)
        {
            return Result.Fail<Claim>(ErrorMessages.ErrorMessage_FailedToCreateClaim);
        }
    }

    public async Task<Result> UpdateClaim(Claim claim)
    {
        try
        {
            await _container.ReplaceItemAsync(claim, claim.Id.ToString(), new PartitionKey(claim.Id.ToString()));
            return Result.Ok();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);
        }
        catch (Exception)
        {
            return Result.Fail(ErrorMessages.ErrorMessage_FailedToCreateClaim);
        }
    }

    public async Task<Result> DeleteClaim(Guid id)
    {
        try
        {
            await _container.DeleteItemAsync<Claim>(id.ToString(), new PartitionKey(id.ToString()));
            return Result.Ok();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);
        }
        catch (Exception)
        {
            return Result.Fail(ErrorMessages.ErrorMessage_FailedToCreateClaim);
        }
    }
}