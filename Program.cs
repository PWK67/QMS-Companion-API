using QMS.Companion.Api.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QMS Companion API",
        Version = "v1",
        Description = """
        QMS Companion API is an open-source developer API
        for accessing QMS blockchain data.

        Features:
        - Network information
        - Latest block data
        - Wallet balance lookup
        - Transaction lookup
        - Transaction receipt lookup

        Built with ASP.NET Core 8 and C#.
        """,
        Contact = new OpenApiContact
        {
            Name = "QMS Companion API"
        }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});


builder.Services.AddHttpClient<QmsRpcService>(client =>
{
    client.BaseAddress = new Uri("https://rpc.testnet.qms.finance");
    client.Timeout = TimeSpan.FromSeconds(15);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
