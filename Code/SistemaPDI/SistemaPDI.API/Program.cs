using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Application.Interfaces.IUtilizadorServices;
using SistemaPDI.Application.Services;
using SistemaPDI.Infrastructure.Data;
using SistemaPDI.Infrastructure.Repositories;
using SistemaPDI.Web.Services;
using System.Text;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ── DbContext ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PdiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositórios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUtilizadorRepository, UtilizadorRepository>();
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<ILoteRepository, LoteRepository>();  
builder.Services.AddScoped<ILocalizacaoRepository, LocalizacaoRepository>();
builder.Services.AddScoped<IEncomendaRepository, EncomendaRepository>();
builder.Services.AddScoped<IHistoricoPrecoRepository, HistoricoPrecoRepository>();

// ── Serviços ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUtilizadorService, UtilizadorService>();
builder.Services.AddScoped<IArtigoService, ArtigoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<ILoteService, LoteService>();  
builder.Services.AddScoped<ILocalizacaoService, LocalizacaoService>();
builder.Services.AddScoped<IEncomendaService, EncomendaService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IHistoricoPrecoService, HistoricoPrecoService>();

// ── JWT Authentication ─────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema PDI API",
        Version = "v1",
        Description = "CVP Ribeirão - Gestão de Stock"
    });

    // Configurar JWT no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira 'Bearer' [espaço] e depois o seu token JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
    
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7198")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseCors("WebApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();