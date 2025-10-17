using Microsoft.EntityFrameworkCore;
using Publink.Api.Config;
using Publink.Core.CQRS.Queries;
using Publink.Data.Context;
using Publink.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Publink Logs API",
        Version = "v1"
    });
    // Use fully qualified names for schema IDs to avoid collisions for nested types like "Response"
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsConfig.DevCors, policy =>
        policy.AllowAnyOrigin() // Todo: Restrict to specific origins in future
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("PublinkConnection");
builder.Services.AddDbContext<PublinkDbContext>(options =>
    options.UseNpgsql(connectionString));

// Scan Publink.Core for MediatR handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetLogsForOrganisationQuery.Request>());

// Repositories
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsConfig.DevCors);

app.UseAuthorization();

app.MapControllers();

app.Run();