# Atividades do Passo 06: Redis Output Caching

Este guia detalha o passo a passo realizado para implementar o cacheamento HTTP distribuído no endpoint de classificação geral (`Leaderboard`) utilizando o Redis Output Caching.

---

## 1. Instalação do Redis Output Caching

Instalamos o pacote oficial do ASP.NET Core para salvar o cache de páginas/respostas no Redis:

```bash
dotnet add package Microsoft.AspNetCore.OutputCaching.StackExchangeRedis
```

---

## 2. Configuração do Cache no Pipeline

Atualizamos o arquivo `api/Program.cs` para registrar o provedor Redis do Output Caching associado à string de conexão configurada:

```csharp
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});
```

---

## 3. Implementação do Endpoint de Leaderboard

Criamos o arquivo `api/Features/Sweepstakes/GetLeaderboardEndpoint.cs`:

*   **Rota:** `GET /sweepstakes/{id}/leaderboard`
*   **Regras:**
    *   Requer autenticação via `X-API-KEY`.
    *   Verifica se o bolão existe e se o usuário autenticado participa dele (retorna `403 Forbidden` caso não participe).
    *   Retorna a lista de participantes ordenados por pontuação (`TotalScore`) descendente e nome do usuário ascendente.
*   **Cache de Resposta:**
    *   Adicionamos o modificador `.CacheOutput(policy => ...)` com expiração de 5 minutos, variando por rota (`id` do bolão), e com a tag genérica `sb-leaderboard`.
