using RetentionReleaseManager.Models.Abstract;

namespace RetentionReleaseManager.Models.Data;

public class Environment : BaseModel
{
    public string Name { get; set; }

    public Environment(string id, string name) 
        : base(id)
    {
        Name = name;
    }
}