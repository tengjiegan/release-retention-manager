namespace RetentionReleaseManager.Models;

internal class Node
{
    public CacheEntry Entry { get; set; }
    public Node Next { get; set; }
    public Node Prev { get; set; }
}