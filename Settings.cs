namespace Shop
{
    public static class Settings
    {
        // Packages
        // dotnet add package Microsoft.AspNetCore.Authentication
        // dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

        // Chave privada (Secret) : usado para fazer decrypt de uma parte do token
        // Poderia ter deixado no appsettings, mas é mais seguro em uma classe static
        public static string Secret = "5e262370daae44f889ea051a5d32eab1";
    }
}