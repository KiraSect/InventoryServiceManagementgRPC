using InventoryGrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<InventoryManagementService>();

app.MapGet("/", () => "Inventory gRPC Service is running.");

app.Run();
