using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ASP.Claims.API.Application.Services;

/// <summary>
/// Seeds initial data into Cosmos DB for development/testing
/// </summary>
public class CosmosDbSeeder(IClaimRepository claimRepository, IUserRepository userRepository, ILogger<CosmosDbSeeder> logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IClaimRepository _claimRepository = claimRepository;
    private readonly ILogger<CosmosDbSeeder> _logger = logger;

    /// <summary>
    /// Seeds default users and sample claims for development
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Cosmos DB seeding...");

        try
        {
            await SeedUsersAsync(cancellationToken);
            await SeedClaimsAsync(cancellationToken);

            _logger.LogInformation("Cosmos DB seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding Cosmos DB");
            throw;
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var defaultUsers = new[]
        {
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "testuser",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                Role = Role.Admin
            },
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("AdminPass123!"),
                Role = Role.Admin
            },
            new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "user",
                Password = BCrypt.Net.BCrypt.HashPassword("UserPass123!"),
                Role = Role.User
            }
        };

        foreach (var user in defaultUsers)
        {
            // Check if user already exists
            var existing = await _userRepository.GetByUsernameAsync(user.Username);
            if (existing == null)
            {
                var result = await _userRepository.SaveAsync(user);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Seeded user: {Username} (Role: {Role})", user.Username, user.Role);
                }
                else
                {
                    _logger.LogWarning("Failed to seed user {Username}: {Error}", user.Username, result.Errors[0].Message);
                }
            }
            else
            {
                _logger.LogInformation("User {Username} already exists, skipping", user.Username);
            }
        }
    }

    private async Task SeedClaimsAsync(CancellationToken cancellationToken)
    {
        // Check if any claims exist (to avoid re-seeding)
        var existingClaims = await _claimRepository.GetAll();
        if (existingClaims.Any())
        {
            _logger.LogInformation("Claims already exist in database ({Count} claims found), skipping claim seeding", existingClaims.Count());
            return;
        }

        var sampleClaims = new List<Claim>
        {
            // Property Claims
            new PropertyClaim
            {
                Id = Guid.NewGuid(),
                Description = "Kitchen fire caused by electrical fault",
                PropertyDamageType = PropertyDamageType.Fire,
                Address = "123 Main Street, Stockholm",
                EstimatedDamageCost = 150000m,
                ReportedDate = DateTime.UtcNow.AddDays(-10),
                Status = ClaimStatus.RequiresManualReview
            },
            new PropertyClaim
            {
                Id = Guid.NewGuid(),
                Description = "Burst pipe flooded basement",
                PropertyDamageType = PropertyDamageType.Water,
                Address = "456 Oak Avenue, Gothenburg",
                EstimatedDamageCost = 85000m,
                ReportedDate = DateTime.UtcNow.AddDays(-5),
                Status = ClaimStatus.None
            },
            new PropertyClaim
            {
                Id = Guid.NewGuid(),
                Description = "Jewelry stolen during break-in",
                PropertyDamageType = PropertyDamageType.Theft,
                Address = "789 Pine Road, Malm�",
                EstimatedDamageCost = 250000m,
                ReportedDate = DateTime.UtcNow.AddDays(-3),
                Status = ClaimStatus.Escalated
            },

            // Travel Claims
            new TravelClaim
            {
                Id = Guid.NewGuid(),
                Description = "Flight cancelled due to weather, missed hotel reservations",
                Country = Country.France,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-23),
                IncidentType = IncidentType.Cancellation,
                ReportedDate = DateTime.UtcNow.AddDays(-20),
                Status = ClaimStatus.None
            },
            new TravelClaim
            {
                Id = Guid.NewGuid(),
                Description = "Luggage lost by airline",
                Country = Country.Japan,
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow.AddDays(-8),
                IncidentType = IncidentType.LostLuggage,
                ReportedDate = DateTime.UtcNow.AddDays(-7),
                Status = ClaimStatus.RequiresManualReview
            },
            new TravelClaim
            {
                Id = Guid.NewGuid(),
                Description = "Medical emergency requiring hospitalization",
                Country = Country.Spain,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-15),
                IncidentType = IncidentType.Medical,
                ReportedDate = DateTime.UtcNow.AddDays(-14),
                Status = ClaimStatus.FraudCheck
            },

            // Vehicle Claims
            new VehicleClaim
            {
                Id = Guid.NewGuid(),
                Description = "Rear-ended at traffic light",
                RegistrationNumber = "ABC123",
                PlaceOfAccident = "E4 Highway, Stockholm",
                ReportedDate = DateTime.UtcNow.AddDays(-12),
                Status = ClaimStatus.None
            },
            new VehicleClaim
            {
                Id = Guid.NewGuid(),
                Description = "Hit parked car in parking lot",
                RegistrationNumber = "XYZ789",
                PlaceOfAccident = "Ikea Parking, Gothenburg",
                ReportedDate = DateTime.UtcNow.AddDays(-8),
                Status = ClaimStatus.RequiresManualReview
            },
            new VehicleClaim
            {
                Id = Guid.NewGuid(),
                Description = "Windshield cracked by road debris",
                RegistrationNumber = "DEF456",
                PlaceOfAccident = "Route 55, Malm�",
                ReportedDate = DateTime.UtcNow.AddDays(-4),
                Status = ClaimStatus.Escalated
            }
        };

        foreach (var claim in sampleClaims)
        {
            var result = await _claimRepository.Save(claim);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Seeded {ClaimType} claim: {Description} (Status: {Status})", 
                    claim.Type, claim.Description.Substring(0, Math.Min(30, claim.Description.Length)) + "...", claim.Status);
            }
            else
            {
                _logger.LogWarning("Failed to seed claim {Description}: {Error}", 
                    claim.Description, result.Errors[0].Message);
            }
        }

        _logger.LogInformation("Seeded {Count} sample claims", sampleClaims.Count);
    }
}
