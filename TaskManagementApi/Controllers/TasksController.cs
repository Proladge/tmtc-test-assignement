namespace TaskManagementApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Services;
using TaskManagementApi.DTOs;
using TaskManagementApi.Models;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IDataService _dataService;

    public TasksController(IDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAllTasks()
    {
        var tasks = await _dataService.GetAllTasksAsync();
        var taskResponses = new List<TaskResponse>();

        foreach (var task in tasks)
        {
            string? assignedUserName = null;
            if (task.AssignedToUserId.HasValue)
            {
                var user = await _dataService.GetUserByIdAsync(task.AssignedToUserId.Value);
                assignedUserName = user?.Name;
            }

            // Get previous assigned user name
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            // Build assignment history
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
                assignedUserName,
                task.PreviousAssignedUserId,
                previousAssignedUserName,
                task.IsEligibleForReassignment,
                assignmentHistory,
                task.CreatedAt,
                task.UpdatedAt
            ));
        }

        return Ok(taskResponses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetTask(Guid id)
    {
        var task = await _dataService.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound($"Task with ID {id} not found.");

        string? assignedUserName = null;
        if (task.AssignedToUserId.HasValue)
        {
            var user = await _dataService.GetUserByIdAsync(task.AssignedToUserId.Value);
            assignedUserName = user?.Name;
        }

        // Get previous assigned user name
        string? previousAssignedUserName = null;
        if (task.PreviousAssignedUserId.HasValue)
        {
            var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
            previousAssignedUserName = previousUser?.Name;
        }

        // Build assignment history
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

        var taskResponse = new TaskResponse(
            task.Id,
            task.Title,
            task.State,
            task.AssignedToUserId,
            assignedUserName,
            task.PreviousAssignedUserId,
            previousAssignedUserName,
            task.IsEligibleForReassignment,
            assignmentHistory,
            task.CreatedAt,
            task.UpdatedAt
        );
        return Ok(taskResponse);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Task title is required.");

        try
        {
            var task = await _dataService.CreateTaskAsync(request.Title.Trim());
            
            string? assignedUserName = null;
            if (task.AssignedToUserId.HasValue)
            {
                var user = await _dataService.GetUserByIdAsync(task.AssignedToUserId.Value);
                assignedUserName = user?.Name;
            }

            // Get previous assigned user name
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            // Build assignment history
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

            var taskResponse = new TaskResponse(
                task.Id,
                task.Title,
                task.State,
                task.AssignedToUserId,
                assignedUserName,
                task.PreviousAssignedUserId,
                previousAssignedUserName,
                task.IsEligibleForReassignment,
                assignmentHistory,
                task.CreatedAt,
                task.UpdatedAt
            );
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            // Validate assignment if provided
            if (request.AssignedToUserId.HasValue)
            {
                var targetUser = await _dataService.GetUserByIdAsync(request.AssignedToUserId.Value);
                if (targetUser == null)
                    return BadRequest($"User with ID {request.AssignedToUserId.Value} not found.");
            }

            var task = await _dataService.UpdateTaskAsync(
                id,
                request.Title?.Trim(),
                request.State,
                request.AssignedToUserId
            );

            if (task == null)
                return NotFound($"Task with ID {id} not found.");

            string? assignedUserName = null;
            if (task.AssignedToUserId.HasValue)
            {
                var user = await _dataService.GetUserByIdAsync(task.AssignedToUserId.Value);
                assignedUserName = user?.Name;
            }

            // Get previous assigned user name
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            // Build assignment history
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

            var taskResponse = new TaskResponse(
                task.Id,
                task.Title,
                task.State,
                task.AssignedToUserId,
                assignedUserName,
                task.PreviousAssignedUserId,
                previousAssignedUserName,
                task.IsEligibleForReassignment,
                assignmentHistory,
                task.CreatedAt,
                task.UpdatedAt
            );
            return Ok(taskResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var deleted = await _dataService.DeleteTaskAsync(id);
        if (!deleted)
            return NotFound($"Task with ID {id} not found.");

        return NoContent();
    }

    [HttpPost("{id:guid}/assign/{userId:guid}")]
    public async Task<ActionResult<TaskResponse>> AssignTask(Guid id, Guid userId)
    {
        try
        {
            // Check if user exists
            var user = await _dataService.GetUserByIdAsync(userId);
            if (user == null)
                return BadRequest($"User with ID {userId} not found.");

            // Check if user can accept more tasks
            if (!await _dataService.CanUserAcceptMoreTasksAsync(userId))
                return BadRequest($"User '{user.Name}' already has the maximum number of tasks (3).");

            var task = await _dataService.UpdateTaskAsync(id, assignedToUserId: userId);
            if (task == null)
                return NotFound($"Task with ID {id} not found.");

            // Get previous assigned user name
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            // Build assignment history
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

            var taskResponse = new TaskResponse(
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
            );
            return Ok(taskResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/unassign")]
    public async Task<ActionResult<TaskResponse>> UnassignTask(Guid id)
    {
        try
        {
            var task = await _dataService.UpdateTaskAsync(id, assignedToUserId: null);
            if (task == null)
                return NotFound($"Task with ID {id} not found.");

            // Set state to Waiting if it was InProgress
            if (task.State == TaskState.InProgress)
            {
                task = await _dataService.UpdateTaskAsync(id, state: TaskState.Waiting);
            }

            // Get previous assigned user name
            string? previousAssignedUserName = null;
            if (task.PreviousAssignedUserId.HasValue)
            {
                var previousUser = await _dataService.GetUserByIdAsync(task.PreviousAssignedUserId.Value);
                previousAssignedUserName = previousUser?.Name;
            }

            // Build assignment history
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

            var taskResponse = new TaskResponse(
                task.Id,
                task.Title,
                task.State,
                task.AssignedToUserId,
                null,
                task.PreviousAssignedUserId,
                previousAssignedUserName,
                task.IsEligibleForReassignment,
                assignmentHistory,
                task.CreatedAt,
                task.UpdatedAt
            );
            return Ok(taskResponse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
