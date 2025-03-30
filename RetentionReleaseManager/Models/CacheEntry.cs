namespace RetentionReleaseManager.Models;

public class CacheEntry
{
    public string ReleaseId { get; set; }
    public string EnvironmentId { get; set; }
    public DateTime DeploymentDate { get; set; }
    
    public CacheEntry(string releaseId, string environmentId, DateTime deploymentDate)
    {
        ReleaseId = releaseId;
        EnvironmentId = environmentId;
        DeploymentDate = deploymentDate;
    }
}