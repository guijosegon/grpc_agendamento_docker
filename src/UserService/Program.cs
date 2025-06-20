using Booking.User.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => {options.ListenAnyIP(5001, o => o.Protocols = HttpProtocols.Http2);});
builder.Services.AddSingleton<IUserRepository, FileUserRepository>();
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<UserServiceImpl>();
app.MapGet("/", () => "UserService gRPC rodando...");
app.Run();