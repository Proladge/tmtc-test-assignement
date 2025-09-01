namespace TaskManagementApi.Services;

using TaskManagementApi.Models;
using System.Collections.Concurrent;

public class InMemoryDataService : IDataService
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ConcurrentDictionary<Guid, UserTask> _tasks = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _taskLocks = new();

    // Helper method to get or create a lock for a specific task
    // Each task gets its own lock, enabling concurrent operations on different tasks
    private SemaphoreSlim GetTaskLock(Guid taskId)
    {
        return _taskLocks.GetOrAdd(taskId, _ => new SemaphoreSlim(1, 1));
    }

    // Helper method to cleanup unused task locks (prevents memory leaks)
    private void CleanupTaskLock(Guid taskId)
    {
        if (_taskLocks.TryRemove(taskId, out var semaphore))
        {
            semaphore.Dispose();
        }
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Values.AsEnumerable());
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByNameAsync(string name)
    {
        var user = _users.Values.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public async Task<User> CreateUserAsync(string name)
    {
        // Check if user with this name already exists
        var existingUser = await GetUserByNameAsync(name);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with name '{name}' already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _users[user.Id] = user;
        return user;
    }

    public async Task<User?> UpdateUserAsync(Guid userId, string name)
    {
        if (!_users.TryGetValue(userId, out var user))
            return null;

        // Check if another user already has this name
        var existingUser = await GetUserByNameAsync(name);
        if (existingUser != null && existingUser.Id != userId)
        {
            throw new InvalidOperationException($"User with name '{name}' already exists.");
        }

        user.Name = name;
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        if (!_users.TryGetValue(userId, out var user))
            return false;

        // Check if user has active tasks
        var userTasks = await GetTasksByUserAsync(userId);
        var activeTasks = userTasks.Where(t => t.State != TaskState.Completed).ToList();

        if (activeTasks.Any())
        {
            // Reassign tasks to other available users or mark as Waiting
            foreach (var task in activeTasks)
            {
                var availableUser = await FindAvailableUserForTaskReassignment(userId);
                if (availableUser != null)
                {
                    task.AssignedToUserId = availableUser.Id;
                }
                else
                {
                    task.AssignedToUserId = null;
                    task.State = TaskState.Waiting;
                }
                task.UpdatedAt = DateTime.UtcNow;
            }
        }

        return _users.TryRemove(userId, out _);
    }

    public Task<IEnumerable<UserTask>> GetAllTasksAsync()
    {
        return Task.FromResult(_tasks.Values.AsEnumerable());
    }

    public Task<UserTask?> GetTaskByIdAsync(Guid taskId)
    {
        _tasks.TryGetValue(taskId, out var task);
        return Task.FromResult(task);
    }

    public Task<UserTask?> GetTaskByTitleAsync(string title)
    {
        var task = _tasks.Values.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(task);
    }

    public Task<IEnumerable<UserTask>> GetTasksByUserAsync(Guid userId)
    {
        var userTasks = _tasks.Values.Where(t => t.AssignedToUserId == userId);
        return Task.FromResult(userTasks);
    }

    public async Task<UserTask> CreateTaskAsync(string title)
    {
        // Check if task with this title already exists
        var existingTask = await GetTaskByTitleAsync(title);
        if (existingTask != null)
        {
            throw new InvalidOperationException($"Task with title '{title}' already exists.");
        }

        var task = new UserTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            State = TaskState.Waiting,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Try to auto-assign to an available user
        var availableUser = await FindAvailableUserAsync();
        if (availableUser != null)
        {
            task.AssignedToUserId = availableUser.Id;
            task.State = TaskState.InProgress;
            
            // Add to assignment history
            task.AssignmentHistory.Add(new AssignmentHistoryEntry
            {
                UserId = availableUser.Id,
                AssignedAt = DateTime.UtcNow
            });
        }

        _tasks[task.Id] = task;
        return task;
    }

    public async Task<UserTask?> UpdateTaskAsync(Guid taskId, string? title = null, TaskState? state = null, Guid? assignedToUserId = null)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return null;

        var taskLock = GetTaskLock(taskId);
        await taskLock.WaitAsync();
        
        try
        {
            // Update title if provided and check uniqueness
            if (title != null && !task.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            {
                var existingTask = _tasks.Values.FirstOrDefault(t => 
                    t.Id != taskId && t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (existingTask != null)
                {
                    throw new InvalidOperationException($"Task with title '{title}' already exists.");
                }
                task.Title = title;
            }

            // Update state if provided (task-level operation)
            if (state.HasValue)
            {
                task.State = state.Value;
            }

            // Update assignment if provided
            if (assignedToUserId.HasValue)
            {
                // Check if the target user can accept more tasks
                // Note: This check is "best effort" - rare race conditions may allow brief violations
                // of the 3-task limit, but this is acceptable for better performance
                var currentUserTaskCount = _tasks.Values.Count(t => 
                    t.AssignedToUserId == assignedToUserId.Value && t.State != TaskState.Completed);
                
                if (currentUserTaskCount >= 3)
                {
                    throw new InvalidOperationException($"User already has the maximum number of tasks (3).");
                }

                // Track previous assignment for reassignment logic
                task.PreviousAssignedUserId = task.AssignedToUserId;
                task.AssignedToUserId = assignedToUserId.Value;
                
                // Add to assignment history if this is a new assignment
                if (!task.AssignmentHistory.Any(h => h.UserId == assignedToUserId.Value))
                {
                    task.AssignmentHistory.Add(new AssignmentHistoryEntry
                    {
                        UserId = assignedToUserId.Value,
                        AssignedAt = DateTime.UtcNow
                    });
                }
                
                if (task.State == TaskState.Waiting)
                {
                    task.State = TaskState.InProgress;
                }
            }

            task.UpdatedAt = DateTime.UtcNow;
            return task;
        }
        finally
        {
            taskLock.Release();
        }
    }

    public async Task<bool> DeleteTaskAsync(Guid taskId)
    {
        var result = _tasks.TryRemove(taskId, out _);
        if (result)
        {
            // Clean up the task lock to prevent memory leaks
            CleanupTaskLock(taskId);
        }
        return result;
    }

    public async Task<int> GetUserTaskCountAsync(Guid userId)
    {
        var userTasks = await GetTasksByUserAsync(userId);
        return userTasks.Count(t => t.State != TaskState.Completed);
    }

    public async Task<User?> FindAvailableUserAsync()
    {
        var users = await GetAllUsersAsync();
        
        foreach (var user in users)
        {
            if (await CanUserAcceptMoreTasksAsync(user.Id))
            {
                return user;
            }
        }
        
        return null;
    }

    public async Task<bool> CanUserAcceptMoreTasksAsync(Guid userId)
    {
        var taskCount = await GetUserTaskCountAsync(userId);
        return taskCount < 3;
    }

    private async Task<User?> FindAvailableUserForTaskReassignment(Guid excludeUserId)
    {
        var users = await GetAllUsersAsync();
        
        foreach (var user in users.Where(u => u.Id != excludeUserId))
        {
            if (await CanUserAcceptMoreTasksAsync(user.Id))
            {
                return user;
            }
        }
        
        return null;
    }

    // Reassignment logic implementation
    public async Task<IEnumerable<UserTask>> GetTasksEligibleForReassignmentAsync()
    {
        var tasks = await GetAllTasksAsync();
        return tasks.Where(t => t.State != TaskState.Completed && t.IsEligibleForReassignment);
    }

    public async Task<User?> FindNextUserForTaskAsync(UserTask task)
    {
        var allUsers = await GetAllUsersAsync();
        var availableUsers = new List<User>();

        foreach (var user in allUsers)
        {
            // Skip if this is the currently assigned user
            if (task.AssignedToUserId == user.Id)
                continue;

            // Skip if this is the previous round user
            if (task.PreviousAssignedUserId == user.Id)
                continue;

            // Skip if user can't accept more tasks
            if (!await CanUserAcceptMoreTasksAsync(user.Id))
                continue;

            availableUsers.Add(user);
        }

        if (!availableUsers.Any())
            return null;

        // Prefer users who haven't been assigned to this task yet
        var neverAssignedUsers = availableUsers
            .Where(u => !task.AssignmentHistory.Any(h => h.UserId == u.Id))
            .ToList();

        if (neverAssignedUsers.Any())
        {
            // Return random user who has never been assigned
            var random = new Random();
            return neverAssignedUsers[random.Next(neverAssignedUsers.Count)];
        }

        // All available users have been assigned before, pick randomly from available
        var random2 = new Random();
        return availableUsers[random2.Next(availableUsers.Count)];
    }

    public async Task<bool> HasTaskBeenAssignedToAllUsersAsync(UserTask task)
    {
        var allUsers = await GetAllUsersAsync();
        var assignedUserIds = task.AssignmentHistory.Select(h => h.UserId).ToHashSet();
        
        // Check if all existing users have been assigned to this task
        return allUsers.All(u => assignedUserIds.Contains(u.Id));
    }

    public async Task ReassignTasksAsync()
    {
        var eligibleTasks = await GetTasksEligibleForReassignmentAsync();
        
        // Process tasks concurrently with per-task locking for better performance
        var reassignmentTasks = eligibleTasks.Select(async task =>
        {
            var taskLock = GetTaskLock(task.Id);
            await taskLock.WaitAsync();
            
            try
            {
                // Check if task should be completed (assigned to all users)
                if (await HasTaskBeenAssignedToAllUsersAsync(task))
                {
                    // Mark as completed and unassigned
                    task.State = TaskState.Completed;
                    task.PreviousAssignedUserId = task.AssignedToUserId;
                    task.AssignedToUserId = null;
                    task.IsEligibleForReassignment = false;
                    task.UpdatedAt = DateTime.UtcNow;
                    return;
                }

                // Find next user for this task
                var nextUser = await FindNextUserForTaskAsync(task);
                
                if (nextUser != null)
                {
                    // Reassign to next user
                    task.PreviousAssignedUserId = task.AssignedToUserId;
                    task.AssignedToUserId = nextUser.Id;
                    task.State = TaskState.InProgress;
                    
                    // Add to assignment history if not already there
                    if (!task.AssignmentHistory.Any(h => h.UserId == nextUser.Id))
                    {
                        task.AssignmentHistory.Add(new AssignmentHistoryEntry
                        {
                            UserId = nextUser.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                    }
                    
                    task.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // No available user, set to waiting
                    task.PreviousAssignedUserId = task.AssignedToUserId;
                    task.AssignedToUserId = null;
                    task.State = TaskState.Waiting;
                    task.UpdatedAt = DateTime.UtcNow;
                }
            }
            finally
            {
                taskLock.Release();
            }
        });

        // Wait for all reassignments to complete
        await Task.WhenAll(reassignmentTasks);
    }

    public async Task<UserTask?> AssignTaskToUserWithHistoryAsync(Guid taskId, Guid userId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return null;

        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return null;

        var taskLock = GetTaskLock(taskId);
        await taskLock.WaitAsync();
        
        try
        {
            // Recheck capacity inside the lock to prevent race conditions
            if (!await CanUserAcceptMoreTasksAsync(userId))
                return null;

            // Track previous assignment
            task.PreviousAssignedUserId = task.AssignedToUserId;
            task.AssignedToUserId = userId;
            task.State = TaskState.InProgress;
            
            // Add to assignment history if not already there
            if (!task.AssignmentHistory.Any(h => h.UserId == userId))
            {
                task.AssignmentHistory.Add(new AssignmentHistoryEntry
                {
                    UserId = userId,
                    AssignedAt = DateTime.UtcNow
                });
            }
            
            task.UpdatedAt = DateTime.UtcNow;
            return task;
        }
        finally
        {
            taskLock.Release();
        }
    }
}
