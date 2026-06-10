using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;
using CadentManagement.UserTasks;
using CadentManagement.UserTasks.Dto;

namespace CadentManagement.Tests.UserTasks;

public class UserTaskAppService_Tests : AppTestBase
{
    private readonly IUserTaskAppService _userTaskAppService;

    public UserTaskAppService_Tests()
    {
        _userTaskAppService = Resolve<IUserTaskAppService>();
    }

    [MultiTenantFact]
    public async Task Should_Create_UserTask()
    {
        var input = new CreateOrEditUserTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Medium,
            Status = KanbanTaskStatus.Todo,
            EstimatedMinutes = 60,
            DueDate = DateTime.Now.AddDays(7)
        };

        await _userTaskAppService.CreateAsync(input);

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Test Task");
            task.ShouldNotBeNull();
            task.Description.ShouldBe("Test Description");
            task.Priority.ShouldBe(TaskPriority.Medium);
            task.Status.ShouldBe(KanbanTaskStatus.Todo);
            task.EstimatedMinutes.ShouldBe(60);
        });
    }

    [MultiTenantFact]
    public async Task Should_Update_UserTask()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog,
            EstimatedMinutes = 30
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Original Title");
            taskId = task.Id;
        });

        await _userTaskAppService.UpdateAsync(new CreateOrEditUserTaskDto
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.InProgress,
            EstimatedMinutes = 60
        });

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            task.Title.ShouldBe("Updated Title");
            task.Description.ShouldBe("Updated Description");
            task.Priority.ShouldBe(TaskPriority.High);
            task.Status.ShouldBe(KanbanTaskStatus.InProgress);
            task.EstimatedMinutes.ShouldBe(60);
        });
    }

    [MultiTenantFact]
    public async Task Should_Delete_UserTask()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Task to Delete",
            Description = "This will be deleted",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task to Delete");
            taskId = task.Id;
        });

        await _userTaskAppService.DeleteAsync(new EntityDto(taskId));

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == taskId);
            task.ShouldNotBeNull();
            task.IsDeleted.ShouldBeTrue();
        });
    }

    [MultiTenantFact]
    public async Task Should_Get_UserTasks()
    {
        for (var i = 0; i < 3; i++)
        {
            await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
            {
                Title = $"Task {i}",
                Priority = TaskPriority.Medium,
                Status = KanbanTaskStatus.Todo
            });
        }

        var result = await _userTaskAppService.GetTasksAsync(new GetUserTasksInput());

        result.Items.Count.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.Any(t => t.Title.StartsWith("Task")).ShouldBeTrue();
    }

    [MultiTenantFact]
    public async Task Should_Update_Task_Status()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Task for Status Update",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task for Status Update");
            taskId = task.Id;
        });

        await _userTaskAppService.UpdateStatusAsync(new UpdateTaskStatusInput
        {
            TaskId = taskId,
            NewStatus = (int)KanbanTaskStatus.Done,
            NewSortOrder = 0
        });

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            task.Status.ShouldBe(KanbanTaskStatus.Done);
            task.CompletedAt.ShouldNotBeNull();
        });
    }

    [MultiTenantFact]
    public async Task Should_Set_CompletedAt_When_Creating_Done_Task()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Done On Create",
            Priority = TaskPriority.Medium,
            Status = KanbanTaskStatus.Done
        });

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Done On Create");
            task.ShouldNotBeNull();
            task.Status.ShouldBe(KanbanTaskStatus.Done);
            task.CompletedAt.ShouldNotBeNull();
        });
    }

    [MultiTenantFact]
    public async Task Should_Clear_CompletedAt_When_Updating_Task_To_NonDone_Status()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Done Then Reopened",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.Done
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Done Then Reopened");
            taskId = task.Id;
            task.CompletedAt.ShouldNotBeNull();
        });

        await _userTaskAppService.UpdateAsync(new CreateOrEditUserTaskDto
        {
            Id = taskId,
            Title = "Done Then Reopened",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.InProgress
        });

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            task.Status.ShouldBe(KanbanTaskStatus.InProgress);
            task.CompletedAt.ShouldBeNull();
        });
    }

    [MultiTenantFact]
    public async Task Should_Get_UserTask_For_Edit()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Task for Edit",
            Description = "Edit me",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.Todo
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task for Edit");
            taskId = task.Id;
        });

        var result = await _userTaskAppService.GetForEditAsync(new NullableIdDto<int>(taskId));

        result.Task.ShouldNotBeNull();
        result.Task.Title.ShouldBe("Task for Edit");
        result.Task.Description.ShouldBe("Edit me");
        result.Task.Priority.ShouldBe(TaskPriority.High);
    }

    [MultiTenantFact]
    public async Task Should_Get_Empty_UserTask_For_Create()
    {
        var result = await _userTaskAppService.GetForEditAsync(new NullableIdDto<int>(null));

        result.Task.ShouldNotBeNull();
        result.Task.Id.ShouldBeNull();
        result.Task.Title.ShouldBeNull();
    }

    [MultiTenantFact]
    public async Task Should_Create_And_Update_Todo_Status()
    {
        var createdId = await _userTaskAppService.CreateTodoStatusAsync(new CreateOrEditTodoStatusDto
        {
            Name = "Blocked",
            Color = "#ff0000",
            SortOrder = 95,
            IsCompleted = false
        });

        await _userTaskAppService.UpdateTodoStatusAsync(new CreateOrEditTodoStatusDto
        {
            Id = createdId,
            Name = "Blocked By Dependency",
            Color = "#cc0000",
            SortOrder = 96,
            IsCompleted = false
        });

        var statuses = await _userTaskAppService.GetTodoStatusesAsync();
        var blocked = statuses.FirstOrDefault(s => s.Id == createdId);
        blocked.ShouldNotBeNull();
        blocked.Name.ShouldBe("Blocked By Dependency");
        blocked.Color.ShouldBe("#cc0000");
        blocked.SortOrder.ShouldBe(96);
    }

    [MultiTenantFact]
    public async Task Should_Complete_Task_From_Quick_Action()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Quick Complete",
            Priority = TaskPriority.Medium,
            Status = KanbanTaskStatus.Todo
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Quick Complete");
            taskId = task.Id;
        });

        await _userTaskAppService.CompleteAsync(new EntityDto<int>(taskId));

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            task.Status.ShouldBe(KanbanTaskStatus.Done);
            task.CompletedAt.ShouldNotBeNull();
        });
    }

    [MultiTenantFact]
    public async Task Should_Update_Task_DueDate()
    {
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Planner Due Date",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Todo
        });

        var taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Planner Due Date");
            taskId = task.Id;
        });

        var dueDate = DateTime.Today.AddDays(3);
        await _userTaskAppService.UpdateDueDateAsync(new UpdateTaskDueDateInput
        {
            TaskId = taskId,
            DueDate = dueDate
        });

        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Id == taskId);
            task.DueDate.ShouldNotBeNull();
            task.DueDate.Value.Date.ShouldBe(dueDate);
        });
    }
}