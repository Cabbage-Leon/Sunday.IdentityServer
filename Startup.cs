// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sunday.IdentityServer.Authorization;
using Sunday.IdentityServer.Data;
using Sunday.IdentityServer.Extensions;
using Sunday.IdentityServer.Models;

namespace Sunday.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSameSiteCookiePolicy();

            string connectionStringFile = Configuration.GetConnectionString("DefaultConnection_file");
            var connectionString = File.Exists(connectionStringFile) ? File.ReadAllText(connectionStringFile).Trim() : Configuration.GetConnectionString("DefaultConnection");
            //使用NET CORE 内置的IdentityUser，ApplicationDbContext是继承自他
            services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString));

            //启用 Identity 服务 添加指定的用户和角色类型的默认标识系统配置
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User = new UserOptions
                {
                    RequireUniqueEmail = true, //要求Email唯一
                    AllowedUserNameCharacters = null //允许的用户名字符
                };
                options.Password = new PasswordOptions
                {
                    RequiredLength = 8, //要求密码最小长度，默认是 6 个字符
                    RequireDigit = true, //要求有数字
                    RequiredUniqueChars = 3, //要求至少要出现的字母数
                    RequireLowercase = true, //要求小写字母
                    RequireNonAlphanumeric = false, //要求特殊字符
                    RequireUppercase = false //要求大写字母
                };

                //options.Lockout = new LockoutOptions
                //{
                //    AllowedForNewUsers = true, // 新用户锁定账户
                //    DefaultLockoutTimeSpan = TimeSpan.FromHours(1), //锁定时长，默认是 5 分钟
                //    MaxFailedAccessAttempts = 3 //登录错误最大尝试次数，默认 5 次
                //};
                //options.SignIn = new SignInOptions
                //{
                //    RequireConfirmedEmail = true, //要求激活邮箱
                //    RequireConfirmedPhoneNumber = true //要求激活手机号
                //};
                //options.ClaimsIdentity = new ClaimsIdentityOptions
                //{
                //    // 这里都是修改相应的Cliams声明的
                //    RoleClaimType = "IdentityRole",
                //    UserIdClaimType = "IdentityId",
                //    SecurityStampClaimType = "SecurityStamp",
                //    UserNameClaimType = "IdentityName"
                //};
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/oauth2/authorize");
            });

            // 配置session的有效时间,单位秒
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(30);
            });

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            //Identity相关文档，自定义验证
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    // 查看发现文档
                    //options.IssuerUri = Configuration["OnlinePath"].ToString();
                    options.UserInteraction = new UserInteractionOptions
                    {
                        LoginUrl = "/oauth2/authorize"//登录地址
                    };
                })

                // 自定义验证，可以不走Identity
                //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddExtensionGrantValidator<WeiXinOpenGrantValidator>()
                // 数据库模式
                .AddAspNetIdentity<ApplicationUser>()

                // this adds the config data from DB (clients, resources, CORS)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseMySql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 15; // interval in seconds. 15 seconds useful for debugging
                });

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.Requirements.Add(new ClaimRequirement("rolename", "Admin")));
                options.AddPolicy("SuperAdmin", policy => policy.Requirements.Add(new ClaimRequirement("rolename", "SuperAdmin")));
            });

            services.AddSingleton<IAuthorizationHandler, ClaimsRequirementHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSession();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();

            app.UseAuthentication();
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