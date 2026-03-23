using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.Infrastructure.Persistence;
using HomeFlow.Modules.Households.Infrastructure.DependencyInjection;
//using HomeFlow.Modules.Identity.Infrastructure.DependencyInjection;
//using HomeFlow.Modules.Subscriptions.Infrastructure.DependencyInjection;
using HomeFlow.Modules.Tenancy.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HomeFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<HomeFlowDbContext>());

builder.Services
    //.AddIdentityModule()
    .AddTenancyModule()
    //.AddSubscriptionsModule()
    .AddHouseholdsModule();

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
