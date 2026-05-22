using Abp.Notifications;
using System;

namespace CadentManagement.Web.Areas.App.Models.Notifications;

public class NotificationDetailViewModel
{
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationSeverity Severity { get; set; }
    public DateTime CreationTime { get; set; }
}


