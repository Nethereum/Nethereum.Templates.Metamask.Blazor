using ExampleProjectSiwe.RestApi.Authorisation;
using Nethereum.Siwe;
using Nethereum.Siwe.UserServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddControllers();

services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var inMemorySessionNonceStorage = new InMemorySessionNonceStorage();
services.AddScoped<ISessionStorage>(x => inMemorySessionNonceStorage);

//we don't need a ethereumUserService (db or contract), ignore address 1271 validation web3
services.AddScoped(x => new SiweMessageService(inMemorySessionNonceStorage, null, null));
services.AddScoped<ISiweJwtAuthorisationService, SiweJwtAuthorisationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseAuthorization();
app.UseCors(configure =>
{
    configure.WithOrigins("https://localhost:44337", "https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.MapControllers();

app.UseMiddleware<SiweJwtMiddleware>();
app.Run();
