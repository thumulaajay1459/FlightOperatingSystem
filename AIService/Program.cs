using System.Text;
using AIService.Data;
using AIService.RAG;
using AIService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

// Add HttpContextAccessor for token forwarding
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<AiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP clients for other services
builder.Services.AddHttpClient("AzureAI");

builder.Services.AddHttpClient<FlightServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:FlightService"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient<BookingServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:BookingService"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient<SeatServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:SeatService"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// RAG services — singleton so knowledge base is loaded once
builder.Services.AddSingleton<KnowledgeBase>();
builder.Services.AddSingleton<ResponseGenerator>();

// Chat service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ChatService>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AiDbContext>().Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
