using RetentionReleaseManager.Models.Abstract;

namespace RetentionReleaseManager.Models.Data;

public class Project : BaseModel
{
    public string Name { get; set; }

    public Project(string id, string name) 
        : base(id)
    {
        Name = name;
    }
}