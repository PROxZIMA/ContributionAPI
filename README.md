<a id="readme-top"></a>

<div align="center">
  <img src="assets/logo.png" alt="ContributionAPI Logo" width="200">
  <h3>Contribution Hub API</h3>
  <p>
    <strong>A unified API for aggregating developer contributions across multiple platforms</strong>
  </p>

  <!-- Badges -->
  <p>
    <a href="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.Hub.yaml">
      <img src="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.Hub.yaml/badge.svg" alt="Hub Contribution API">
    </a>
    <a href="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.AzureDevOps.yaml">
      <img src="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.AzureDevOps.yaml/badge.svg" alt="AzureDevOps Contribution API">
    </a>
    <a href="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.GitHub.yaml">
      <img src="https://github.com/PROxZIMA/ContributionAPI/actions/workflows/Contribution.GitHub.yaml/badge.svg" alt="GitHub Contribution API">
    </a>
    <a href="LICENSE">
      <img src="https://img.shields.io/github/license/PROxZIMA/ContributionAPI" alt="License">
    </a>
    <!-- <a href="https://github.com/PROxZIMA/ContributionAPI/network/members">
      <img src="https://img.shields.io/github/forks/PROxZIMA/ContributionAPI" alt="Forks">
    </a>
    <a href="https://github.com/PROxZIMA/ContributionAPI/stargazers">
      <img src="https://img.shields.io/github/stars/PROxZIMA/ContributionAPI" alt="Stars">
    </a> -->
  </p>
</div>

## Table of Contents

- [About The Project](#about-the-project)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
- [Usage](#usage)
  - [Authentication](#authentication)
  - [API Endpoints](#api-endpoints)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## About The Project

ContributionAPI is a powerful microservices-based platform that aggregates developer contributions from multiple sources including GitHub, Azure DevOps, and other development platforms. It provides a unified API interface to retrieve comprehensive contribution data, enabling developers and organizations to get a holistic view of coding activity across different platforms.

### Key Features

- **Multi-Platform Aggregation**: Seamlessly combines contributions from GitHub, Azure DevOps, and more
- **Microservices Architecture**: Scalable and maintainable service-oriented design
- **Firebase Integration**: Secure user data storage and management
- **Secret Management**: Secure API key and token management
- **Rich Data**: Detailed contribution breakdowns including commits, pull requests, and work items
- **High Performance**: Optimized caching and concurrent data fetching
- **Secure**: Token-based authentication with read-only access patterns

### Built With

* [![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/en-us/aspnet/core/)
* [![Firebase Firestore](https://img.shields.io/badge/Firebase_Firestore-Database-FFCA28?style=for-the-badge&logo=firebase&logoColor=white)](https://firebase.google.com/products/firestore)
* [![Google Secret Manager](https://img.shields.io/badge/Google_Secret_Manager-Security-4285F4?style=for-the-badge&logo=googlecloud&logoColor=white)](https://cloud.google.com/secret-manager)
* [![Azure DevOps SDK](https://img.shields.io/badge/Azure_DevOps-REST_SDK-0078D7?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOCAxOCI+PGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJhIiB4MT0iOSIgeTE9IjE2Ljk3IiB4Mj0iOSIgeTI9IjEuMDMiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIj48c3RvcCBvZmZzZXQ9IjAiIHN0b3AtY29sb3I9IiNmZmYiLz48c3RvcCBvZmZzZXQ9Ii4xNiIgc3RvcC1jb2xvcj0iI2ZmZiIvPjxzdG9wIG9mZnNldD0iLjUzIiBzdG9wLWNvbG9yPSIjZmZmIi8+PHN0b3Agb2Zmc2V0PSIuODIiIHN0b3AtY29sb3I9IiNmZmYiLz48c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiNmZmYiLz48L2xpbmVhckdyYWRpZW50PjwvZGVmcz48cGF0aCBkPSJNMTcgNHY5Ljc0bC00IDMuMjgtNi4yLTIuMjZWMTdsLTMuNTEtNC41OSAxMC4yMy44VjQuNDR6bS0zLjQxLjQ5TDcuODUgMXYyLjI5TDIuNTggNC44NCAxIDYuODd2NC42MWwyLjI2IDFWNi41N3oiIGZpbGw9InVybCgjYSkiLz48L3N2Zz4=&logoColor=white)](https://docs.microsoft.com/en-us/azure/devops/)
* [![GitHub API](https://img.shields.io/badge/GitHub_API-REST_GraphQL-181717?style=for-the-badge&logo=github&logoColor=white)](https://docs.github.com/en/rest)
* [![Azure Web Apps](https://img.shields.io/badge/Azure_Web_Apps-Hosting-0078D7?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOCAxOCI+PGRlZnM+PGxpbmVhckdyYWRpZW50IGlkPSJiIiB4MT0iNC40IiB5MT0iMTEuNDgiIHgyPSI0LjM3IiB5Mj0iNy41MyIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPjxzdG9wIG9mZnNldD0iMCIgc3RvcC1jb2xvcj0iI2ZmZiIvPjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI2ZjZmNmYyIvPjwvbGluZWFyR3JhZGllbnQ+PGxpbmVhckdyYWRpZW50IGlkPSJjIiB4MT0iMTAuMTMiIHkxPSIxNS40NSIgeDI9IjEwLjEzIiB5Mj0iMTEuOSIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPjxzdG9wIG9mZnNldD0iMCIgc3RvcC1jb2xvcj0iI2NjYyIvPjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI2ZjZmNmYyIvPjwvbGluZWFyR3JhZGllbnQ+PGxpbmVhckdyYWRpZW50IGlkPSJkIiB4MT0iMTQuMTgiIHkxPSIxMS4xNSIgeDI9IjE0LjE4IiB5Mj0iNy4zOCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPjxzdG9wIG9mZnNldD0iMCIgc3RvcC1jb2xvcj0iI2NjYyIvPjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI2ZjZmNmYyIvPjwvbGluZWFyR3JhZGllbnQ+PHJhZGlhbEdyYWRpZW50IGlkPSJhIiBjeD0iMTM0MjguODEiIGN5PSIzNTE4Ljg2IiByPSI1Ni42NyIgZ3JhZGllbnRUcmFuc2Zvcm09Im1hdHJpeCguMTUgMCAwIC4xNSAtMjAwNS4zMyAtNTE4LjgzKSIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPjwvcmFkaWFsR3JhZGllbnQ+PC9kZWZzPjxwYXRoIGQ9Ik0xNC4yMSAxNS43MkE4LjUgOC41IDAgMDEzLjc5IDIuMjhsLjA5LS4wNmE4LjUgOC41IDAgMDExMC4zMyAxMy41IiBmaWxsPSJ1cmwoI2EpIi8+PHBhdGggZD0iTTYuNjkgNy4yM2ExMyAxMyAwIDAxOC45MS0zLjU4IDguNDcgOC40NyAwIDAwLTEuNDktMS40NCAxNC4zNCAxNC4zNCAwIDAwLTQuNjkgMS4xIDEyLjU0IDEyLjU0IDAgMDAtNC4wOCAyLjgyIDIuNzYgMi43NiAwIDAxMS4zNSAxLjF6TTIuNDggMTAuNjVhMTcuODYgMTcuODYgMCAwMC0uODMgMi42MiA3LjgyIDcuODIgMCAwMC42Mi45MmMuMTguMjMuMzUuNDQuNTUuNjVhMTcuOTQgMTcuOTQgMCAwMTEuMDgtMy40NyAyLjc2IDIuNzYgMCAwMS0xLjQyLS43MnoiIGZpbGw9IiNmZmYiIG9wYWNpdHk9Ii42Ii8+PHBhdGggZD0iTTMuNDYgNi4xMWExMiAxMiAwIDAxLS42OS0yLjk0IDguMTUgOC4xNSAwIDAwLTEuMSAxLjQ1QTEyLjY5IDEyLjY5IDAgMDAyLjI0IDdhMi42OSAyLjY5IDAgMDExLjIyLS44OXoiIGZpbGw9IiNmMmYyZjIiIG9wYWNpdHk9Ii41NSIvPjxjaXJjbGUgY3g9IjQuMzgiIGN5PSI4LjY4IiByPSIyLjczIiBmaWxsPSJ1cmwoI2IpIi8+PHBhdGggZD0iTTguMzYgMTMuNjdhMS43NyAxLjc3IDAgMDEuNTQtMS4yNyAxMS44OCAxMS44OCAwIDAxLTIuNTMtMS44NiAyLjc0IDIuNzQgMCAwMS0xLjQ5LjgzIDEzLjEgMTMuMSAwIDAwMS40NSAxLjI4IDEyLjEyIDEyLjEyIDAgMDAyLjA1IDEuMjUgMS43OSAxLjc5IDAgMDEtLjAyLS4yM3pNMTQuNjYgMTMuODhhMTIgMTIgMCAwMS0yLjc2LS4zMi40MS40MSAwIDAxMCAuMTEgMS43NSAxLjc1IDAgMDEtLjUxIDEuMjQgMTMuNjkgMTMuNjkgMCAwMDMuNDIuMjRBOC4yMSA4LjIxIDAgMDAxNiAxMy44MWExMS41IDExLjUgMCAwMS0xLjM0LjA3eiIgZmlsbD0iI2YyZjJmMiIgb3BhY2l0eT0iLjU1Ii8+PGNpcmNsZSBjeD0iMTAuMTMiIGN5PSIxMy42NyIgcj0iMS43OCIgZmlsbD0idXJsKCNjKSIvPjxwYXRoIGQ9Ik0xMi4zMiA4LjkzYTEuODMgMS44MyAwIDAxLjYxLTEgMjUuNSAyNS41IDAgMDEtNC40Ni00LjE0IDE2LjkxIDE2LjkxIDAgMDEtMi0yLjkyIDcuNjQgNy42NCAwIDAwLTEuMDkuNDIgMTguMTQgMTguMTQgMCAwMDIuMTUgMy4xOCAyNi40NCAyNi40NCAwIDAwNC43OSA0LjQ2eiIgZmlsbD0iI2YyZjJmMiIgb3BhY2l0eT0iLjciLz48Y2lyY2xlIGN4PSIxNC4xOCIgY3k9IjkuMjciIHI9IjEuODkiIGZpbGw9InVybCgjZCkiLz48cGF0aCBkPSJNMTcuMzUgMTAuNTRsLS4zNS0uMTctLjMtLjE2aC0uMDZsLS4yNi0uMjFoLS4wN0wxNiA5LjhhMS43NiAxLjc2IDAgMDEtLjY0LjkyYy4xMi4wOC4yNS4xNS4zOC4yMmwuMDguMDUuMzUuMTkuODYuNDVhOC42MyA4LjYzIDAgMDAuMjktMS4xMXoiIGZpbGw9IiNmMmYyZjIiIG9wYWNpdHk9Ii41NSIvPjxjaXJjbGUgY3g9IjQuMzgiIGN5PSI4LjY4IiByPSIyLjczIiBmaWxsPSJ1cmwoI2IpIi8+PGNpcmNsZSBjeD0iMTAuMTMiIGN5PSIxMy42NyIgcj0iMS43OCIgZmlsbD0idXJsKCNjKSIvPjwvc3ZnPg==&logoColor=white)](https://azure.microsoft.com/en-us/products/app-service/web)
<!-- * [![Docker](https://img.shields.io/badge/Docker-Containerization-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/) -->

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Architecture

ContributionAPI follows a microservices architecture pattern.

### System Overview

```mermaid
---
title: Contribution Hub
---

graph TB
    %% User Management & Token Management
    A1[Contribution Hub Manager] -.-> J1[User Registration/Login]
    J1 -.-> H[Firebase Firestore]
    A1 -.-> J2[Token Management UI]
    J2 -.-> H
    J2 -.-> I[Google Secret Manager API]
    
    %% API Gateway & Main Service
    A1 -.-> B2[Contribution.Hub API]
    B2 --> C[Contribution Aggregator]
    C --> D[Provider Factory]
    
    %% Data Access
    C --> H
    C --> I
    
    %% Provider Services
    D --> E[Contribution.AzureDevOps API]
    D --> F[Contribution.GitHub API]
    D --> G[Contribution.Providers API]
    
    E --> K2
    F --> K1
    G --> K3

    %% Azure App Service Deployment
    subgraph "Azure App Service Deployment"
        subgraph "Frontend Services"
            A1
            J1
            J2
        end

        subgraph "API Services"
            B2
            C
            D
        end
        
        subgraph "Microservices"
            E
            F
            G
        end
    end

    %% External APIs
    subgraph "External APIs"
        K1[GitHub GraphQL API]
        K2[Azure DevOps API SDK]
        K3[Future APIs]
        %% External Data Layer
        subgraph "Data & Security Layer"
            H
            I
        end
    end
    
    %% Styling
    classDef frontend fill:#e3f2fd,stroke:#b0bec5,color:#616161
    classDef api fill:#ede7f6,stroke:#b0bec5,color:#616161
    classDef microservice fill:#e8f5e9,stroke:#b0bec5,color:#616161
    classDef data fill:#fff8e1,stroke:#b0bec5,color:#616161
    classDef external fill:#fce4ec,stroke:#b0bec5,color:#616161
    
    class A1,A3,B1 frontend
    class B2,C,D api
    class E,F,G microservice
    class H,I data
    class K1,K2,K3 external
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

Follow these steps to get ContributionAPI running locally.

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Git](https://git-scm.com/downloads)
- Firebase project with Firestore enabled (Optional)
- Google Cloud project with Secret Manager enabled (Optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/PROxZIMA/ContributionAPI.git
   cd ContributionAPI
   ```

2. **Set up Firebase credentials** (Optional)
   ```bash
   # Download your Firebase service account key
   # Place it in src/Contribution.Hub/firebase-credentials.json
   ```

3. **Configure Google Cloud credentials** (Optional)
   ```bash
   # Set up Application Default Credentials
   gcloud auth application-default login
   
   # Or set the environment variable
   export GOOGLE_APPLICATION_CREDENTIALS="path/to/your/service-account-key.json"
   ```

4. **Configure application settings** (Optional)
   ```bash
   # Copy and modify configuration files
   cp src/Contribution.Hub/appsettings.Development.json.example src/Contribution.Hub/appsettings.Development.json
   
   # Edit the configuration with your Firebase project details
   ```

5. **Restore dependencies and build**
   ```bash
   dotnet restore
   dotnet build
   ```

6. **Run the services**
   
   **Option A: Run all services together**
   ```bash
   # From VS Code, use the "Run Contribution API" task
   # Or run individually:
   ```
   
   **Option B: Run services individually**
   ```bash
   # Terminal 1 - Hub Service
   cd src/Contribution.Hub
   dotnet run
   
   # Terminal 2 - Azure DevOps Service
   cd src/Contribution.AzureDevOps
   dotnet run
   
   # Terminal 3 - GitHub Service
   cd src/Contribution.GitHub
   dotnet run
   ```

7. **Verify installation**
   ```bash
   curl http://localhost:5298/health
   ```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

### Authentication

ContributionAPI uses read-only tokens for secure access to external services.

#### Setting up Read-Only Tokens

**GitHub Personal Access Token:**
1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate new token (classic) with these scopes:
   - `read:user` (Read ALL user profile data)

**Azure DevOps Personal Access Token:**
1. Go to Azure DevOps → User Settings → Personal access tokens
2. Create new token with these permissions:
   - Code: Read
   - Identity: Read
   - Work Items: Read

### API Endpoints

- Reference: [Contribution Hub Manager](https://c-m-app.azurewebsites.net/home#endpoint) for existing endpoints and usage.

For detailed API documentation, visit the Swagger UI at `http://localhost:5298/swagger` when running locally.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Roadmap

### Current Version (v1.0)
- [x] GitHub integration
- [x] Azure DevOps integration
- [x] Firebase Firestore data storage
- [x] Google Secret Manager integration
- [x] Basic contribution aggregation
- [x] RESTful API endpoints

### Version 1.1 (Q4 2025)
- [x] Support SVG endpoint for contribution graphs
- [ ] Support PNG endpoint for contribution graphs
- [ ] Add provider specific breakdown

### Version 1.2 (Q1 2026)
- [ ] Set up CI/CD via GitHub Actions (Coolify) to VPS for hosting
- [ ] GitLab integration
- [ ] Bitbucket integration
- [ ] Advanced analytics and insights
- [ ] GraphQL API support
- [ ] Persistant caching strategies
    - [ ] Redis
    - [ ] Serve last cache
    - [ ] Queue cache refresh per user per minute
    - [ ] Environment based caching configuration

### Version 2.0 (Q2 2026)
- [ ] Real-time contribution streaming
- [ ] Machine learning-powered insights
- [ ] Team and organization support
- [ ] Advanced reporting and dashboards
- [ ] Plugin architecture for custom providers

### Future Considerations
- [ ] Jira integration
- [ ] Stack Overflow integration
- [ ] LinkedIn integration
- [ ] Custom provider framework
- [ ] Enterprise SSO support

See our [Issues](https://github.com/PROxZIMA/ContributionAPI/issues) page to request features or report bugs.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contributing

We welcome contributions from the community! ContributionAPI is an open-source project, and we appreciate any help to make it better.

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

### Code of Conduct

This project follows our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to uphold this code.

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for more information.

## Contact

### Project Maintainer
**PROxZIMA** - [@PROxZIMA](https://github.com/PROxZIMA)

### Project Links
- **Project Repository**: [https://github.com/PROxZIMA/ContributionAPI](https://github.com/PROxZIMA/ContributionAPI)
- **Issue Tracker**: [https://github.com/PROxZIMA/ContributionAPI/issues](https://github.com/PROxZIMA/ContributionAPI/issues)

### Support

- **Discussions**: Use [GitHub Discussions](https://github.com/PROxZIMA/ContributionAPI/discussions) for community support
- **Bug Reports**: Use our [issue templates](.github/ISSUE_TEMPLATE/) for bug reports
- **Feature Requests**: Submit feature requests through GitHub Issues

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

<div align="center">
  <p>Built with ❤️ by <a href="https://github.com/PROxZIMA">PROxZIMA</a></p>
  <p>⭐ Star this repo if you find it helpful!</p>
</div>