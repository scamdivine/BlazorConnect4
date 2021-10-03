﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorConnect4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("./Data");
            }
            //CreateHostBuilder(args).Build().Run();

            InitiateNavySealTrainingProtocol();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void InitiateNavySealTrainingProtocol()
        {
            Console.WriteLine("Started Navy Seal Training Protocol");
            AIModels.QAgent red;
            AIModels.QAgent yellow;

            if (File.Exists("Data/Q1Red.bin"))
            {
                red = AIModels.QAgent.ConstructFromFile("Data/Q1Red.bin");
            }
            else
            {
                red = new AIModels.QAgent(Model.CellColor.Red);
                red.ToFile("Data/Q1Red.bin");
            }
            if (File.Exists("Data/Q1Yellow.bin"))
            {
                yellow = AIModels.QAgent.ConstructFromFile("Data/Q1Yellow.bin");
            }
            else
            {
                yellow = new AIModels.QAgent(Model.CellColor.Yellow);
                yellow.ToFile("Data/Q1Yellow.bin");
            }
            red.Trainer(100, yellow);
            red.ToFile("Data/Q1Red.bin");
        }
    }
}
