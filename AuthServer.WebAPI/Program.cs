using AuthServer.Application.Interfaces.Services;
using AuthServer.Domain.Entities;
using AuthServer.Persistence;
using AuthServer.Infrastructure;
using AuthServer.Persistence.Contexts;
using AuthServer.WebAPI.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthServer.WebAPI.Middlewares;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers(options =>
{
    options.Filters.Add<RolePermissionFilter>();
    options.Filters.Add<TransactionFilter>();
});
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

#region Add Layers 
builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);
#endregion

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



#region Add Identity
// TODO : Add Identity with json 
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();
#endregion

#region Add Authentication
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<AuthServer.Application.Options.TokenOptions>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
    };
});
#endregion

#region Add Authorization
//var authorizationService = builder.Services.BuildServiceProvider().GetService<IAuthorizationService>();
//var roleService = builder.Services.BuildServiceProvider().GetService<IRoleService>();

//var endpoints = authorizationService.GetAuthorizeDefinitionEndpoints(typeof(Program));
//await roleService.SaveEndpointsAsync(endpoints);
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
