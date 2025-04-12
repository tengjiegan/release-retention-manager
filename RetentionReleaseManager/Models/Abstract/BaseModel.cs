namespace RetentionReleaseManager.Models.Abstract;

public abstract class BaseModel
{
    public string Id { get; set; }
    
    public BaseModel(string id)
    {
        Id = id;
    }
}