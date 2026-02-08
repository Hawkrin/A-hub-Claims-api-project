using ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using AutoMapper;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace ASP.Claims.API.Test.HandlerTests.LoggingTests;

public class TravelClaimHandlerLoggingTests
{
    private readonly Mock<IClaimRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IClaimEventPublisher> _mockEventPublisher;

    public TravelClaimHandlerLoggingTests()
    {
        _mockRepository = new Mock<IClaimRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockEventPublisher = new Mock<IClaimEventPublisher>();
    }

    [Fact]
    public async Task CreateHandler_LogsInformation_WhenClaimCreated()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreateTravelClaimHandler>();
        var command = new CreateTravelClaimCommand(
            Country: Country.Spain,
            StartDate: new DateTime(2024, 1, 1),
            EndDate: new DateTime(2024, 1, 15),
            IncidentType: IncidentType.Medical,
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new TravelClaim
        {
            Id = Guid.NewGuid(),
            Country = Country.Spain,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 15),
            IncidentType = IncidentType.Medical,
            Status = ClaimStatus.None
        };

        _mockMapper.Setup(m => m.Map<TravelClaim>(command)).Returns(claim);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Ok());

        var handler = new CreateTravelClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Travel claim created") &&
            log.Message.Contains("Spain") &&
            log.Message.Contains("Medical"));
    }

    [Fact]
    public async Task CreateHandler_LogsError_WhenSaveFails()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreateTravelClaimHandler>();
        var command = new CreateTravelClaimCommand(
            Country: Country.Spain,
            StartDate: new DateTime(2024, 1, 1),
            EndDate: new DateTime(2024, 1, 15),
            IncidentType: IncidentType.Medical,
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new TravelClaim { Id = Guid.NewGuid() };

        _mockMapper.Setup(m => m.Map<TravelClaim>(command)).Returns(claim);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Fail("Database error"));

        var handler = new CreateTravelClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Error && 
            log.Message.Contains("Failed to create travel claim"));
    }

    [Fact]
    public async Task UpdateHandler_LogsInformation_WhenStatusChanges()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdateTravelClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdateTravelClaimCommand(
            Id: claimId,
            Country: Country.Spain,
            StartDate: new DateTime(2024, 1, 1),
            EndDate: new DateTime(2024, 1, 15),
            IncidentType: IncidentType.Medical,
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        var existingClaim = new TravelClaim
        {
            Id = claimId,
            Status = ClaimStatus.RequiresManualReview
        };

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync(existingClaim);
        _mockMapper.Setup(m => m.Map(command, existingClaim)).Callback(() => {
            existingClaim.Status = ClaimStatus.None; // Simulate the mapping changing the status
        }).Returns(existingClaim);
        _mockRepository.Setup(r => r.UpdateClaim(existingClaim)).ReturnsAsync(Result.Ok());

        var handler = new UpdateTravelClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Travel claim status changed") &&
            log.Message.Contains("RequiresManualReview") &&
            log.Message.Contains("None"));
    }

    [Fact]
    public async Task UpdateHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdateTravelClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdateTravelClaimCommand(
            Id: claimId,
            Country: Country.Spain,
            StartDate: new DateTime(2024, 1, 1),
            EndDate: new DateTime(2024, 1, 15),
            IncidentType: IncidentType.Medical,
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new UpdateTravelClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Attempted to update non-existent travel claim"));
    }

    [Fact]
    public async Task DeleteHandler_LogsInformation_WhenDeleteSucceeds()
    {
        // Arrange
        var fakeLogger = new FakeLogger<DeletePropertyTravelHandler>();
        var claimId = Guid.NewGuid();
        var command = new DeleteTravelClaimCommand(claimId);

        _mockRepository.Setup(r => r.DeleteClaim(claimId)).ReturnsAsync(Result.Ok());

        var handler = new DeletePropertyTravelHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Travel claim deleted"));
    }

    [Fact]
    public async Task QueryHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<TravelClaimQueryHandler>();
        var claimId = Guid.NewGuid();
        var query = new GetTravelClaimByIdQuery(claimId);

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new TravelClaimQueryHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Travel claim not found"));
    }
}
