using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Resources;
using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class CosmosDbClaimRepository(Container container) : IClaimRepository
{
    private readonly Container _container = container;

    private static Claim? DeserializeClaim(JObject json)
    {
        var typeToken = json["Type"] ?? throw new Exception("Type discriminator missing!");
        var claimType = (ClaimType)typeToken.Value<int>();

        return claimType switch
        {
            ClaimType.Property => json.ToObject<PropertyClaim>(),
            ClaimType.Vehicle => json.ToObject<VehicleClaim>(),
            ClaimType.Travel => json.ToObject<TravelClaim>(),
            _ => throw new Exception($"Unknown ClaimType: {typeToken}")
        };
    }

    public async Task<IEnumerable<Claim>> GetAll()
    {
        var iterator = _container.GetItemLinqQueryable<JObject>()
                                 .ToFeedIterator();

        var results = new List<Claim>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var json in response)
            {
                var claim = DeserializeClaim(json);
                if (claim != null)
                    results.Add(claim);
            }
        }
        return results;
    }

    public async Task<IEnumerable<Claim>> GetByType(ClaimType type)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.Type = @type")
            .WithParameter("@type", (int)type);

        var iterator = _container.GetItemQueryIterator<JObject>(query);
        var results = new List<Claim>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var json in response)
            {
                var claim = DeserializeClaim(json);
                if (claim != null)
                    results.Add(claim);
            }
        }
        return results;
    }

    public async Task<Claim?> GetById(Guid id)
    {
        try
        {
            var response = await _container.ReadItemAsync<JObject>(id.ToString(), new PartitionKey(id.ToString()));
            var json = response.Resource;
            return DeserializeClaim(json);
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

            switch (claim)
            {
                case PropertyClaim propertyClaim:
                    await _container.CreateItemAsync(propertyClaim, new PartitionKey(propertyClaim.Id.ToString()));
                    break;
                case VehicleClaim vehicleClaim:
                    await _container.CreateItemAsync(vehicleClaim, new PartitionKey(vehicleClaim.Id.ToString()));
                    break;
                case TravelClaim travelClaim:
                    await _container.CreateItemAsync(travelClaim, new PartitionKey(travelClaim.Id.ToString()));
                    break;
                default:
                    throw new Exception("Unknown claim type");
            }

            return Result.Ok(claim);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CosmosDbClaimRepository.Save] {ex}");
            throw;
        }
    }

    public async Task<Result<Claim>> UpdateClaim(Claim claim)
    {
        try
        {
            var response = await _container.ReplaceItemAsync(claim, claim.Id.ToString(), new PartitionKey(claim.Id.ToString()));
            var updatedClaim = response.Resource;

            return Result.Ok(updatedClaim);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return Result.Fail<Claim>(ErrorMessages.ErrorMessage_ClaimNotFound);
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
            await _container.DeleteItemAsync<JObject>(id.ToString(), new PartitionKey(id.ToString()));
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