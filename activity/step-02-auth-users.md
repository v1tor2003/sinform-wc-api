# Atividades do Passo 02: Autenticação & Usuários

Este guia detalha o passo a passo realizado para implementar o cadastro de usuários com senhas seguras (hashing via BCrypt) e autenticação simplificada via cabeçalho HTTP personalizado (`X-API-KEY`).

---

## 1. Instalação do BCrypt

Instalamos o pacote `BCrypt.Net-Next` para prover algoritmos robustos de salting e hashing de senhas.

```bash
dotnet add package BCrypt.Net-Next
```

---

## 2. Definição da Entidade Usuário

Criamos o modelo `User` em `api/Entities/User.cs` contendo o campo de chave de acesso API Key gerada automaticamente:

```csharp
using System;

namespace SinformWcApi.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## 3. Configuração do Banco de Dados

Atualizamos `api/AppDbContext.cs` para registrar a tabela de usuários e aplicar índices únicos de busca rápida e validação de unicidade:

```csharp
public DbSet<User> Users => Set<User>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
        entity.Property(e => e.PasswordHash).IsRequired();
        entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(100);

        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.ApiKey).IsUnique();
    });
}
```

---

## 4. Criação do Middleware de Autenticação via API Key

Criamos o middleware em `api/Middleware/ApiKeyAuthentication.cs` que lê o cabeçalho `X-API-KEY`, valida no banco de dados e cria a identidade (ClaimsPrincipal) no request HTTP. Também expõe o filtro `.RequireApiKey()` para rotas protegidas:

```csharp
// Middleware/ApiKeyAuthentication.cs
// Implementa ApiKeyAuthenticationMiddleware e ApiKeyAuthenticationExtensions
```

---

## 5. Implementação dos Endpoints (Registro e Login)

Usando a estrutura REPR (Request-Endpoint-Response), isolamos cada rota na pasta `api/Features/Auth/`:

*   **`POST /auth/register`**: Normaliza o e-mail para minúsculas, verifica se já existe, computa o hash da senha, gera um token prefixado com `usr_live_` e persiste.
*   **`POST /auth/login`**: Localiza o e-mail, compara o hash com `BCrypt.Verify` e retorna a chave `apiKey`.

---

## 6. Geração e Aplicação de Migrations

Executamos os comandos de ferramentas do EF Core para migrar a base PostgreSQL local rodando em container:

```bash
# Instalar a ferramenta ef cli global se necessário
dotnet tool install --global dotnet-ef

# Criar a migration
dotnet ef migrations add AddUsers

# Aplicar as tabelas no PostgreSQL
dotnet ef database update
```
* **Nota:** O banco é atualizado automaticamente aplicando a tabela `Users` e os índices únicos definidos.
