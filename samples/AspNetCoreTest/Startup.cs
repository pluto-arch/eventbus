using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Event;
using Newtonsoft.Json;
using Pluto.EventBus.Abstract;

namespace AspNetCoreTest
{

    public class NewtonsoftMessageSerializeProvider : IMessageSerializeProvider
    {
        /// <inheritdoc />
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <inheritdoc />
        public T Deserialize<T>(string objStr)
        {
            return JsonConvert.DeserializeObject<T>(objStr);
        }

        /// <inheritdoc />
        public object Deserialize(string objStr, Type type)
        {
            return JsonConvert.DeserializeObject(objStr, type);
        }
    }


    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //services.AddDbContext<DemoDbContext>();

            services.AddTransient<UserEventHandler>();
            services.AddTransient<DemoEventHandler>();

            services.AddSingleton<IMessageSerializeProvider, NewtonsoftMessageSerializeProvider>();

            
            services.AddUserEventBus();
            services.AddAdminEventBus();

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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
