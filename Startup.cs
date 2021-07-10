using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Data;

namespace Shop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Possibilita o acesso aos arquivos de configuração: appsettings
        // appsettings.Development.json: usado para dev
        // appsettings.json: usado para prod

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // Compactação do json enviado para tela
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new [] { "application/json" });
            });

            // Cache da aplicação inteira
            //services.AddResponseCaching();
            services.AddControllers();

            // Configuração padrão de autenticação com Jwt
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Configuração do EF Core - InMemoryDatabase
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));

            // Configuração do EF Core - SQL Server
            //services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));

            // Injeção de dependência
            // São as dependências do projeto (precisamos resolver essas depedências aqui)
            // No .net temos 3 formas de trabalhar com as dependências, são elas: scoped, transient, singleton
            // Scoped: terei apenas 1 DataContext por requisição - guarda na memória (ao final da requisição, destroi o DataContext que destroi a conexão com o BD)
            // Transient: toda vez que for solicitado um DataContext, será criado um novo
            // Singleton: cria uma instância do DataContext por aplicação
            //services.AddScoped<DataContext, DataContext>();

            // Documentação da api
            // dotnet add package Swashbuckle.AspNetCore -v 5.0.0-rc4
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop Api", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Força a api a usar https
            app.UseHttpsRedirection();

            // Configura o endpoint (padrão)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // https://localhost:5001/swagger/index.html
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API v1");
            });

            app.UseRouting();

            // Possibilita requisições localhost
            app.UseCors(x => x.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());

            // Authentication: diz quem o usuário é
            // Authorization: são as roles (acessos do usuário)
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
