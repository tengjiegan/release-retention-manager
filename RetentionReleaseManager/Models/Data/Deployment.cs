using RetentionReleaseManager.Models.Abstract;

namespace RetentionReleaseManager.Models.Data;

public class Deployment : BaseModel
{
    public string ReleaseId { get; set; }
    public string EnvironmentId { get; set; }
    public DateTime DeployedOn { get; set; }
    public string ProjectId { get; set; }

    public Deployment(
        string id, 
        string releaseId, 
        string environmentId, 
        DateTime deployedOn, 
        string projectId) : base(id)
    {
        ReleaseId = releaseId;
        EnvironmentId = environmentId;
        DeployedOn = deployedOn;
        ProjectId = projectId;
    }
}