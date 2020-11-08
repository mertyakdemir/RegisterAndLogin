using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegisterAndLogin.UI.EMailServices;
using RegisterAndLogin.UI.Models;

namespace RegisterAndLogin.UI
{
    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<UserAppContext>(options => options.UseSqlServer
            ("Server=DESKTOP-KDQ2BAE\\SQLEXPRESS;Database=UserDb; Integrated Security=SSPI; MultipleActiveResultSets=True;"));

            services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<UserAppContext>().AddDefaultTokenProviders().AddDefaultUI();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/acccount/logout";
                options.AccessDeniedPath = "/account/accessdenied";
            });

            services.AddControllersWithViews();

            services.AddScoped<IEmailSmtp, GmailSmtp>(i => new GmailSmtp(
                 _configuration["EmailStmp:Host"],
                 _configuration.GetValue<int>("EmailStmp:Port"),
                 _configuration.GetValue<bool>("EmailStmp:EnableSSL"),
                 _configuration["EmailStmp:Email"],
                 _configuration["EmailStmp:Password"]
                 ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            SeedIdentity.Seed(userManager, roleManager, configuration).Wait();
        }
    }
}
