using AspNetCoreRateLimit;
using HotelListing.Data;
using HotelListing.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelListing
{
    public static class ServiceExtensions
    {
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<ApiUser>(q => q.User.RequireUniqueEmail = true);
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
            builder.AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();
        }

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Environment.GetEnvironmentVariable("KEY");

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        //ValidAudience = jwtSettings.GetSection("audience").Value,
                        ValidIssuer = jwtSettings.GetSection("issuer").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                    o.Audience = jwtSettings.GetSection("audience").Value;
                });
        }

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if(contextFeature != null)
                    {
                        Log.Error($"Something went wrong... {contextFeature.Error}");
                        await context.Response.WriteAsync(new Error
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Message = "Internal Server Error. Please try again later."
                        }.ToString());
                    }
                });
            });
        }

        public static void ConfigureApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = ApiVersion.Default;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("api-version"));
            });
            services.AddVersionedApiExplorer(options => 
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services)
        {
            services.AddResponseCaching();
            // Also handles Age, Last-Modified, Expires, Cache-Control and ETag header
            services.AddHttpCacheHeaders(
                expirationOptions => {
                    expirationOptions.MaxAge = 60;
                    expirationOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Public;
                },
                validationOptions => {
                    validationOptions.MustRevalidate = true;
                });
        }

        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            var rateLimitRules = new List<RateLimitRule> {
                // Limit every endpoint to 1 call per 5 seconds
                new RateLimitRule { Endpoint = "*", Limit = 1, Period = "5s" }
            };

            services.Configure<IpRateLimitOptions>(o => {
                o.GeneralRules = rateLimitRules;
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
    }
}
