//using blogapp.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<BlogDBContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
//builder.Services.AddDistributedMemoryCache(); // ?
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//var app = builder.Build();

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseSession(); // ? Must be here

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Landing}/{id?}");

//app.Run();




//using blogapp.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Configure EF Core with SQL Server
//builder.Services.AddDbContext<BlogDBContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));

//// Add Identity (for secure login + role-based access)
//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<BlogDBContext>()
//    .AddDefaultTokenProviders();

//// Configure Identity cookie settings
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Admin/Login";
//    options.AccessDeniedPath = "/Admin/AccessDenied"; // Optional
//});

//// Enable Session (for other session-based features if needed)
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//var app = builder.Build();

//// ?? Seed the admin user and role after building app
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedAdmin(services);
//}

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//// Identity & Session middleware
//app.UseAuthentication(); // Required for Identity
//app.UseAuthorization();
//app.UseSession(); // Always after authentication/authorization

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Landing}/{id?}");

//app.Run();

//// ? Seed Admin User + Role (method must be below app.Run())
//async Task SeedAdmin(IServiceProvider services)
//{
//    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

//    string adminEmail = "admin@blog.com";
//    string adminUsername = "admin";
//    string adminPassword = "StrongAdmin@123"; // Use a strong password

//    // Create "Admin" role if not exists
//    if (!await roleManager.RoleExistsAsync("Admin"))
//        await roleManager.CreateAsync(new IdentityRole("Admin"));

//    // Create admin user if not exists
//    var adminUser = await userManager.FindByEmailAsync(adminEmail);
//    if (adminUser == null)
//    {
//        adminUser = new IdentityUser
//        {
//            UserName = adminUsername,
//            Email = adminEmail,
//            EmailConfirmed = true
//        };

//        var result = await userManager.CreateAsync(adminUser, adminPassword);
//        if (result.Succeeded)
//        {
//            await userManager.AddToRoleAsync(adminUser, "Admin");
//        }
//    }
//}








//using blogapp.Data;
//using blogapp.Services;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// ? Configure EF Core with SQL Server and EnableRetryOnFailure
//builder.Services.AddDbContext<BlogDBContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("MyDbConnection"),
//        sql => sql
//            .EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(5),
//                errorNumbersToAdd: null
//            )
//            .CommandTimeout(120) // reasonable timeout
//    )
//);

//// ? Add this to register the EmailService
//builder.Services.AddScoped<EmailService>();


//// ? Add Identity (for login + role)
//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<BlogDBContext>()
//    .AddDefaultTokenProviders();

//// ? Cookie settings
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Admin/Login";
//    options.AccessDeniedPath = "/Admin/AccessDenied";
//});

//// ? Session support
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//var app = builder.Build();

//// ? Seed default Admin
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedAdmin(services);
//}

//// ? Middleware setup
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthentication();    // Must be before UseAuthorization
//app.UseAuthorization();
//app.UseSession();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Landing}/{id?}");
//)

//app.Run();


//// ? Admin Seeder Function
//async Task SeedAdmin(IServiceProvider services)
//{
//    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

//    string adminEmail = "admin@blog.com";
//    string adminUsername = "admin";
//    string adminPassword = "StrongAdmin@123"; // Must meet password policy

//    // Create role if it doesn't exist
//    if (!await roleManager.RoleExistsAsync("Admin"))
//        await roleManager.CreateAsync(new IdentityRole("Admin"));

//    // Create user if it doesn't exist
//    var adminUser = await userManager.FindByEmailAsync(adminEmail);
//    if (adminUser == null)
//    {
//        adminUser = new IdentityUser
//        {
//            UserName = adminUsername,
//            Email = adminEmail,
//            EmailConfirmed = true
//        };

//        var result = await userManager.CreateAsync(adminUser, adminPassword);
//        if (result.Succeeded)
//        {
//            await userManager.AddToRoleAsync(adminUser, "Admin");
//        }
//    }
//}


using blogapp.Data;
using blogapp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------
// ?? Configure Services
// ----------------------------------

// ? MVC Controllers and Razor Views
builder.Services.AddControllersWithViews();

// ? EF Core + Retry + Timeout
builder.Services.AddDbContext<BlogDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MyDbConnection"),
        sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)
                  .CommandTimeout(120)

    ) 
);

// ? Register Email Service
builder.Services.AddScoped<EmailService>();

// ? Add Identity + Token Providers
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // ? Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;

    // ? Lockout policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<BlogDBContext>()
    .AddDefaultTokenProviders();

// ? Configure cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
});

// ? Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ----------------------------------
// ?? Seed Default Admin User + Role
// ----------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedAdminAsync(services);
}

// ----------------------------------
// ?? Middleware Pipeline
// ----------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // before authorization
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");

app.Run();

// ----------------------------------
// ?? Admin Seeder Method
// ----------------------------------
static async Task SeedAdminAsync(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@blog.com";
    string adminUsername = "admin";
    string adminPassword = "StrongAdmin@123"; // Must match the password policy

    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create Admin user if it doesn't exist
    var adminUser = await userManager.FindByNameAsync(adminUsername);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminUsername,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Log or throw if needed
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"Admin seeding failed: {error}");
        }
    }
}

