using System.IO;
using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CadentManagement.Web.Public.Startup;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseCastleWindsor(IocManager.Instance.IocContainer)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(opt => opt.AddServerHeader = false);
                webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                webBuilder.UseIIS();
                webBuilder.UseStartup<Startup>();
            });
    }
}
