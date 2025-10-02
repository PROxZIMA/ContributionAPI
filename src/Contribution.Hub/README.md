# Contribution.Hub

The Contribution.Hub is an aggregation service that combines contribution data from multiple providers (Azure DevOps and GitHub) into a unified response. It uses Firebase for user data storage and Google Cloud Secret Manager for secure PAT token management.

## Features

- **Multi-Provider Aggregation**: Merges data from Azure DevOps and GitHub services into a single ContributionsResponse
- **Firebase Integration**: Stores user configuration data in Firestore
- **Secure Token Management**: Uses GCP Secret Manager for PAT tokens
- **Dictionary-Key Merging**: Intelligently merges Total and Breakdown dictionaries by key
- **Date-based Contribution Merging**: Combines contributions by date across providers
- **Provider-wise Error Tracking**: Tracks errors per provider in the Meta.Errors collection
- **Flexible Configuration**: Enable/disable individual providers per user
- **Error Resilience**: Continues aggregation even if one provider fails

## API Endpoint

### GET /contributions

Retrieves aggregated contributions for a user across all enabled providers.

**Query Parameters:**
- `userId` (required): Firebase user ID
- `year` (required): Year to fetch contributions for (YYYY format)
- `providers` (optional): Array of providers to include (e.g., `azuredevops`, `github`). If not specified, all available providers are used.
- `includeActivity` (optional): Include activity breakdown (default: false)
- `includeBreakdown` (optional): Include contribution type breakdown (default: false)

**Example Requests:**
```
GET /contributions?userId=firebase-user-123&year=2024&includeActivity=true&includeBreakdown=true
GET /contributions?userId=firebase-user-123&year=2024&providers=azuredevops&providers=github
GET /contributions?userId=firebase-user-123&year=2024&providers=github
```

**Example Response:**
```json
{
  "total": {
    "commits": 200,
    "pullRequests": 50,
    "workItems": 100
  },
  "contributions": [
    {
      "date": "2024-01-01",
      "count": 5,
      "level": 2,
      "activity": {
        "commits": 3,
        "pullRequests": 2
      }
    }
  ],
  "breakdown": {
    "commits": 200,
    "pullRequests": 50,
    "workItems": 100
  },
  "meta": {
    "scannedProjects": 10,
    "scannedRepos": 25,
    "elapsedMs": 1500,
    "cachedProjects": true,
    "errors": [
      "GitHub: Rate limit exceeded"
    ]
  }
}
```

## Configuration

### appsettings.json

```json
{
  "Hub": {
    "ServiceUrls": {
      "AzureDevOpsApiUrl": "https://localhost:7001",
      "GitHubApiUrl": "https://localhost:7002"
    },
    "Gcp": {
      "ProjectId": "your-gcp-project-id",
      "CredentialsPath": "/path/to/gcp/credentials.json",
      "Secrets": {
        "AzureDevOpsPat": "azure-devops-pat-secret-name",
        "GitHubPat": "github-pat-secret-name"
      }
    },
    "Firebase": {
      "ProjectId": "contribution-token-manager",
      "CollectionName": "credentials",
      "CredentialsPath": "/path/to/firebase/credentials.json"
    }
  }
}
```

## Firestore Database Structure

**Database**: `contribution-token-manager`  
**Collection**: `credentials`  
**Document**: `{userId}`

Each user document should have the following structure:

```json
{
  "userId": "firebase-user-123",
  "azure": {
    "email": "user@example.com",
    "organization": "myorg"
  },
  "github": {
    "username": "github-username"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-10-02T10:30:00Z"
}
```

**Note**: The `enabled` property has been removed. Provider availability is now controlled via query parameters.

## Setup Requirements

### Google Cloud Platform
1. Create a GCP project
2. Enable Firestore and Secret Manager APIs
3. Create service account with appropriate permissions
4. Store PAT tokens as secrets in Secret Manager

### Firebase
1. Create a Firebase project (can be the same as GCP)
2. Initialize Firestore database
3. Create user collection with user configuration data

### Azure DevOps & GitHub Services
1. Ensure both Contribution.AzureDevOps and Contribution.GitHub services are running
2. Update service URLs in configuration

## Authentication

The service uses the same authentication mechanism as other services in the solution:
- Supports Basic authentication with PAT tokens
- Supports Bearer tokens
- Uses the `CommonAuthenticationHandler` from `Contribution.Common`

## Dependencies

- **Google.Cloud.Firestore**: Firestore integration
- **Google.Cloud.SecretManager.V1**: Secret Manager integration
- **Contribution.Common**: Shared models and authentication
- **Microsoft.Extensions.Http**: HTTP client factory
- **Newtonsoft.Json**: JSON serialization

## Development

### Running the Service

```bash
dotnet run --project src/Contribution.Hub
```

The service will be available at:
- HTTPS: https://localhost:7003
- HTTP: http://localhost:5003
- Swagger UI: https://localhost:7003/swagger

### Building

```bash
dotnet build src/Contribution.Hub
```

### Testing

The service includes comprehensive error handling and logging. Check the logs for detailed information about:
- Firestore operations
- Secret Manager access
- External service calls
- Aggregation processes

## Architecture

```
┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│   Client    │───▶│ Contribution │───▶│  Firestore  │
│             │    │     Hub      │    │   (Users)   │
└─────────────┘    └──────────────┘    └─────────────┘
                           │
                           ├─────────────────────────┐
                           ▼                         ▼
                   ┌──────────────┐         ┌──────────────┐
                   │ GCP Secret   │         │   Provider   │
                   │   Manager    │         │   Factory    │
                   └──────────────┘         └──────────────┘
                                                   │
                                    ┌──────────────┼──────────────┐
                                    ▼              ▼              ▼
                              ┌───────────┐ ┌─────────┐ ┌─────────────┐
                              │ Azure     │ │ GitHub  │ │   Future    │
                              │ DevOps    │ │         │ │ Providers   │
                              └───────────┘ └─────────┘ └─────────────┘
```

### Factory Pattern

The service uses a factory pattern to manage contribution providers:

- **Provider Constants**: Lowercase string constants (`azuredevops`, `github`)
- **Provider Factory**: Centralized provider creation and management
- **Extensible Design**: Easy to add new providers without modifying existing code
- **Type Safety**: Constants prevent typos and provide IntelliSense support

For detailed information on extending the service with new providers, see [FACTORY_PATTERN.md](../../../FACTORY_PATTERN.md).

## Data Merging Strategy

The service implements intelligent merging of ContributionsResponse objects:

### Dictionary Merging
- **Total**: Keys are merged with values summed (e.g., "commits": 50 + "commits": 30 = "commits": 80)
- **Breakdown**: Same key-wise merging as Total dictionary
- **Activity**: Nested dictionary merging within individual contribution entries

### Contributions Merging
- Contributions are grouped by date
- Counts are summed for the same date across providers
- Activity dictionaries are merged key-wise for each date
- Level is automatically recalculated based on the merged count

### Error Tracking
- Provider-specific errors are prefixed with provider name
- All errors from all providers are collected in Meta.Errors
- Failed providers don't prevent successful aggregation from other providers

### Meta Information
- ScannedProjects, ScannedRepos, ElapsedMs are summed across providers
- CachedProjects is true if any provider used cache
- Errors include both aggregation-level and provider-specific errors

## Error Handling

The service is designed to be resilient:
- If one provider fails, others continue to work
- Failed provider attempts are logged with error details and included in Meta.Errors
- Partial responses are returned with detailed error information per provider
- Firestore and Secret Manager errors are properly handled and logged
- Returns standard ContributionsResponse format even with partial failures

### Provider Not Found Errors

When a requested provider is not configured in the user's Firestore document:

```json
{
  "total": {},
  "contributions": [],
  "breakdown": null,
  "meta": {
    "errors": [
      "azuredevops: Provider 'azuredevops' not found in user data"
    ]
  }
}
```

### Partial Success Scenarios

If Azure DevOps data is available but GitHub is not configured:

```
GET /contributions?userId=user-123&year=2024&providers=azuredevops&providers=github
```

Response includes Azure DevOps data with GitHub error:
```json
{
  "total": {
    "2024": 150
  },
  "contributions": [...],
  "breakdown": {
    "commits": 100,
    "workitems": 50
  },
  "meta": {
    "errors": [
      "github: Provider 'github' not found in user data"
    ]
  }
}
```