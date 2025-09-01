# Original Requirements

## Assignment:
Create a simple web API to manage Users and Tasks.

- Users are defined by a unique Name.

- Tasks must have a unique Title and a State (Waiting, InProgress, Completed).

- Users can have multiple tasks, but not more than 3 at the same time.

- Each Task can only be assigned to one User.

- When a Task is created, it should automatically be assigned to a User (if available). If no Users are available, the Task stays in Waiting.

- Users can be deleted (you may define your own rules and limitations for edge cases).

## Reassignment rule:

Every 2 minutes, all Tasks should be reassigned to another random User.

### Rules:

- It cannot be the same User currently assigned.

- It cannot be the User assigned in the previous round.

- It can be a User assigned two or more iterations before.

- If no Users are available, the Task remains in Waiting.

- All Tasks must eventually be transferred to all Users at least once (including Users created after the Task, unless the Task is already Completed).

- Once a Task has been assigned to all existing Users, it should be marked Completed and remain unassigned.

## Notes:

- No UI is required.

- The requirements are intentionally loose—you can decide on implementation details.

- Please make sure the solution is easy to deploy/run (debug).

- Comments in the code explaining your decisions (logic, technology used, etc.) are welcome.

## Key Points to Keep in Mind for Your Assignment

- **Keep it focused and clear.** Avoid adding extra or unnecessary elements that aren't part of the task. The team values straightforward, problem-focused solutions rather than overcomplicated ones.

- **Make sure it runs out of the box.** Your project should be easy to set up and run without manual fixes. Include everything needed (like database setup, endpoints, and configs) so it works smoothly when tested.

- **Cover all required functionality.** Double-check that all endpoints and features requested in the task are implemented. Missing parts (e.g., certain GET endpoints) make it hard for the team to evaluate your solution.

- **Show your own thinking.** The team looks for critical thinking and personal problem-solving, not just generic or AI-generated code. If you use tools for help, make sure the final solution clearly reflects your own understanding and approach.

- **Stick closely to the assignment guidelines.** Every part of your logic should follow what's asked. If you take a different approach, explain why in comments or documentation.

- **Aim for efficiency and simplicity.** The logic, especially in areas like task reassignments, should be clean and efficient. Avoid unnecessary complexity—clear, maintainable code is preferred over "clever" but convoluted solutions.

- **No unused code.** Make sure the final version only contains what's needed for the task. Extra or leftover code creates noise and makes the solution harder to assess.
