using CoreWCF.Configuration;
using ForestTails.Server.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddDbContextFactory<ForestTailsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    var binding = new CoreWCF.NetTcpBinding();
    binding.Security.Mode = CoreWCF.SecurityMode.None;
});

app.Run();