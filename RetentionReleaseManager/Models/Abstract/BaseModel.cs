namespace RetentionReleaseManager.Models.Abstract;

public abstract class BaseModel
{
    public virtual string Id { get; set; }
    
    public BaseModel(string id)
    {
        Id = id;
    }
}