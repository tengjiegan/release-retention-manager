using Moq;
using AutoFixture;
using Microsoft.Extensions.Logging;
using RetentionReleaseManager.Models.Data;
using RetentionReleaseManager.ReleaseRetention;
using Environment = RetentionReleaseManager.Models.Data.Environment;

namespace RetentionReleaseManagerTests;

public class ReleaseRetentionTests
    {
        private readonly ReleaseRetention _releaseRetention;
        private readonly Fixture _fixture;

        public ReleaseRetentionTests()
        {
            _fixture = new Fixture();
            Mock<ILogger<ReleaseRetention>> mockLogger = new();
            _releaseRetention = new ReleaseRetention(mockLogger.Object);
        }

        [Fact]
        public void GetReleasesToKeep_GivenReleasesWithoutDeployments_CanReturnEmptyList()
        {
            // Arrange
            var projects = _fixture.CreateMany<Project>(1).ToList();
            var environments = _fixture.CreateMany<Environment>(1).ToList();
            
            var releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, projects[0].Id)
                .CreateMany(5)
                .ToList();
            
            var deployments = new List<Deployment>();

            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                projects, 
                environments, 
                releases, 
                deployments, 
                3);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetReleasesToKeep_GivenSingleReleaseAndDeployment_CanKeepDeployedRelease()
        {
            // Arrange
            var projectId = _fixture.Create<string>();
            var environmentId = _fixture.Create<string>();
            var releaseId = _fixture.Create<string>();
            
            var project = _fixture.Build<Project>()
                .With(p => p.Id, projectId)
                .Create();
                
            var environment = _fixture.Build<Environment>()
                .With(e => e.Id, environmentId)
                .Create();
                
            var release = _fixture.Build<Release>()
                .With(r => r.Id, releaseId)
                .With(r => r.ProjectId, projectId)
                .Create();
                
            var deployment = _fixture.Build<Deployment>()
                .With(d => d.ReleaseId, releaseId)
                .With(d => d.ProjectId, projectId)
                .With(d => d.EnvironmentId, environmentId)
                .Create();
                
            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                [release],
                [deployment], 
                1);

            // Assert
            Assert.Single(result);
            Assert.Equal(releaseId, result[0].Id);
        }
        
        [Fact]
        public void GetReleasesToKeep_GivenTwoReleasesOneEnvironment_CanKeepMostRecentlyDeployedRelease()
        {
            // Arrange
            var projectId = _fixture.Create<string>();
            var environmentId = _fixture.Create<string>();
            
            var project = _fixture.Build<Project>()
                .With(p => p.Id, projectId)
                .Create();
                
            var environment = _fixture.Build<Environment>()
                .With(e => e.Id, environmentId)
                .Create();
            
            var release1 = _fixture.Build<Release>()
                .With(r => r.Id, "Release-1")
                .With(r => r.ProjectId, projectId)
                .Create();
                
            var release2 = _fixture.Build<Release>()
                .With(r => r.Id, "Release-2")
                .With(r => r.ProjectId, projectId)
                .Create();
                
            var deployment1 = _fixture.Build<Deployment>()
                .With(d => d.ReleaseId, "Release-2")
                .With(d => d.ProjectId, projectId)
                .With(d => d.EnvironmentId, environmentId)
                .With(d => d.DeployedOn, DateTime.Now.AddHours(-2))
                .Create();
                
            var deployment2 = _fixture.Build<Deployment>()
                .With(d => d.ReleaseId, "Release-1")
                .With(d => d.ProjectId, projectId)
                .With(d => d.EnvironmentId, environmentId)
                .With(d => d.DeployedOn, DateTime.Now.AddHours(-1))
                .Create();
            
            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                [release1, release2],
                [deployment1, deployment2],
                1);

            // Assert
            Assert.Single(result);
            Assert.Equal("Release-1", result[0].Id);
        }

        [Fact]
        public void GetReleasesToKeep_GivenTwoReleasesAndTwoEnvironments_CanKeepBothDeployedReleases()
        {
            // Arrange
            var projectId = _fixture.Create<string>();
            
            var project = _fixture.Build<Project>()
                .With(p => p.Id, projectId)
                .Create();
                
            var environment1 = _fixture.Build<Environment>()
                .With(e => e.Id, "Environment-1")
                .Create();
                
            var environment2 = _fixture.Build<Environment>()
                .With(e => e.Id, "Environment-2")
                .Create();
            
            var release1 = _fixture.Build<Release>()
                .With(r => r.Id, "Release-1")
                .With(r => r.ProjectId, projectId)
                .Create();
                
            var release2 = _fixture.Build<Release>()
                .With(r => r.Id, "Release-2")
                .With(r => r.ProjectId, projectId)
                .Create();
                
            var deployment1 = _fixture.Build<Deployment>()
                .With(d => d.ReleaseId, "Release-2")
                .With(d => d.ProjectId, projectId)
                .With(d => d.EnvironmentId, "Environment-1")
                .With(d => d.DeployedOn, DateTime.Now.AddDays(-2))
                .Create();
                
            var deployment2 = _fixture.Build<Deployment>()
                .With(d => d.ReleaseId, "Release-1")
                .With(d => d.ProjectId, projectId)
                .With(d => d.EnvironmentId, "Environment-2")
                .With(d => d.DeployedOn, DateTime.Now.AddDays(-1))
                .Create();
            
            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment1, environment2],
                [release1, release2],
                [deployment1, deployment2],
                1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == "Release-1");
            Assert.Contains(result, r => r.Id == "Release-2");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void GetReleasesToKeep_GivenNReleasesToKeep_CanKeepNMostRecentlyDeployedReleases(int releasesToKeep)
        {
            // Arrange
            var project = _fixture.Create<Project>();
            var environment = _fixture.Create<Environment>();
            
            var releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project.Id)
                .CreateMany(5)
                .ToList();
            
            var deployments = new List<Deployment>();
            for (int i = 0; i < releases.Count; i++)
            {
                deployments.Add(_fixture.Build<Deployment>()
                    .With(d => d.Id, $"Deployment-{i+1}")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[i].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-i))
                    .Create());
            }

            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                releases,
                deployments,
                releasesToKeep);

            // Assert
            Assert.Equal(Math.Min(releasesToKeep, releases.Count), result.Count);
            
            for (int i = 0; i < Math.Min(releasesToKeep, releases.Count); i++)
            {
                Assert.Contains(releases[i], result);
            }
        }
        
        [Fact]
        public void GetReleasesToKeep_GivenMultipleProjectsAndEnvironments_CanKeepCorrectDeployedReleases()
        {
            // Arrange
            var project1 = _fixture.Create<Project>();
            var project2 = _fixture.Create<Project>();
            var projects = new List<Project> { project1, project2 };
            
            var env1 = _fixture.Create<Environment>();
            var env2 = _fixture.Create<Environment>();
            var environments = new List<Environment> { env1, env2 };
            
            var project1Releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project1.Id)
                .CreateMany(5)
                .ToList();
                
            var project2Releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project2.Id)
                .CreateMany(3)
                .ToList();
                
            var allReleases = project1Releases.Concat(project2Releases).ToList();
            
            var deployments = new List<Deployment>();
            
            // Project 1, Env 1 - all releases
            for (int i = 0; i < project1Releases.Count; i++)
            {
                deployments.Add(_fixture.Build<Deployment>()
                    .With(d => d.Id, $"Deployment-P1E1-{i+1}")
                    .With(d => d.ProjectId, project1.Id)
                    .With(d => d.EnvironmentId, env1.Id)
                    .With(d => d.ReleaseId, project1Releases[i].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-i))
                    .Create());
            }
            
            // Project 1, Env 2 - first 3 releases
            for (int i = 0; i < 3; i++)
            {
                deployments.Add(_fixture.Build<Deployment>()
                    .With(d => d.Id, $"Deployment-P1E2-{i+1}")
                    .With(d => d.ProjectId, project1.Id)
                    .With(d => d.EnvironmentId, env2.Id)
                    .With(d => d.ReleaseId, project1Releases[i].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-i))
                    .Create());
            }
            
            // Project 2, Env 1 - all releases
            for (int i = 0; i < project2Releases.Count; i++)
            {
                deployments.Add(_fixture.Build<Deployment>()
                    .With(d => d.Id, $"Deployment-P2E1-{i+1}")
                    .With(d => d.ProjectId, project2.Id)
                    .With(d => d.EnvironmentId, env1.Id)
                    .With(d => d.ReleaseId, project2Releases[i].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-i))
                    .Create());
            }

            // Act
            var result = _releaseRetention.GetReleasesToKeep(projects, environments, allReleases, deployments, 2);

            // Assert
            Assert.True(result.Count <= 6);
            
            var p1E1Releases = project1Releases.Take(2).ToList();
            Assert.All(p1E1Releases, r => Assert.Contains(r, result));
            
            var p1E2Releases = project1Releases.Take(2).ToList();
            Assert.All(p1E2Releases, r => Assert.Contains(r, result));
            
            var p2E1Releases = project2Releases.Take(2).ToList();
            Assert.All(p2E1Releases, r => Assert.Contains(r, result));
        }
        
        [Fact]
        public void GetReleasesToKeep_GivenMultipleDeploymentRecencies_CanKeepNLatestDeployedReleases()
        {
            // Arrange
            var project = _fixture.Create<Project>();
            var environment = _fixture.Create<Environment>();
            
            var releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project.Id)
                .CreateMany(3)
                .ToList();
            
            var deployments = new List<Deployment>
            {
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-1")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[0].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-2))
                    .Create(),
                    
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-2")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[0].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-10))
                    .Create(),
                
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-3")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[1].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-5))
                    .Create(),
                
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-4")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[2].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-1))
                    .Create()
            };

            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                releases,
                deployments,
                2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(releases[2], result);
            Assert.Contains(releases[0], result);
            Assert.DoesNotContain(releases[1], result);
        }
        
        [Fact]
        public void GetReleasesToKeep_GivenMissingProjectReference_CanSkipDeployment()
        {
            // Arrange
            var project = _fixture.Create<Project>();
            var environment = _fixture.Create<Environment>();
            
            var releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project.Id)
                .CreateMany(2)
                .ToList();
                
            const string nonExistentProjectId = "Project-99";
            var releaseWithMissingProject = _fixture.Build<Release>()
                .With(r => r.Id, "Release-99")
                .With(r => r.ProjectId, nonExistentProjectId)
                .With(r => r.Version, "9.9.9")
                .Create();
                
            releases.Add(releaseWithMissingProject);
            
            var deployments = new List<Deployment>
            {
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-1")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[0].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddDays(-1))
                    .Create(),
                    
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-2")
                    .With(d => d.ProjectId, nonExistentProjectId)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releaseWithMissingProject.Id)
                    .With(d => d.DeployedOn, DateTime.Now)
                    .Create()
            };

            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                releases,
                deployments,
                2);

            // Assert
            Assert.Single(result);
            Assert.Contains(releases[0], result);
            Assert.DoesNotContain(releaseWithMissingProject, result);
        }

        [Fact]
        public void GetReleasesToKeep_GivenMissingEnvironmentReference_CanSkipDeployment()
        {
            // Arrange
            var project = _fixture.Create<Project>();
            var environment = _fixture.Create<Environment>();
            
            var releases = _fixture.Build<Release>()
                .With(r => r.ProjectId, project.Id)
                .CreateMany(2)
                .ToList();
                
            var deployments = new List<Deployment>
            {
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-1")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, releases[0].Id)
                    .With(d => d.DeployedOn, DateTime.Now)
                    .Create(),
                    
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-2")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, "Non-existent-Environment")
                    .With(d => d.ReleaseId, releases[1].Id)
                    .With(d => d.DeployedOn, DateTime.Now.AddHours(-1))
                    .Create()
            };
        
            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                releases,
                deployments,
                2);
        
            // Assert
            Assert.Single(result);
            Assert.Contains(releases[0], result);
            Assert.DoesNotContain(releases[1], result);
        }

        [Fact]
        public void GetReleasesToKeep_GivenMissingReleaseReference_CanSkipDeployment()
        {
            // Arrange
            var project = _fixture.Create<Project>();
            var environment = _fixture.Create<Environment>();
            
            var release = _fixture.Build<Release>()
                .With(r => r.ProjectId, project.Id)
                .Create();
                
            var deployments = new List<Deployment>
            {
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-1")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, release.Id)
                    .With(d => d.DeployedOn, DateTime.Now)
                    .Create(),
                    
                _fixture.Build<Deployment>()
                    .With(d => d.Id, "Deployment-2")
                    .With(d => d.ProjectId, project.Id)
                    .With(d => d.EnvironmentId, environment.Id)
                    .With(d => d.ReleaseId, "Non-existent-Release")
                    .With(d => d.DeployedOn, DateTime.Now.AddHours(-1))
                    .Create()
            };
        
            // Act
            var result = _releaseRetention.GetReleasesToKeep(
                [project],
                [environment],
                [release],
                deployments,
                2);
        
            // Assert
            Assert.Single(result);
            Assert.Equal(release.Id, result[0].Id);
        }
    }