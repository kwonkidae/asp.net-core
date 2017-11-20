using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using db;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Agent.ConString = "Data Source=192.168.0.17;Initial Catalog=toylib;Persist Security Info=True;User ID=sa;Password=nicom123.";
            Agent.DefaultCommandType = System.Data.CommandType.Text;
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
