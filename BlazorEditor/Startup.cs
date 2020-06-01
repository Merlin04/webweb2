using System;
using System.IO;
using System.Linq;
using System.Net;
using BlazorEditor.Data;
using BlazorEditor.GitHub;
using BlazorEditor.Identity;
using BlazorEditor.Shared;
using BlazorEditor.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorEditor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("ApplicationDbContext")));
            services.AddScoped<OMCSService>();
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<GitHubService>();
            // https://dev.to/j_sakamoto/how-can-i-protect-static-files-with-authorization-on-asp-net-core-4l0o
            services.Configure<StaticFileOptions>(options =>
            {
                options.OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name == "blazor.server.js" && ctx.File.PhysicalPath is null) return;
                    string path = MiscUtils.UnmapPath(ctx.File.PhysicalPath);
                    string pathFirst = path.Split('/').First();
                    if (!ctx.Context.User.Identity.IsAuthenticated && pathFirst != "webwebResources" &&
                        pathFirst != "favicon.ico")
                    {
                        // The user should not be accessing this file
                        ctx.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}