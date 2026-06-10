# Atividades do Passo 01: Setup do Boilerplate

Este guia detalha o passo a passo realizado para configurar a base da API e sua infraestrutura local, permitindo que qualquer pessoa reproduza o ambiente a partir do zero.

---

## 1. Inicialização do Projeto .NET e Solução

Criamos um novo projeto ASP.NET Core Web API utilizando Minimal APIs com Top-level Statements e uma solução (.sln) no diretório raiz para agrupar nossos projetos.

```bash
# Criar um novo projeto webapi (por padrão gera Minimal API no .NET 9+) na pasta 'api'
dotnet new webapi -o api

# Criar a solução (.sln) na raiz do repositório
dotnet new sln -n sinform-wc-api -f sln

# Registrar o projeto da API na solução
dotnet sln add api/api.csproj
```
* **Motivo:** O comando gera o projeto `api.csproj` pré-configurado com as diretivas básicas, o arquivo `Program.cs` e a estrutura de diretórios para a nossa API C#, enquanto a solução nos permitirá gerenciar o projeto de produção e de testes de forma centralizada.

---

## 2. Instalação das Dependências (NuGet Packages)

Adicionamos os pacotes necessários para suportar documentação moderna, ORM (Banco de Dados) e Caching no Redis:

```bash
# Suporte ao Scalar UI para visualização bonita de documentação interativa OpenAPI
dotnet add package Scalar.AspNetCore

# Provedor do PostgreSQL para o Entity Framework Core
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Ferramentas de design do EF Core para gerenciar migrations e atualizações de schema
dotnet add package Microsoft.EntityFrameworkCore.Design

# Provedor de cache distribuído integrado com Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

---

## 3. Configuração do Banco de Dados (Entity Framework Core)

Para vincular o EF Core com a API, criamos a classe de contexto do banco:

### Criar `AppDbContext.cs`
Definimos o contexto base herdando de `DbContext`:
```csharp
using Microsoft.EntityFrameworkCore;

namespace SinformWcApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
```

---

## 4. Atualização da Inicialização (`Program.cs`)

Modificamos o `Program.cs` para remover o código de exemplo padrão (WeatherForecast) e configurar os nossos middleware de serviço:

```csharp
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SinformWcApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar o DbContext utilizando Npgsql (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrar o serviço de cache de saída HTTP
builder.Services.AddOutputCache();

// 3. Registrar o gerador de documentos OpenAPI nativo do .NET
builder.Services.AddOpenApi();

// 4. Registrar o serviço de Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// 5. Configurar pipeline HTTP de Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();              // Expõe o JSON de documentação em /openapi/v1.json
    app.MapScalarApiReference();   // Expõe a UI interativa do Scalar em /scalar/v1
}

app.UseHttpsRedirection();
app.UseOutputCache();

// 6. Criar rota simples para validar que o container da API subiu
app.MapGet("/health-check", () => Results.Ok("OK"))
   .WithName("HealthCheck");

app.Run();
```

---

## 5. Arquivos de Configuração Local (`appsettings.json`)

Adicionamos as connection strings de acesso ao banco PostgreSQL e ao Redis local:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sinform_wc;Username=postgres;Password=postgres",
    "RedisConnection": "localhost:6379"
  }
}
```

---

## 6. Infraestrutura de Desenvolvimento (`docker-compose.yml`)

Criamos o arquivo `docker-compose.yml` na pasta `api/` para subir o PostgreSQL e o Redis em contêineres Docker isolados:

```yaml
services:
  db:
    image: postgres:15-alpine
    container_name: sinform-db
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: sinform_wc
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    container_name: sinform-redis
    ports:
      - "6379:6379"

volumes:
  pgdata:
```

Para subir a infraestrutura:
```bash
docker compose up -d
```
