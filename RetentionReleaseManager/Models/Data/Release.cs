using RetentionReleaseManager.Models.Abstract;

namespace RetentionReleaseManager.Models.Data;

public class Release : BaseModel
{
    public string ProjectId { get; set; }
    public string Version { get; set; }
    public DateTime Created { get; set; }

    public Release(
        string id, 
        string projectId, 
        string version, 
        DateTime created) : base(id)
    {
        ProjectId = projectId;
        Version = version;
        Created = created;
    }
}