using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace compUpload
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((serverOptions) =>
                    {
                        serverOptions.Limits.MaxRequestBodySize = 1610612736;
                        serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15);
                        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(15);
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
