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

public class PropertyClaimHandlerLoggingTests
{
    private readonly Mock<IClaimRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IClaimStatusEvaluator> _mockStatusEvaluator;
    private readonly Mock<IClaimEventPublisher> _mockEventPublisher;

    public PropertyClaimHandlerLoggingTests()
    {
        _mockRepository = new Mock<IClaimRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockStatusEvaluator = new Mock<IClaimStatusEvaluator>();
        _mockEventPublisher = new Mock<IClaimEventPublisher>();
    }

    #region Create Handler Tests

    [Fact]
    public async Task CreateHandler_LogsInformation_WhenStandardClaimCreated()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreatePropertyClaimHandler>();
        var command = new CreatePropertyClaimCommand(
            Address: "123 Main St",
            PropertyDamageType: PropertyDamageType.Fire,
            EstimatedDamageCost: 5000,
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new PropertyClaim
        {
            Id = Guid.NewGuid(),
            Address = "123 Main St",
            EstimatedDamageCost = 5000,
            Status = ClaimStatus.None
        };

        _mockMapper.Setup(m => m.Map<PropertyClaim>(command)).Returns(claim);
        _mockRepository.Setup(r => r.GetByType(ClaimType.Property)).ReturnsAsync([]);
        _mockStatusEvaluator.Setup(e => e.Evaluate(claim, It.IsAny<IEnumerable<Claim>>())).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Ok());

        var handler = new CreatePropertyClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Property claim created") &&
            log.Message.Contains(claim.Id.ToString()));
    }

    [Fact]
    public async Task CreateHandler_LogsError_WhenSaveFails()
    {
        // Arrange
        var fakeLogger = new FakeLogger<CreatePropertyClaimHandler>();
        var command = new CreatePropertyClaimCommand(
            Address: "123 Main St",
            PropertyDamageType: PropertyDamageType.Fire,
            EstimatedDamageCost: 5000,
            ReportedDate: DateTime.UtcNow,
            Description: "Test claim"
        );

        var claim = new PropertyClaim { Id = Guid.NewGuid() };

        _mockMapper.Setup(m => m.Map<PropertyClaim>(command)).Returns(claim);
        _mockRepository.Setup(r => r.GetByType(ClaimType.Property)).ReturnsAsync([]);
        _mockStatusEvaluator.Setup(e => e.Evaluate(claim, It.IsAny<IEnumerable<Claim>>())).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.Save(claim)).ReturnsAsync(Result.Fail("Database error"));

        var handler = new CreatePropertyClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Error && 
            log.Message.Contains("Failed to create property claim"));
    }

    #endregion

    #region Update Handler Tests

    [Fact]
    public async Task UpdateHandler_LogsInformation_WhenStatusChanges()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdatePropertyClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdatePropertyClaimCommand(
            Id: claimId,
            Address: "123 Main St",
            PropertyDamageType: PropertyDamageType.Fire,
            EstimatedDamageCost: 5000,
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        var existingClaim = new PropertyClaim
        {
            Id = claimId,
            Status = ClaimStatus.RequiresManualReview // Old status
        };

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync(existingClaim);
        _mockMapper.Setup(m => m.Map(command, existingClaim)).Returns(existingClaim);
        _mockRepository.Setup(r => r.GetByType(ClaimType.Property)).ReturnsAsync([]);
        _mockStatusEvaluator.Setup(e => e.Evaluate(existingClaim, It.IsAny<IEnumerable<Claim>>()))
            .Returns(ClaimStatus.None); // New status
        _mockRepository.Setup(r => r.UpdateClaim(existingClaim)).ReturnsAsync(Result.Ok());

        var handler = new UpdatePropertyClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Property claim status changed") &&
            log.Message.Contains("RequiresManualReview") &&
            log.Message.Contains("None"));
    }

    [Fact]
    public async Task UpdateHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdatePropertyClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdatePropertyClaimCommand(
            Id: claimId,
            Address: "123 Main St",
            PropertyDamageType: PropertyDamageType.Fire,
            EstimatedDamageCost: 5000,
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new UpdatePropertyClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Attempted to update non-existent property claim"));
    }

    [Fact]
    public async Task UpdateHandler_LogsError_WhenUpdateFails()
    {
        // Arrange
        var fakeLogger = new FakeLogger<UpdatePropertyClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new UpdatePropertyClaimCommand(
            Id: claimId,
            Address: "123 Main St",
            PropertyDamageType: PropertyDamageType.Fire,
            EstimatedDamageCost: 5000,
            ReportedDate: DateTime.UtcNow,
            Description: "Updated claim",
            Status: ClaimStatus.None
        );

        var existingClaim = new PropertyClaim { Id = claimId, Status = ClaimStatus.RequiresManualReview };

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync(existingClaim);
        _mockMapper.Setup(m => m.Map(command, existingClaim)).Returns(existingClaim);
        _mockRepository.Setup(r => r.GetByType(ClaimType.Property)).ReturnsAsync([]);
        _mockStatusEvaluator.Setup(e => e.Evaluate(existingClaim, It.IsAny<IEnumerable<Claim>>())).Returns(ClaimStatus.None);
        _mockRepository.Setup(r => r.UpdateClaim(existingClaim)).ReturnsAsync(Result.Fail("Database error"));

        var handler = new UpdatePropertyClaimHandler(_mockRepository.Object, _mockMapper.Object, _mockStatusEvaluator.Object, _mockEventPublisher.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Error && 
            log.Message.Contains("Failed to update property claim"));
    }

    #endregion

    #region Delete Handler Tests

    [Fact]
    public async Task DeleteHandler_LogsInformation_WhenDeleteSucceeds()
    {
        // Arrange
        var fakeLogger = new FakeLogger<DeletePropertyClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new DeletePropertyClaimCommand(claimId);

        _mockRepository.Setup(r => r.DeleteClaim(claimId)).ReturnsAsync(Result.Ok());

        var handler = new DeletePropertyClaimHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Property claim deleted"));
    }

    [Fact]
    public async Task DeleteHandler_LogsWarning_WhenDeleteFails()
    {
        // Arrange
        var fakeLogger = new FakeLogger<DeletePropertyClaimHandler>();
        var claimId = Guid.NewGuid();
        var command = new DeletePropertyClaimCommand(claimId);

        _mockRepository.Setup(r => r.DeleteClaim(claimId)).ReturnsAsync(Result.Fail("Claim not found"));

        var handler = new DeletePropertyClaimHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Failed to delete property claim"));
    }

    #endregion

    #region Query Handler Tests

    [Fact]
    public async Task QueryHandler_LogsWarning_WhenClaimNotFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<PropertyClaimQueryHandler>();
        var claimId = Guid.NewGuid();
        var query = new GetPropertyClaimByIdQuery(claimId);

        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync((Claim?)null);

        var handler = new PropertyClaimQueryHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Contains(logEntries, log => 
            log.Level == LogLevel.Warning && 
            log.Message.Contains("Property claim not found"));
    }

    [Fact]
    public async Task QueryHandler_DoesNotLog_WhenClaimFound()
    {
        // Arrange
        var fakeLogger = new FakeLogger<PropertyClaimQueryHandler>();
        var claimId = Guid.NewGuid();
        var query = new GetPropertyClaimByIdQuery(claimId);

        var claim = new PropertyClaim { Id = claimId };
        _mockRepository.Setup(r => r.GetById(claimId)).ReturnsAsync(claim);

        var handler = new PropertyClaimQueryHandler(_mockRepository.Object, fakeLogger);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert - No logs should be generated for successful queries
        var logEntries = fakeLogger.Collector.GetSnapshot();
        Assert.Empty(logEntries);
    }

    #endregion
}
