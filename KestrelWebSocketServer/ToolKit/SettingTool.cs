using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace KestrelWebSocketServer
{
    public class SettingTool
    {
        public static IConfigurationRoot AddServerOptionsJsonFile(string fileName = "appsettings.json")
        {
            return new ConfigurationBuilder()
                       .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                       .AddJsonFile(fileName)
                       .Build();
        }

    }
}
