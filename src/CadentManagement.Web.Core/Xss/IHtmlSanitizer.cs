using Abp.Dependency;

namespace CadentManagement.Web.Xss;

public interface IHtmlSanitizer : ITransientDependency
{
    string Sanitize(string html);
}

