using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.ZooKeeperNet.WebAppCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //加载log4net日志配置文件
            XmlConfigurator.Configure(LogManager.CreateRepository("log4net-default-repository"), new System.IO.FileInfo("Configs/log4net.config"));
            Nodis.RedisPoolBuilder.Init("192.168.4.77:2181,192.168.4.78:2181,192.168.4.79:2181", "codis-mango", 5);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
