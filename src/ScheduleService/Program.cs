using booking.schedule.Services.Repositories;
using Booking.Schedule.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using Booking.Notify;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => {options.ListenAnyIP(5002, o => o.Protocols = HttpProtocols.Http2);});
builder.Services.AddGrpcClient<NotificationService.NotificationServiceClient>(options => { options.Address = new Uri("http://notification-service:5003"); });
builder.Services.AddSingleton<IScheduleRepository, FileScheduleRepository>();
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<ScheduleServiceImpl>();
app.MapGet("/", () => "ScheduleService gRPC rodando...");
app.Run();