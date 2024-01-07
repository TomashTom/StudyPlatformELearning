using Microsoft.EntityFrameworkCore;
using StudyPlatformELearningHub.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using StudyPlatformELearningHub.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using StudyPlatformELearningHub.Service;
using StudyPlatformELearningHub.IService;
using Microsoft.AspNetCore.Authorization;
using StudyPlatformELearningHub.RequiredConfirm;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);




        builder.Services.AddControllersWithViews();

        builder.Services.AddRazorPages().AddSessionStateTempDataProvider();
        builder.Services.AddSession();
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DataContextConnection")));


        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = long.MaxValue; // Set the maximum request body size to the largest possible value
        });

        // If using IIS:
        builder.Services.Configure<IISServerOptions>(options =>
        {
            options.MaxRequestBodySize = long.MaxValue; // Set the maximum request body size to the largest possible value
        });

        // Configure FormOptions for the multipart body length limit
        builder.Services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = long.MaxValue; // In here we're setting the limit to the maximum value
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });







        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddScoped<IEmailService, EmailService>();


        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue;
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("ConfirmedTeacher", policy =>
                policy.Requirements.Add(new RequireConfirmedRoleRequirement("Teacher")));
        });

      


        builder.Services.AddScoped<IAuthorizationHandler, RequireConfirmedRoleHandler>();
        


        var app = builder.Build();


        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }


        app.UseSession();
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

       

        app.MapRazorPages();


        app.UseEndpoints(endpoints =>
        {

            //////////////////////////////////////////////////////
            endpoints.MapAreaControllerRoute(
                name: "user_area_contactus",
                areaName: "User",
                pattern: "User/ContactUs",
                defaults: new { controller = "UserContactUs", action = "Index" });

            endpoints.MapAreaControllerRoute(
                 name: "user_area_userprofile",
                 areaName: "User",
                 pattern: "User/UserProfile/{action=Index}/{id?}",
                defaults: new { controller = "UserProfiles", action = "Index" });

            endpoints.MapAreaControllerRoute(
                 name: "user_area_seelater",
                 areaName: "User",
                 pattern: "User/AddToSeeLater/{action=Index}/{id?}",
                defaults: new { controller = "SeeLater", action = "Index" });

           





            endpoints.MapAreaControllerRoute(
                name: "user_video_area",
                areaName: "User",
                pattern: "User/{action=Index}/{id?}",
                defaults: new { controller = "Video" });

            // Default route for non-area requests
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
                name: "reset_password",
                pattern: "UserProfiles/ResetPassword/{code?}", // Ensure this pattern is correct
                defaults: new { controller = "UserProfiles", action = "ResetPassword" });

            endpoints.MapControllerRoute(
                name: "ChangeVideoStatus",
                pattern: "Upload/ChangeVideoStatus/{id}/{status}",
                defaults: new { controller = "Upload", action = "ChangeVideoStatus" }
);



        });
       




        app.Run();
    }
}