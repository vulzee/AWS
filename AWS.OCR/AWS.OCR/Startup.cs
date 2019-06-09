﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AWS.OCR.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Net;

namespace AWS.OCR
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
			// Configure default endpoint limit
			// This is the value used by AWS SDK, but by explicitly setting it here
			// we are hoping to work around a deadlock bug in .NET FX being encountered
			// See https://github.com/dotnet/corefx/issues/21796
			ServicePointManager.DefaultConnectionLimit = 50;

			services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

			services.AddDataProtection()
				.PersistKeysToDbContext<ApplicationDbContext>();

			services.AddDbContext<ApplicationDbContext>(options =>
              //  options.UseInMemoryDatabase("AWS_OCR"));
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var provider = services.BuildServiceProvider();
            var dbContext = provider.GetService<ApplicationDbContext>();
            var awsAccess = dbContext.AwsAccesses.FirstOrDefault();
            if(awsAccess == null)
            {
                awsAccess = new AwsAccess
                {
                    AwsAccessKeyID = Configuration.GetValue<string>("awsaccessKeyID", null),
                    AwsSecreteAccessKey = Configuration.GetValue<string>("awsSecreteAccessKey", null),
                    Region = Configuration.GetValue<string>("awsRegion", null),
                    Token = Configuration.GetValue<string>("awsToken", null),
                    S3BucketName = Configuration.GetValue<string>("s3BucketName", null),
                };
                dbContext.Add(awsAccess);
            }
            dbContext.SaveChanges();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
