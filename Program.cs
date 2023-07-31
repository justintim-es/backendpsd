using System.Text;
using backend.Email;
using backend.Models;
using backend.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration.UserSecrets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<Coschon>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton(builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
builder.Services.AddIdentity<SC, ShopRole>(options => {
    options.SignIn.RequireConfirmedEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
}).AddEntityFrameworkStores<Coschon>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddStripeInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Token:Secret")!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddCors(p => p.AddPolicy("corsapp", builder => {
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

// builder.Configuration.AddEnvironmentVariables().AddUserSecrets(Assembl)


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("corsapp");

app.Run();
