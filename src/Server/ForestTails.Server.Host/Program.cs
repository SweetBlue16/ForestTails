using CoreWCF.Configuration;
using ForestTails.Server.Data;
using ForestTails.Server.Logic.Config;
using ForestTails.Server.Logic.Utils;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddDbContextFactory<ForestTailsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// TODO: Dependency injection for data layer

builder.Services.AddSingleton<ServiceExecutor>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<INotificationService, NotificationService>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    var binding = new CoreWCF.NetTcpBinding();
    binding.Security.Mode = CoreWCF.SecurityMode.None;
});

Log.Information("Starting ForestTails Server Host...");

app.Run();