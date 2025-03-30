using Microsoft.Extensions.Logging;
using RetentionReleaseManager.Models.Data;
using Environment = RetentionReleaseManager.Models.Data.Environment;

namespace RetentionReleaseManager.ReleaseRetention;

public class ReleaseRetention(ILogger<ReleaseRetention> logger)
{
    /// <summary>
    /// Retrieves the releases should be kept based on the retention rule
    /// </summary>
    /// <param name="projects">List of projects</param>
    /// <param name="environments">List of environments</param>
    /// <param name="releases">List of releases</param>
    /// <param name="deployments">List of deployments</param>
    /// <param name="releasesToKeep">Number of most recent releases of keep</param>
    /// <returns>List of releases should be kept</returns>
    public List<Release> GetReleasesToKeep(
            List<Project> projects,
            List<Environment> environments,
            List<Release> releases,
            List<Deployment> deployments,
            int releasesToKeep)
    {
        // Dictionary and hashsets for lookups
        var releasesById = releases.ToDictionary(r => r.Id);
    
        var projectIds = new HashSet<string>(projects.Select(p => p.Id));
        var environmentIds = new HashSet<string>(environments.Select(e => e.Id));
    
        var caches = new Dictionary<(string projectId, string envId), ReleaseRetentionCache>();

        // Process the deployments in sequential order
        foreach (var deployment in deployments.OrderBy(d => d.DeployedOn))
        {
            // Assumption: Skip invalid deployments with missing references
            if (!releasesById.TryGetValue(deployment.ReleaseId, out var release) ||
                !projectIds.Contains(release.ProjectId) ||
                !environmentIds.Contains(deployment.EnvironmentId))
            {
                continue;
            }
        
            // Get or create the cache for project/environment tuple as a key
            var key = (release.ProjectId, deployment.EnvironmentId);
            if (!caches.TryGetValue(key, out var cache))
            {
                cache = new ReleaseRetentionCache(releasesToKeep, logger);
                caches[key] = cache;
            }
        
            // Track the deployment in the retention cache
            cache.TrackDeployment(deployment.ReleaseId, deployment.EnvironmentId, deployment.DeployedOn);
        }
    
        // Collect all unique release IDs to keep from the retention cache
        var releasesToKeepIds = caches.Values
            .SelectMany(cache => cache.GetReleasesToKeep())
            .ToHashSet();
    
        // Return the actual release object for the ones to keep
        return releasesToKeepIds
            .Select(id => releasesById[id])
            .ToList();
    }
}