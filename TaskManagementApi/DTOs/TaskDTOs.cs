namespace TaskManagementApi.DTOs;

using TaskManagementApi.Models;

public record CreateTaskRequest(string Title);

public record UpdateTaskRequest(
    string? Title = null,
    TaskState? State = null,
    Guid? AssignedToUserId = null
);

public record AssignmentHistoryResponse(
    Guid UserId,
    string UserName,
    DateTime AssignedAt
);

public record TaskResponse(
    Guid Id,
    string Title,
    TaskState State,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid? PreviousAssignedUserId,
    string? PreviousAssignedUserName,
    bool IsEligibleForReassignment,
    IEnumerable<AssignmentHistoryResponse> AssignmentHistory,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
