using System;
using ElasticsearchClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace RelatedArticlesService
{
    public class Startup
    {
        /// <summary>
        /// NLog log manager.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string elasticsearchApiBaseAddress;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // read Elasticsearch API base address from appsettings.json config
            elasticsearchApiBaseAddress = configuration["ElasticsearchApiBaseAddress"];
            if (string.IsNullOrEmpty(elasticsearchApiBaseAddress))
            {
                Logger.Error("ElasticsearchApiBaseAddress is not provided in configuration file. Default http://localhost:9200/ will be used.");
                elasticsearchApiBaseAddress = "http://localhost:9200/";
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Logger.Info("Related Articles Web API is starting up, services are being configured.");

            // use dependency injection to inject article client for Elasticsearch API
            var baseAddress = new Uri(elasticsearchApiBaseAddress);
            services.AddSingleton<IArticleClient>(new ElasticsearchArticleClient(baseAddress));

            // use mock article client for testing purposes (stores articles in memory instead of Elasticsearch) 
            //services.AddSingleton<IArticleClient>(new MockArticleClient());

            services.AddMvc();
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