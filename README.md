# Continuous Deployment Retention Release Manager

A .NET tool for managing release retention policies, determining which releases to keep based on deployment history.

## Purpose

This project provides a ReleaseRetention class that processes project releases and deployments to identify releases that should be retained according to specified rules (e.g., keeping the N most recent releases per project/environment).

## Features

- Analyzes releases across multiple projects and environments
- Considers deployment recency to prioritize retention
- Efficiently handles memory usage through caching

## Algorithm and Data Structures

- HashSet
- Linked List
- Dictionary/HashTable

## NuGet Packages Used

- xUnit: for unit testing.
- AutoFixture: for generating test data.
- Moq: for mocking dependencies during unit testing.
- Microsoft.Extensions.Logging: for logging interfaces.
- Microsoft.NET.Test.Sdk: for running tests with the .NET SDK.

## Getting Started

### Prerequisites

- .NET SDK (version 8.0 or later)
- An IDE like Rider or Visual Studio

### Installation

1. Clone the repository:

    >  `git clone https://github.com/tengjiegan/RetentionReleaseManager.git`

2. Navigate to the project directory:

    >  `cd release-retention-manager`

3. Restore dependencies:

    >  `dotnet restore`

4. Build the project:

    >  `dotnet build`

### Running Tests

Run the unit tests using the following command:

> `dotnet test`
