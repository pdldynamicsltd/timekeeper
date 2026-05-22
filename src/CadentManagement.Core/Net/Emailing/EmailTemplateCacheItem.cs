using System;

namespace CadentManagement.Net.Emailing;

[Serializable]
public class EmailTemplateCacheItem
{
    public const string CacheName = "AppEmailTemplateCache";

    public string Template { get; private set; }

    public EmailTemplateCacheItem(string template)
    {
        Template = template;
    }
}