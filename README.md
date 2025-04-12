# Continuous Deployment Retention Release Manager

A .NET tool for managing release retention policies, determining which releases to keep based on deployment history.

## Purpose

This project provides a ReleaseRetention class that processes project releases and deployments to identify releases that should be retained according to specified rules (e.g., keeping the N most recent releases per project/environment).

## Features

- Efficient tracking of deployments across multiple projects and environments
- Considers deployment recency to prioritize retentions
- Configurable capacity retention policy


## Algorithm and Data Structures

- HashSet
- Dictionary
- Doubly Linked List

<br>

<p align="center">
  <img src="https://github.com/user-attachments/assets/550772be-b514-4837-8709-3286a31738b3" alt="Image" />
</p>

<br>


## Doubly Linked List + Dictionary Approach

<div align="center">

| Feature                          | Time Complexity    | Notes                                                                 |
|----------------------------------|--------------------|-----------------------------------------------------------------------|
| **Insertion**                    | `O(1)`             | New releases can be added to the head or tail instantly               |
| **Removal**                      | `O(1)`             | Any release can be removed using a direct node reference              |
| **Reordering (Move to Head)**    | `O(1)`             | Recently deployed releases can be moved in constant time              |
| **Lookup (Find by Release ID)**  | `O(1)`             | Dictionary provides fast access reference to the nodes                |
| **Maintains Recency Order**      | `O(n log n)`       | Deployments are externally sorted before being passed                 |
| **Efficiently Keeps N Items**    | `O(1)`             | Easy to evict tail nodes beyond the allowed capacity                  |

</div>

<br>

## NuGet Packages Used

- xUnit: for unit testing.
- AutoFixture: for generating test data.
- Moq: for mocking dependencies during unit testing.
- Microsoft.Extensions.Logging: for logging interfaces.
- Microsoft.NET.Test.Sdk: for running tests with the .NET SDK.

<br>

## Getting Started

### Prerequisites

- .NET SDK (version 8.0 or later)
- An IDE like Rider or Visual Studio


### Installation

1. Clone the repository:

    >  `git clone https://github.com/tengjiegan/release-retention-manager.git`

2. Navigate to the project directory:

    >  `cd release-retention-manager`

3. Restore dependencies:

    >  `dotnet restore`

4. Build the project:

    >  `dotnet build`


### Running Tests

Run the unit tests using the following command:

>  `dotnet test`
