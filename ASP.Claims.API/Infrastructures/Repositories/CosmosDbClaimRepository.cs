using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Resources;
using FluentResults;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class CosmosDbClaimRepository(Container container) : IClaimRepository
{
    private readonly Container _container = container;

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
            // Read as JObject to inspect discriminator
            var response = await _container.ReadItemAsync<JObject>(id.ToString(), new PartitionKey(id.ToString()));
            var json = response.Resource;

            // Get discriminator value
            var claimTypeString = json["ClaimType"]?.ToString();
            if (string.IsNullOrEmpty(claimTypeString))
                throw new Exception("ClaimType discriminator missing!");

            var claimType = Enum.Parse<ClaimType>(claimTypeString);

            // Deserialize to correct type
            return claimType switch
            {
                ClaimType.Property => json.ToObject<PropertyClaim>(),
                ClaimType.Vehicle => json.ToObject<VehicleClaim>(),
                ClaimType.Travel => json.ToObject<TravelClaim>(),
                _ => throw new Exception($"Unknown ClaimType: {claimTypeString}"),
            };
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
        catch (Exception ex)
        {
            Console.WriteLine($"[CosmosDbClaimRepository.Save] {ex}");
            throw;
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
        catch (Exception ex)
        {
            Console.WriteLine($"[CosmosDbClaimRepository.UpdateClaim] {ex}");
            throw;
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
        catch (Exception ex)
        {
            Console.WriteLine($"[CosmosDbClaimRepository.DeleteClaim] {ex}");
            throw;
        }
    }
}