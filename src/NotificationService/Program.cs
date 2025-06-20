using Booking.Notify.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => {options.ListenAnyIP(5003, o => o.Protocols = HttpProtocols.Http2);});
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<NotificationServiceImpl>();
app.MapGet("/", () => "NotificationService gRPC rodando...");
app.Run();