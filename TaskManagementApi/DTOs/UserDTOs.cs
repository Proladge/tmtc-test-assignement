namespace TaskManagementApi.DTOs;

public record CreateUserRequest(string Name);

public record UpdateUserRequest(string Name);

public record UserResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UserWithTasksResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<TaskResponse> Tasks
);
