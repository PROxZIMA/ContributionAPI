# Contributing to ContributionAPI

First off, thank you for considering contributing to ContributionAPI! 🎉 

It's people like you that make ContributionAPI such a great tool for developers worldwide. This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Submitting Changes](#submitting-changes)
- [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

### Ways to Contribute

There are many ways you can contribute to ContributionAPI:

- **Report Bugs**: Use our [bug report template](.github/ISSUE_TEMPLATE/bug_report.md)
- **Suggest Features**: Use our [feature request template](.github/ISSUE_TEMPLATE/feature_request.md)
- **Improve Documentation**: Help make our docs clearer and more comprehensive
- **Submit Code**: Fix bugs, implement features, or improve performance
- **Write Tests**: Help improve our test coverage
- **Review Pull Requests**: Help review code changes from other contributors

### Prerequisites

Before contributing, ensure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) for version control
- A Firebase project (for testing integrations)
- Google Cloud project with Secret Manager enabled
- Familiarity with C# and ASP.NET Core

### Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/ContributionAPI.git
   cd ContributionAPI
   ```

2. **Set up Development Environment**
   ```bash
   # Restore dependencies
   dotnet restore
   
   # Build the solution
   dotnet build
   ```

3. **Configure Local Settings**
   ```bash
   # Copy development configuration
   cp src/Contribution.Hub/appsettings.Development.json.example src/Contribution.Hub/appsettings.Development.json
   
   # Edit configuration with your test Firebase/GCP settings
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Start Development Services**
   ```bash
   # Option 1: Use VS Code tasks (recommended)
   # Open in VS Code and use "Run Contribution API" task
   
   # Option 2: Manual startup
   cd src/Contribution.Hub && dotnet run --urls "http://localhost:5000"
   cd src/Contribution.GitHub && dotnet run --urls "http://localhost:5001"
   cd src/Contribution.AzureDevOps && dotnet run --urls "http://localhost:5002"
   ```

## Development Process

### Branching Strategy

We use a simplified Git flow:

- **`main`**: Production-ready code
- **`develop`**: Integration branch for features
- **`feature/feature-name`**: Feature development branches
- **`hotfix/fix-name`**: Critical bug fixes
- **`release/version`**: Release preparation branches

### Workflow

1. **Create an Issue**: Before starting work, create or find an existing issue
2. **Create a Branch**: Branch from `develop` for features, `main` for hotfixes
3. **Develop**: Make your changes following our coding standards
4. **Test**: Ensure all tests pass and add new tests for your changes
5. **Document**: Update documentation as needed
6. **Submit PR**: Create a pull request using our template
7. **Review**: Address feedback from code reviewers
8. **Merge**: Once approved, your changes will be merged

### Branch Naming

Use descriptive branch names:
- `feature/github-rate-limiting`
- `bugfix/azure-token-refresh`
- `hotfix/critical-security-patch`
- `docs/api-documentation-update`

## Coding Standards

### C# Style Guidelines

We follow Microsoft's C# coding conventions with some project-specific additions:

#### Naming Conventions
```csharp
// Classes: PascalCase
public class ContributionManager { }

// Methods: PascalCase
public async Task<ContributionsResponse> GetContributionsAsync() { }

// Properties: PascalCase
public string UserId { get; set; }

// Fields: camelCase with underscore prefix for private
private readonly ILogger _logger;

// Constants: PascalCase
public const string DefaultProvider = "github";

// Interfaces: PascalCase with 'I' prefix
public interface IContributionProvider { }
```

#### File Organization
```csharp
// File structure order:
using System;                    // System namespaces first
using Microsoft.AspNetCore.Mvc;  // Microsoft namespaces
using Contribution.Common;       // Project namespaces

namespace Contribution.Hub.Controllers;  // Namespace

public class ContributionsController : ControllerBase  // Class declaration
{
    // Constants
    private const string CachePrefix = "contributions";
    
    // Fields
    private readonly ILogger<ContributionsController> _logger;
    
    // Constructor
    public ContributionsController(ILogger<ContributionsController> logger)
    {
        _logger = logger;
    }
    
    // Properties
    public string ApiVersion { get; } = "v1.0";
    
    // Methods (public first, then private)
    public async Task<IActionResult> GetAsync() { }
    
    private bool ValidateRequest() { }
}
```

#### Code Patterns

**Dependency Injection:**
```csharp
// Use constructor injection
public class ContributionManager(
    IContributionRepository repository,
    ILogger<ContributionManager> logger)
{
    private readonly IContributionRepository _repository = repository;
    private readonly ILogger<ContributionManager> _logger = logger;
}
```

**Async/Await:**
```csharp
// Always use ConfigureAwait(false) in libraries
public async Task<ContributionsResponse> GetContributionsAsync()
{
    var result = await _httpClient.GetAsync(url).ConfigureAwait(false);
    return await ProcessResponse(result).ConfigureAwait(false);
}
```

**Error Handling:**
```csharp
// Use specific exception types
public async Task<ContributionsResponse> GetContributionsAsync()
{
    try
    {
        return await FetchContributions().ConfigureAwait(false);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to fetch contributions for user {UserId}", userId);
        throw new ContributionServiceException("Failed to fetch contributions", ex);
    }
}
```

### Project Structure Standards

#### Service Registration
```csharp
// Register services in logical groups
// Core services
builder.Services.AddScoped<IContributionAggregatorManager, ContributionAggregatorManager>();
builder.Services.AddScoped<IUserDataRepository, UserDataRepository>();

// External service clients
builder.Services.AddHttpClient<IContributionServiceClient, ContributionServiceClient>();

// Configuration
builder.Services.Configure<HubOptions>(builder.Configuration.GetSection(HubOptions.SectionName));
```

#### Configuration Management
```csharp
// Use strongly-typed configuration
public class HubOptions
{
    public const string SectionName = "Hub";
    
    public string FirebaseProjectId { get; set; } = string.Empty;
    public string SecretManagerProject { get; set; } = string.Empty;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(30);
}
```

### Documentation Standards

#### XML Documentation
```csharp
/// <summary>
/// Aggregates contributions from multiple providers for a specific user and year.
/// </summary>
/// <param name="userId">The unique identifier for the user.</param>
/// <param name="year">The year for which to retrieve contributions.</param>
/// <param name="providers">Optional list of specific providers to query.</param>
/// <returns>A task representing the asynchronous operation with aggregated contributions.</returns>
/// <exception cref="ArgumentException">Thrown when userId is null or empty.</exception>
/// <exception cref="ContributionServiceException">Thrown when contribution aggregation fails.</exception>
public async Task<ContributionsResponse> AggregateContributionsAsync(
    string userId, 
    int year, 
    string[]? providers = null)
```

#### Code Comments
```csharp
// Use comments for business logic, not obvious code
public async Task<ContributionsResponse> ProcessContributions()
{
    // Apply rate limiting to prevent API quota exhaustion
    await _rateLimiter.WaitAsync().ConfigureAwait(false);
    
    // Fetch from cache first to reduce external API calls
    var cached = await _cache.GetAsync(cacheKey).ConfigureAwait(false);
    if (cached != null)
    {
        return cached;
    }
    
    // Cache miss - fetch from external provider
    var fresh = await FetchFromProvider().ConfigureAwait(false);
    
    // Cache the result with sliding expiration
    await _cache.SetAsync(cacheKey, fresh, TimeSpan.FromMinutes(30)).ConfigureAwait(false);
    
    return fresh;
}
```

## Testing Guidelines

### Test Structure

We use a three-tier testing approach:

#### Unit Tests
- Test individual components in isolation
- Mock external dependencies
- Fast execution (< 100ms per test)
- High coverage (aim for 80%+ code coverage)

```csharp
[Fact]
public async Task GetContributionsAsync_ValidUser_ReturnsContributions()
{
    // Arrange
    var mockRepo = new Mock<IContributionRepository>();
    mockRepo.Setup(x => x.GetUserDataAsync("user123"))
           .ReturnsAsync(new UserData { UserId = "user123" });
    
    var manager = new ContributionManager(mockRepo.Object);
    
    // Act
    var result = await manager.GetContributionsAsync("user123", 2024);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("user123", result.UserId);
    Assert.Equal(2024, result.Year);
}
```

#### Integration Tests
- Test component interactions
- Use test containers for external dependencies
- Verify end-to-end scenarios

```csharp
[Collection("Integration")]
public class ContributionHubIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetContributions_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/contributions?userId=test&year=2024");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ContributionsResponse>(content);
        Assert.NotNull(result);
    }
}
```

#### Performance Tests
- Verify response times under load
- Test memory usage and resource cleanup
- Use BenchmarkDotNet for micro-benchmarks

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ContributionManagerBenchmarks
{
    [Benchmark]
    public async Task GetContributions_Benchmark()
    {
        var manager = new ContributionManager(/* dependencies */);
        await manager.GetContributionsAsync("user123", 2024);
    }
}
```

### Test Organization

```
tests/
├── Contribution.Hub.Tests/              # Unit tests for Hub
│   ├── Controllers/
│   ├── Managers/
│   └── Services/
├── Contribution.GitHub.Tests/           # Unit tests for GitHub service
├── Contribution.AzureDevOps.Tests/     # Unit tests for Azure service
├── Contribution.Common.Tests/           # Unit tests for shared library
├── Integration.Tests/                   # Integration tests
│   ├── Api/
│   └── Services/
└── Performance.Tests/                   # Performance benchmarks
```

### Test Naming

Use descriptive test names that explain the scenario:

```csharp
// MethodName_Scenario_ExpectedResult
[Fact]
public void GetContributions_UserNotFound_ThrowsUserNotFoundException() { }

[Fact] 
public void GetContributions_ValidUser_ReturnsContributionsWithCorrectTotal() { }

[Fact]
public void GetContributions_RateLimitExceeded_RetriesWithExponentialBackoff() { }
```

## Submitting Changes

### Pull Request Process

1. **Update Documentation**: Ensure README, API docs, and code comments are updated
2. **Add/Update Tests**: Include tests for new functionality or bug fixes
3. **Update Changelog**: Add entry to CHANGELOG.md (if applicable)
4. **Run Full Test Suite**: Ensure all tests pass
5. **Check Code Style**: Run linting and formatting tools
6. **Create PR**: Use our [pull request template](.github/pull_request_template.md)

### Pull Request Guidelines

**Title Format:**
- `feat: add GitHub rate limiting support`
- `fix: resolve Azure DevOps token refresh issue`
- `docs: update API documentation`
- `test: add integration tests for Hub service`
- `refactor: optimize contribution aggregation logic`

**Description Requirements:**
- Clear description of changes
- Link to related issues
- Breaking changes clearly marked
- Testing steps included
- Screenshots (if UI changes)

### Code Review Process

1. **Automated Checks**: CI/CD pipeline runs automatically
2. **Maintainer Review**: Core team reviews for design and implementation
3. **Community Review**: Other contributors may provide feedback
4. **Address Feedback**: Make requested changes
5. **Final Approval**: Maintainer approves and merges

### Review Criteria

Reviewers will check for:
- **Functionality**: Does it work as intended?
- **Code Quality**: Is it readable and maintainable?
- **Performance**: Are there any performance implications?
- **Security**: Are there any security concerns?
- **Testing**: Is there adequate test coverage?
- **Documentation**: Is documentation updated?

## Community

### Communication Channels

- **GitHub Issues**: Bug reports, feature requests
- **GitHub Discussions**: General questions, ideas
- **Pull Requests**: Code review and collaboration

### Getting Help

If you need help:

1. Check existing [documentation](README.md)
2. Search [existing issues](https://github.com/PROxZIMA/ContributionAPI/issues)
3. Create a new issue with detailed information
4. Join [GitHub Discussions](https://github.com/PROxZIMA/ContributionAPI/discussions)

### Recognition

Contributors are recognized through:
- GitHub contributor graphs
- CHANGELOG.md mentions
- Special recognition for significant contributions

## Good First Issues

Looking for something to work on? Check out issues labeled:
- `good first issue`: Perfect for newcomers
- `help wanted`: We need community help
- `documentation`: Improve our docs
- `testing`: Help improve test coverage

## Additional Resources

- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [GitHub Flow](https://guides.github.com/introduction/flow/)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

Thank you for contributing to ContributionAPI!

Your contributions help make this project better for developers worldwide. If you have any questions about contributing, don't hesitate to ask in our [GitHub Discussions](https://github.com/PROxZIMA/ContributionAPI/discussions).