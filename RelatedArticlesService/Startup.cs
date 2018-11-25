using System;
using ElasticsearchClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RelatedArticlesService
{
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
            // use dependency injection to inject article client for Elasticsearch API with given base address
            // TODO read uri from config
            services.AddSingleton<IArticleClient>(new ElasticsearchArticleClient(new Uri("http://localhost:9200/")));

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