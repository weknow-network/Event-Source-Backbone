using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.WebEventTest
{
    public class Startup
    {
        const string VERSION = "V1";
        const string TITLE = "Event Source";
        const string DESC = "";
        private readonly IHostEnvironment _hostEnv;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="configuration">The configuration.</param>
        public Startup(
            IHostEnvironment hostEnv,
            IConfiguration configuration)
        {
            _hostEnv = hostEnv;
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStandardWeknow(_hostEnv)
                    .AddOpenAPIWeknow(VERSION, TITLE, DESC);


            services.AddSingleton(ioc =>
            {
                ILogger<Startup> logger = ioc.GetService<ILogger<Startup>>() ?? throw new ArgumentNullException();
                IEventFlowProducer producer = ProducerBuilder.Empty.UseRedisChannelInjection(ioc)
                                     .AddS3Strategy()
                                     .Partition("demo")
                                     .Shard("default")
                                     .WithLogger(logger)
                                     .BuildEventFlowProducer();
                return producer;
            });
            services.AddSingleton(ioc =>
            {
                IConsumerLoggerBuilder consumer =
                            ConsumerBuilder.Empty.UseRedisChannelInjection(ioc)
                                     .AddS3Strategy()
                                     .WithOptions(o => o with { TraceAsParent = TimeSpan.FromMinutes(10) })
                                     .Partition("demo")
                                     .Shard("default");
                return consumer;
            });

            services.AddControllers()
                            .WithStandardWeknow();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<Startup> logger)
        {
            app.ConfigureWeknow(env, logger);
        }
    }
}
