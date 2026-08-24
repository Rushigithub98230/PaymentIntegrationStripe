using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentIntegrationStripe.Application.Payments;
using PaymentIntegrationStripe.Infrastructure.Payments;
using PaymentIntegrationStripe.Infrastructure.Payments.Stripe;
using PaymentIntegrationStripe.Infrastructure.Persistence;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentIntegrationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IMoneyConverter, StripeMoneyConverter>();
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();
builder.Services.AddScoped<IPaymentRepository, EfPaymentRepository>();
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));

var stripeOptions = builder.Configuration.GetSection(StripeOptions.SectionName).Get<StripeOptions>()
    ?? throw new InvalidOperationException("Stripe configuration is required.");
if (!string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
    StripeConfiguration.ApiKey = stripeOptions.SecretKey;

var jwtSection = builder.Configuration.GetSection("Authentication:Jwt");
var signingKey = jwtSection["SigningKey"];
if (!string.IsNullOrWhiteSpace(signingKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey))
            };
        });
}

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
