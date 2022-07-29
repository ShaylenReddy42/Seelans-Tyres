using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.Mvc.Data;
using SeelansTyres.Mvc.Data.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SeelansTyresContext>(
    options => options.UseSqlServer(
        builder.Configuration["ConnectionStrings:SeelansTyresContext"]));

builder.Services.AddIdentity<Customer, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SeelansTyresContext>()
    .AddRoles<IdentityRole<Guid>>();

// i'm not all that strict with this
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 0;

    options.User.RequireUniqueEmail = true;
});

builder.Services.AddScoped<AdminAccountSeeder>();

builder.Services.AddHttpClient("SeelansTyresAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7012/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await RunAdminAccountSeeder();

app.Run();

async Task RunAdminAccountSeeder()
{
    using (var scope = app.Services.CreateScope())
    {
        var adminAccountSeeder = scope.ServiceProvider.GetService<AdminAccountSeeder>();
        await adminAccountSeeder!.CreateAdminAsync();
    }
}
