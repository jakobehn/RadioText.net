using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orneholm.RadioText.Core.Storage;
using Orneholm.RadioText.Web.Models;
using CloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;

namespace Orneholm.RadioText.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(x =>
            {
                var storageAccount = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(Configuration["AzureStorage:ConnectionString"]);
                return storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            });

            services.AddTransient<ISummaryStorage, SummaryAzureTableStorage>(s => new SummaryAzureTableStorage(
                s.GetRequiredService<CloudTableClient>(),
                Configuration["AzureStorage:EpisodeSummaryTableName"]
            ));

            services.Configure<GoogleAnalyticsOptions>(Configuration);
            services.Configure<ImmersiveReaderOptions>(Configuration.GetSection("ImmersiveReader"));


            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation();
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = GetContentTypeProvider()
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }

        private static FileExtensionContentTypeProvider GetContentTypeProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            return provider;
        }
    }
}
