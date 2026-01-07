using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Infrastructures.Repositories;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Xunit;

namespace ASP.Claims.API.Test.RepositoryTests;

public class CosmosDbClaimRepositoryTests
{
    [Fact]
    public void DeserializeClaim_PropertyClaim_Works()
    {
        var guid = Guid.NewGuid();
        var json = JObject.FromObject(new { ClaimType = 1, Id = guid, PropertySpecific = "foo" });

        var claim = InvokeDeserializeClaim(json);

        Assert.IsType<PropertyClaim>(claim);
        Assert.Equal(guid, claim.Id);
    }

    [Fact]
    public void DeserializeClaim_TravelClaim_Works()
    {
        var guid = Guid.NewGuid();
        var json = JObject.FromObject(new { ClaimType = 2, Id = guid, VehicleSpecific = "bar" });

        var claim = InvokeDeserializeClaim(json);

        Assert.IsType<TravelClaim>(claim);
        Assert.Equal(guid, claim.Id);
    }

    [Fact]
    public void DeserializeClaim_VehicleClaim_Works()
    {
        var guid = Guid.NewGuid();
        var json = JObject.FromObject(new { ClaimType = 0, Id = guid, TravelSpecific = "baz" });

        var claim = InvokeDeserializeClaim(json);

        Assert.IsType<VehicleClaim>(claim);
        Assert.Equal(guid, claim.Id);
    }

    [Fact]
    public void DeserializeClaim_Throws_WhenTypeMissing()
    {
        var json = JObject.FromObject(new { Id = Guid.NewGuid() });
        var ex = Assert.Throws<TargetInvocationException>(() => InvokeDeserializeClaim(json));
        Assert.IsType<Exception>(ex.InnerException);
        Assert.Contains("Type discriminator missing", ex.InnerException.Message);
    }

    [Fact]
    public void DeserializeClaim_Throws_WhenTypeUnknown()
    {
        var json = JObject.FromObject(new { Type = 99, Id = Guid.NewGuid() });
        var ex = Assert.Throws<TargetInvocationException>(() => InvokeDeserializeClaim(json));
        Assert.IsType<Exception>(ex.InnerException);
    }

    private static Claim? InvokeDeserializeClaim(JObject json)
    {
        var method = typeof(CosmosDbClaimRepository)
            .GetMethod("DeserializeClaim", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return (Claim?)method!.Invoke(null, [json]);
    }
}
