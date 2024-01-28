using Core;
using Renacci;
using Renacci.UseCases;
using RenacModbus;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
var configuration = builder.Configuration;
var appSettingsSection = configuration.GetRequiredSection("RenacModbusOptions");
var appSettings = appSettingsSection.Get<RenacModbusOptions>();
if(appSettings == null)
    throw new Exception("Failed to load RenacModbusOptions from configuration");
//
builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

Log.Logger.Information("*** Getting Renacci ready ***");
Log.Logger.Information("* Host: {Host}", appSettings.Host);
Log.Logger.Information("* Port: {Port}", appSettings.Port);
Log.Logger.Information("* DeviceId: {DeviceId}", appSettings.DeviceId);

if(appSettings == null)
    throw new Exception("Failed to load RenacModbusOptions from configuration");

var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddTransient((_) => appSettings);

services.AddTransient<RandomByteGenerator>();
services.AddTransient<TcpClientHandler>();
services.AddTransient<IStatusRepository, RenacStatusRepository>();
services.AddTransient<GetStatusUseCase>();
services.AddControllers();
    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
Log.Logger.Information("Serving for your pleasure!");
app.Run();

