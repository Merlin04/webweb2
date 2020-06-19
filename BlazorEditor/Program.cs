using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlazorEditor.Data;
using BlazorEditor.Utils;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorEditor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            CreateDbIfNotExists(host);

            host.Run();
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                ConfigItem testItem = context.Configuration.FirstOrDefault();
                if (testItem is null)
                {
                    // Add initial items to DB
                    new List<string> {"github-token", "github-reponame", "github-destination", "assemblies", "deploy-method", "localfs-path"}.ForEach(
                        c => context.Configuration.Add(new ConfigItem {Title = c, Contents = string.Empty}));
                    context.Configuration.Find("assemblies").Contents = "System.Collections";
                    context.Pages.Add(new Page
                    {
                        Title = "index",
                        HtmlContents = InitialItemValues.PageValues[0],
                        CssContents = InitialItemValues.PageValues[1],
                        JsContents = InitialItemValues.PageValues[2]
                    });
                    context.Components.Add(new Component
                    {
                        Title = "TestComponent",
                        HtmlContents = InitialItemValues.ComponentValues[0],
                        CssContents = InitialItemValues.ComponentValues[1],
                        JsContents = InitialItemValues.ComponentValues[2]
                    });
                    context.Layouts.Add(new Layout
                    {
                        Title = "_Layout",
                        Contents = InitialItemValues.LayoutValues[0]
                    });
                }

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured creating the DB.");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}