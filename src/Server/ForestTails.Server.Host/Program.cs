using CoreWCF.Configuration;
using CoreWCF.Description;
using ForestTails.Server.Data;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Logic.Config;
using ForestTails.Server.Logic.Services;
using ForestTails.Server.Logic.Utils;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information("Configuring ForestTails Server Host...");

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

Log.Information("Serilog configured.");
Log.Information("Starting SQL Server Database...");

builder.Services.AddDbContextFactory<ForestTailsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

Log.Information("Configuring SMTP Service...");

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

Log.Information("SMTP Service configured.");
Log.Information("Starting ForestTails Services...");

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IVerificationCodeRepository, VerificationCodeRepository>();
builder.Services.AddSingleton<IFriendRepository, FriendRepository>();
builder.Services.AddSingleton<ICosmeticRepository, CosmeticRepository>();
builder.Services.AddSingleton<ISanctionRepository, SanctionRepository>();
builder.Services.AddSingleton<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddSingleton<IReportRepository, ReportRepository>();

builder.Services.AddSingleton<ServiceExecutor>();
builder.Services.AddSingleton<CallbackExecutor>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<INotificationService, NotificationService>();

builder.Services.AddSingleton<IAuthValidator, AuthValidator>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

Log.Information("ForestTails Services started.");

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    var binding = new CoreWCF.NetTcpBinding();
    binding.Security.Mode = CoreWCF.SecurityMode.None;

    serviceBuilder.AddService<AuthService>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });
    serviceBuilder.AddServiceEndpoint<AuthService, IAuthService>(binding, "/AuthService");
});

Log.Information("Starting ForestTails Server Host...");

var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;

app.Run();