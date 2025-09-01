namespace TaskManagementApi.Models;

public enum TaskState
{
    Waiting,
    InProgress,
    Completed
}

public class AssignmentHistoryEntry
{
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

public class UserTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TaskState State { get; set; } = TaskState.Waiting;
    public Guid? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Assignment tracking for reassignment logic
    public List<AssignmentHistoryEntry> AssignmentHistory { get; set; } = new();
    public Guid? PreviousAssignedUserId { get; set; } // User from previous round
    public bool IsEligibleForReassignment { get; set; } = true; // False when completed via reassignment rule
}
