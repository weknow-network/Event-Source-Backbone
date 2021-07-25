using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.WebEventTest.Jobs;

namespace Weknow.EventSource.Backbone.WebEventTest
{
    public class Program
    {
        private static readonly WeknowHosting<Startup> _hosting = new WeknowHosting<Startup>();

        public static void Main(string[] args)
        {
            _hosting.HostRest(args, hostBuilder => hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<MicroDemoJob>();
            }));
        }
    }
}
