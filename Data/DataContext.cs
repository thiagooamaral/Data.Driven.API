using Microsoft.EntityFrameworkCore;
using Shop.Models;

namespace Shop.Data
{
    public class DataContext : DbContext
    {
        // Instalação do EF Core + InMemory
        // dotnet add package Microsoft.EntityFrameworkCore.InMemory
        // dotnet add package Microsoft.EntityFrameworkCore.SqlServer
        // dotnet tool install --global dotnet-ef : possibilita o uso dos comandos do EF (como migration)
        // dotnet add package Microsoft.EntityFrameworkCore.Design

        // dotnet ef migrations add InitialCreate
        // dotnet ef database update

        // DbContext: representação do bd em memória
        // DbSet: representação das tabelas em memória

        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}