using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;
using SeelansTyres.WebApi.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler =
                ReferenceHandler.IgnoreCycles;
        })
    .AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SeelansTyresContext>(
    options => options.UseSqlServer(
        builder.Configuration["ConnectionStrings:SeelansTyresContext"]));

builder.Services.AddScoped<ISeelansTyresRepository, SeelansTyresRepository>();

builder.Services.AddIdentity<Customer, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SeelansTyresContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.TokenValidationParameters = new()
        {
            ValidIssuer = builder.Configuration["Token:Issuer"],
            ValidAudience = builder.Configuration["Token:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Token:Key"])),
        };
    });

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustBeARegularCustomer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(handler => handler.User.IsInRole("Administrator") is false);
    });
});

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.Urls.Clear();
    app.Urls.Add("http://localhost:4300");
    app.Urls.Add("https://localhost:4301");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
