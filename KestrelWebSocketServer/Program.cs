using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KestrelWebSocketServer
{
    public class Program
    {
        public static IConfigurationRoot configuration { get => SettingTool.AddServerOptionsJsonFile(); }

        public static async Task Main(string[] args)
        {
            var ip = configuration.GetValue<string>("ServerOptions:IP");
            var port = configuration.GetValue<int>("ServerOptions:Port");
            var path = configuration.GetValue<string>("ServerOptions:Path");

            await Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseKestrel();
                            webBuilder.UseUrls(new string[] { $@"http://{ip}:{port}{path}" });
                            webBuilder.UseStartup<Startup>();
                        })
                        .Build()
                        .RunAsync();

            Console.ReadLine();
        }

    }
}
