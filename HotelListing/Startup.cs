using AspNetCoreRateLimit;
using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.IRepository;
using HotelListing.Repository;
using HotelListing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace HotelListing
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
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("sqlConnection")));

            services.AddMemoryCache();  // This is needed for AspNetCoreRateLimit to work
            services.ConfigureRateLimiting();
            services.AddHttpContextAccessor();  // This is needed for AspNetCoreRateLimit to work

            services.ConfigureHttpCacheHeaders();

            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJwt(Configuration);

            services.AddCors(o =>
            {
                o.AddPolicy("AllowEverything", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
            });

            services.AddAutoMapper(typeof(AutoMapperInitializer));

            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthManager, AuthManager>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing", Version = "1.0", });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "Hotel Listing", Version = "2.0" });
            });

            services.AddControllers()
                // The first time an object is encountered it will be serialized as usual but if the object is encountered as a child object of itself the serializer will skip serializing it.
                // Needed for 'includes' to work properly
                .AddNewtonsoftJson(op => op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.ConfigureApiVersioning();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {                
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            app.UseCors("AllowEverything");

            app.UseResponseCaching();
            app.UseHttpCacheHeaders();

            app.UseIpRateLimiting();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
