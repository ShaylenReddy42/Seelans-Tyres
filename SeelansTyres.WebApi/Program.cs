using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;
using SeelansTyres.WebApi.Services;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(
        options =>
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

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
