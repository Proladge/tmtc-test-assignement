namespace TaskManagementApi.Services;

using TaskManagementApi.Models;

public interface IDataService
{
    // User operations
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByNameAsync(string name);
    Task<User> CreateUserAsync(string name);
    Task<User?> UpdateUserAsync(Guid userId, string name);
    Task<bool> DeleteUserAsync(Guid userId);
    
    // UserTask operations
    Task<IEnumerable<UserTask>> GetAllTasksAsync();
    Task<UserTask?> GetTaskByIdAsync(Guid taskId);
    Task<UserTask?> GetTaskByTitleAsync(string title);
    Task<IEnumerable<UserTask>> GetTasksByUserAsync(Guid userId);
    Task<UserTask> CreateTaskAsync(string title);
    Task<UserTask?> UpdateTaskAsync(Guid taskId, string? title = null, TaskState? state = null, Guid? assignedToUserId = null);
    Task<bool> DeleteTaskAsync(Guid taskId);
    
    // Business logic
    Task<int> GetUserTaskCountAsync(Guid userId);
    Task<User?> FindAvailableUserAsync();
    Task<bool> CanUserAcceptMoreTasksAsync(Guid userId);
    
    // Reassignment logic
    Task<IEnumerable<UserTask>> GetTasksEligibleForReassignmentAsync();
    Task<User?> FindNextUserForTaskAsync(UserTask task);
    Task<bool> HasTaskBeenAssignedToAllUsersAsync(UserTask task);
    Task ReassignTasksAsync();
    Task<UserTask?> AssignTaskToUserWithHistoryAsync(Guid taskId, Guid userId);
}
