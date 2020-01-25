using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using DataServices;
using VirtualRadarServer.Models;
using Newtonsoft.Json.Serialization;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //https://github.com/aspnet/Mvc/issues/4842
            services.AddMvc().AddControllersAsServices().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            var builder = new ContainerBuilder();
            builder.Populate(services);

            var connectionString = "data source=App_Data/StandingData.sqb";

            builder.Register(c =>
            {
                StandingDataContainer context = new MyStandingDataContainer(connectionString);
                return context;
            }).As<StandingDataContainer>();

            builder.RegisterType<DataServices.DataServices>().As<IDataServices>();
            builder.RegisterType<BusinessServices.AirportBusinessService>();

            var container = builder.Build();
            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //https://dotnetcoretutorials.com/2017/04/28/serving-static-files-outside-wwwroot-asp-net-core/
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Content")),
                RequestPath = new PathString("/Content")
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Scripts")),
                RequestPath = new PathString("/Scripts")
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "ExplorerArea", "{area:exists}/{controller=Explorer}/{action=Index}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
