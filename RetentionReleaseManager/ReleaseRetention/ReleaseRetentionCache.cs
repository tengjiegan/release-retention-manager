using Microsoft.Extensions.Logging;
using RetentionReleaseManager.Models;

namespace RetentionReleaseManager.ReleaseRetention;

public class ReleaseRetentionCache(int capacity, ILogger logger)
{
    private readonly Dictionary<string, Node> _dictionary = new();
    private Node _head;
    private Node _tail;
    private int _count;

    /// <summary>
    /// Tracks a deployment in the cache
    /// Updates the deployed release in the cache if already exists
    /// Adds new node to the cache if deployed release does not already exist in cache,
    /// Removes older deployed releases if hit maximum capacity
    /// </summary>
    /// <param name="releaseId"></param>
    /// <param name="environmentId"></param>
    /// <param name="deploymentDate"></param>
    public void TrackDeployment(string releaseId, string environmentId, DateTime deploymentDate)
    {
        // Check if deployed release already exist in the cache, and update if true
        // Create new node and add it to the linked list and dictionary for faster lookup otherwise
        if (_dictionary.TryGetValue(releaseId, out var existingNode))
        {
            // Move deployed release node to the head of the linked list if there is a more recent deployment 
            if (deploymentDate > existingNode.Entry.DeploymentDate)
            {
                RemoveNode(existingNode);
                
                existingNode.Entry.DeploymentDate = deploymentDate;
                existingNode.Entry.EnvironmentId = environmentId;
                
                AddToHead(existingNode);
            }
        }
        else
        {
            var newNode = new Node {
                Entry = new CacheEntry(
                    releaseId, 
                    environmentId, 
                    deploymentDate)
            };
            
            // Add the new node to the linked list and dictionary
            _dictionary[releaseId] = newNode;
            
            AddToHead(newNode);
            
            _count++;
            
            // Removes older deployed releases if hit capacity
            if (_count > capacity)
            {
                RemoveFromTail();
            }
        }
    }
    
    /// <summary>
    /// Adds a node to the head of the linked list
    /// </summary>
    /// <param name="node"></param>
    private void AddToHead(Node node)
    {
        // Adjust new node next and previous pointers
        node.Next = _head;
        node.Prev = null;
        
        // If head exists, update previous pointer to the new node
        if (_head != null)
        {
            _head.Prev = node;
        }
        
        _head = node;
        
        // If tail is null, set tail to the new node
        if (_tail == null)
        {
            _tail = node;
        }
    }
    
    /// <summary>
    /// Removes a node in any position of the linked list
    /// </summary>
    /// <param name="node"></param>
    private void RemoveNode(Node node)
    {
        // Adjust previous node pointer
        if (node.Prev != null)
        {
            node.Prev.Next = node.Next;
        }
        else
        {
            _head = node.Next;
        }
        
        // Adjust next node pointer
        if (node.Next != null)
        {
            node.Next.Prev = node.Prev;
        }
        else
        {
            _tail = node.Prev;
        }
    }
    
    /// <summary>
    /// Removes node at the tail of the linked list and corresponding entry in the dictionary
    /// </summary>
    private void RemoveFromTail()
    {
        if (_tail == null) return;
        
        // Get the oldest deployed release and remove it 
        string releaseId = _tail.Entry.ReleaseId;
        _dictionary.Remove(releaseId);
        _tail = _tail.Prev;
        
        // Update new tail next pointer
        if (_tail != null)
        {
            _tail.Next = null;
        }
        else
        {
            _head = null;
        }
        
        _count--;
    }
    
    /// <summary>
    /// Returns the IDs of releases that should be kept according to the retention rule/policy
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetReleasesToKeep()
    {
        var current = _head;
        while (current != null)
        {
            logger.LogInformation(
                "{ReleaseId} kept because it was deployed to {EnvironmentId} on {DeploymentDate}", 
                current.Entry.ReleaseId, current.Entry.EnvironmentId, current.Entry.DeploymentDate);
            
            yield return current.Entry.ReleaseId;
            current = current.Next;
        }
    }
}