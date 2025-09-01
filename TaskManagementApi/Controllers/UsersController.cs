namespace TaskManagementApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Services;
using TaskManagementApi.DTOs;
using TaskManagementApi.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IDataService _dataService;

    public UsersController(IDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        var users = await _dataService.GetAllUsersAsync();
        var userResponses = users.Select(u => new UserResponse(
            u.Id,
            u.Name,
            u.CreatedAt,
            u.UpdatedAt
        ));
        return Ok(userResponses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _dataService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var userResponse = new UserResponse(
            user.Id,
            user.Name,
            user.CreatedAt,
            user.UpdatedAt
        );
        return Ok(userResponse);
    }

    [HttpGet("{id:guid}/tasks")]
    public async Task<ActionResult<UserWithTasksResponse>> GetUserWithTasks(Guid id)
    {
        var user = await _dataService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        var tasks = await _dataService.GetTasksByUserAsync(id);
        var taskResponses = new List<TaskResponse>();

        foreach (var task in tasks)
        {
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            var assignmentHistory = new List<AssignmentHistoryResponse>();
            foreach (var history in task.AssignmentHistory)
            {
                var historyUser = await _dataService.GetUserByIdAsync(history.UserId);
                if (historyUser != null)
                {
                    assignmentHistory.Add(new AssignmentHistoryResponse(
                        history.UserId,
                        historyUser.Name,
                        history.AssignedAt
                    ));
                }
            }

            taskResponses.Add(new TaskResponse(
                task.Id,
                task.Title,
                task.State,
                task.AssignedToUserId,
                user.Name,
                task.PreviousAssignedUserId,
                previousAssignedUserName,
                task.IsEligibleForReassignment,
                assignmentHistory,
                task.CreatedAt,
                task.UpdatedAt
            ));
        }

        var userWithTasks = new UserWithTasksResponse(
            user.Id,
            user.Name,
            user.CreatedAt,
            user.UpdatedAt,
            taskResponses
        );
        return Ok(userWithTasks);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("User name is required.");

        try
        {
            var user = await _dataService.CreateUserAsync(request.Name.Trim());
            var userResponse = new UserResponse(
                user.Id,
                user.Name,
                user.CreatedAt,
                user.UpdatedAt
            );
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("User name is required.");

        try
        {
            var user = await _dataService.UpdateUserAsync(id, request.Name.Trim());
            if (user == null)
                return NotFound($"User with ID {id} not found.");

            var userResponse = new UserResponse(
                user.Id,
                user.Name,
                user.CreatedAt,
                user.UpdatedAt
            );
            return Ok(userResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var deleted = await _dataService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound($"User with ID {id} not found.");

        return NoContent();
    }
}
