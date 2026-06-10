# Atividades do Passo 05: Apostas & Idempotência

Este guia detalha o passo a passo realizado para implementar os palpites dos usuários (`Guesses`), validação de prazos e unicidade de seleções, e o mecanismo de idempotência via cabeçalho `Idempotency-Key` integrado ao cache distribuído com Redis.

---

## 1. Configuração do Redis e Cache Distribuído

Configuramos o provedor de cache distribuído baseado em Redis no arquivo `api/Program.cs`:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});
```

---

## 2. Criação da Entidade `Guess`

Adicionamos o modelo em `api/Entities/Guess.cs` contendo a ordem final de classificação e o relacionamento de 1 para 1 com o participante (`Participant`):

```csharp
using System;

namespace SinformWcApi.Entities;

public class Guess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public string First { get; set; } = string.Empty;
    public string Second { get; set; } = string.Empty;
    public string Third { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## 3. Configuração do DbContext

Mapeamos a tabela `Guesses` em `api/AppDbContext.cs` adicionando o `DbSet` e um índice único no `ParticipantId` para reforçar a regra de apenas 1 palpite por participante.

---

## 4. Endpoint de Palpite (`POST /guesses`)

Implementado em `api/Features/Guesses/CreateOrUpdateGuessEndpoint.cs`:

1.  **Idempotência:**
    *   Exige o cabeçalho `Idempotency-Key`. Se ausente, lança `DomainException`.
    *   Checa no Redis se a chave `idemp:{chave}` existe. Caso sim, desserializa a resposta guardada e a devolve imediatamente com `200 OK`.
2.  **Validações de Domínio:**
    *   Verifica se o usuário é participante do bolão (`403 Forbidden`).
    *   Verifica se o prazo (`guessesDeadline`) já expirou (`DomainException`).
    *   Verifica se a seleção de países em 1º, 2º (e 3º se ativo no bolão) possui duplicados (`DomainException`).
3.  **Upsert:**
    *   Se o usuário já possui um palpite, atualiza-o. Caso contrário, insere um novo registro.
4.  **Persistência do Cache:**
    *   Salva a resposta resultante no Redis (`IDistributedCache`) com expiração absoluta de 24 horas.

---

## 5. Aplicação das Migrations

Criamos e aplicamos a nova migration no banco local rodando no Docker:

```bash
# Gerar a migration
dotnet ef migrations add AddGuesses

# Aplicar tabelas no PostgreSQL
dotnet ef database update
```
