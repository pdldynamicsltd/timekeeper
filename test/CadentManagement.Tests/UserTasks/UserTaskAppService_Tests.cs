using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Shouldly;
using Xunit;
using CadentManagement.UserTasks;
using CadentManagement.UserTasks.Dto;
using Microsoft.EntityFrameworkCore;

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
        // Arrange
        var input = new CreateOrEditUserTaskDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Medium,
            Status = KanbanTaskStatus.Todo,
            EstimatedMinutes = 60,
            DueDate = DateTime.Now.AddDays(7)
        };

        // Act
        await _userTaskAppService.CreateAsync(input);

        // Assert
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
        // Arrange
        var createInput = new CreateOrEditUserTaskDto
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog,
            EstimatedMinutes = 30
        };

        await _userTaskAppService.CreateAsync(createInput);

        int taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Original Title");
            taskId = task.Id;
        });

        var updateInput = new CreateOrEditUserTaskDto
        {
            Id = taskId,
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.InProgress,
            EstimatedMinutes = 60
        };

        // Act
        await _userTaskAppService.UpdateAsync(updateInput);

        // Assert
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
        // Arrange
        var createInput = new CreateOrEditUserTaskDto
        {
            Title = "Task to Delete",
            Description = "This will be deleted",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog
        };

        await _userTaskAppService.CreateAsync(createInput);

        int taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task to Delete");
            taskId = task.Id;
        });

        // Act
        await _userTaskAppService.DeleteAsync(new EntityDto(taskId));

        // Assert
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
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var input = new CreateOrEditUserTaskDto
            {
                Title = $"Task {i}",
                Priority = TaskPriority.Medium,
                Status = KanbanTaskStatus.Todo
            };
            await _userTaskAppService.CreateAsync(input);
        }

        // Act
        var result = await _userTaskAppService.GetTasksAsync(new GetUserTasksInput());

        // Assert
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.Any(t => t.Title.StartsWith("Task")).ShouldBeTrue();
    }

    [MultiTenantFact]
    public async Task Should_Update_Task_Status()
    {
        // Arrange
        var createInput = new CreateOrEditUserTaskDto
        {
            Title = "Task for Status Update",
            Priority = TaskPriority.Low,
            Status = KanbanTaskStatus.Backlog
        };

        await _userTaskAppService.CreateAsync(createInput);

        int taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task for Status Update");
            taskId = task.Id;
        });

        // Act
        await _userTaskAppService.UpdateStatusAsync(new UpdateTaskStatusInput
        {
            TaskId = taskId,
            NewStatus = KanbanTaskStatus.Done,
            NewSortOrder = 0
        });

        // Assert
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
        // Arrange
        var input = new CreateOrEditUserTaskDto
        {
            Title = "Done On Create",
            Priority = TaskPriority.Medium,
            Status = KanbanTaskStatus.Done
        };

        // Act
        await _userTaskAppService.CreateAsync(input);

        // Assert
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
        // Arrange
        await _userTaskAppService.CreateAsync(new CreateOrEditUserTaskDto
        {
            Title = "Done Then Reopened",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.Done
        });

        int taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Done Then Reopened");
            taskId = task.Id;
            task.CompletedAt.ShouldNotBeNull();
        });

        // Act
        await _userTaskAppService.UpdateAsync(new CreateOrEditUserTaskDto
        {
            Id = taskId,
            Title = "Done Then Reopened",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.InProgress
        });

        // Assert
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
        // Arrange
        var createInput = new CreateOrEditUserTaskDto
        {
            Title = "Task for Edit",
            Description = "Edit me",
            Priority = TaskPriority.High,
            Status = KanbanTaskStatus.Todo
        };

        await _userTaskAppService.CreateAsync(createInput);

        int taskId = 0;
        await UsingDbContextAsync(async context =>
        {
            var task = await context.UserTasks.FirstOrDefaultAsync(t => t.Title == "Task for Edit");
            taskId = task.Id;
        });

        // Act
        var result = await _userTaskAppService.GetForEditAsync(new NullableIdDto<int>(taskId));

        // Assert
        result.Task.ShouldNotBeNull();
        result.Task.Title.ShouldBe("Task for Edit");
        result.Task.Description.ShouldBe("Edit me");
        result.Task.Priority.ShouldBe(TaskPriority.High);
    }

    [MultiTenantFact]
    public async Task Should_Get_Empty_UserTask_For_Create()
    {
        // Act
        var result = await _userTaskAppService.GetForEditAsync(new NullableIdDto<int>(null));

        // Assert
        result.Task.ShouldNotBeNull();
        result.Task.Id.ShouldBeNull();
        result.Task.Title.ShouldBeNull();
    }
}
