using HomeFlow.BuildingBlocks.Application.Abstractions;
using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.BuildingBlocks.Infrastructure.Services;
using HomeFlow.Modules.Households.Infrastructure.DependencyInjection;
using HomeFlow.Modules.Scheduling.Infrastructure.DependencyInjection;
//using HomeFlow.Modules.Identity.Infrastructure.DependencyInjection;
//using HomeFlow.Modules.Subscriptions.Infrastructure.DependencyInjection;
using HomeFlow.Modules.Tenancy.Infrastructure.DependencyInjection;
using HomeFlow.Modules.Integrations.Gmail.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "FrontendCors";

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:2745",   // Angular dev
                "https://localhost:2745"   // Angular HTTPS 
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HomeFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<HomeFlowDbContext>());

builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

builder.Services.AddDataProtection();

builder.Services
    //.AddIdentityModule()
    .AddTenancyModule()
    //.AddSubscriptionsModule()
    .AddHouseholdsModule()
    .AddSchedulingModule()
    .AddGmailModule(builder.Configuration);

var app = builder.Build();

app.UseCors(CorsPolicyName);

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
