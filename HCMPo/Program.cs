using HCMPo.Models;
using HCMPo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.AspNetCore.Server.IIS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure the SQL Server database context
builder.Services.AddDbContext<HCMPo.Data.ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    // Log SQL queries to the console/output
    options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<HCMPo.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.Cookie.MaxAge = TimeSpan.FromHours(1);
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Configure server options to handle request size
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50MB
});

// Configure Kestrel server options (for development)
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 32768; // 32KB (correct property name)
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
    options.Limits.MaxRequestLineSize = 8192; // 8KB
    options.Limits.MaxRequestHeaderCount = 100; // Limit number of headers
});

// Add session with size limits
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
});

// Add attendance sync services
builder.Services.AddScoped<IAttendanceSyncService, AttendanceSyncService>();
builder.Services.AddHostedService<AttendanceSyncBackgroundService>();
builder.Services.AddScoped<IZKTimeService, ZKTimeService>();
builder.Services.AddScoped<HCMPo.Services.AttendanceSummaryService>();
builder.Services.AddScoped<TaxCalculationService>();

// Add leave management services
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUnifiedEmployeeService, UnifiedEmployeeService>();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

// Add memory cache
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUserEmployeeLinkService, UserEmployeeLinkService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Initialize and seed the database in development environment
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<HCMPo.Data.ApplicationDbContext>();
            // Ensure database is created
            context.Database.EnsureCreated();
            
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Initialize the database
            await DbInitializer.Initialize((HCMPo.Data.ApplicationDbContext)context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

app.UseHttpsRedirection();

// Configure static files with explicit content types for fonts
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".woff"] = "application/font-woff";
provider.Mappings[".woff2"] = "application/font-woff2";
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.WebRootPath)),
    RequestPath = "",
    ContentTypeProvider = provider
});

Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");
app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hub for real-time notifications
app.MapHub<NotificationHub>("/notificationHub");
app.MapRazorPages();

app.Run();
