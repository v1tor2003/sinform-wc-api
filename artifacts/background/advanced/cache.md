## Caching: Output Caching vs. Cache-Aside

Para otimizar consultas pesadas (como o Leaderboard do bolão), podemos usar cache. Existem duas abordagens principais de implementação:

### Padrão Cache-Aside (Sob Demanda)
Nesse padrão tradicional, a própria lógica do endpoint/repositório gerencia o cache explicitamente:
1. A aplicação tenta obter os dados do cache (ex: Redis).
2. Se houver um **Cache Hit** (dados encontrados), retorna-os imediatamente.
3. Se houver um **Cache Miss** (dados não encontrados), a aplicação busca no PostgreSQL, salva no Redis para futuras consultas e, então, retorna os dados.

### ASP.NET Core Output Caching
Introduzido no .NET 7, o **Output Caching** é um middleware nativo que intercepta a requisição HTTP inteira no nível do servidor ASP.NET Core:
1. Quando uma chamada chega a um endpoint configurado (ex: `app.MapGet("/sweepstakes/{id}/leaderboard").CacheOutput()`), o middleware verifica se já possui a resposta HTTP bruta salva.
2. Em caso de acerto, o middleware retorna diretamente o JSON e os cabeçalhos HTTP cacheados, **sem sequer acionar o Route Handler** (a lógica do endpoint não é executada).
3. Podemos invalidar o cache de forma limpa usando políticas baseadas em tags (ex: invalidar a tag `leaderboard-sweepstakes-123` quando um novo palpite é computado).

### Comparativo de Fluxos

```mermaid
graph TD
    subgraph Cache-Aside (Código da Aplicação)
        ClientA[Cliente] -->|1. Request /leaderboard| AppA[Código da Aplicação]
        AppA -->|2. Check Key| CacheA[(Cache Store / Redis)]
        CacheA -.->|3a. Cache Hit| AppA
        AppA -.->|3b. Cache Miss| DBA[(PostgreSQL)]
        DBA -.->|4. Retorna Dados| AppA
        AppA -->|5. Salva no Cache| CacheA
        AppA -->|6. Retorna DTO JSON| ClientA
    end

    subgraph Output Caching (Middleware ASP.NET Core)
        ClientB[Cliente] -->|1. Request /leaderboard| Middleware[OutputCache Middleware]
        Middleware -->|2. Check Route Cache| StoreB[(Output Cache Store)]
        StoreB -.->|3a. Hit: Retorna resposta HTTP crua| ClientB
        StoreB -.->|3b. Miss| Middleware
        Middleware -->|4. Executa Endpoint| EndB[Route Handler]
        EndB -->|5. Query Banco| DBB[(PostgreSQL)]
        DBB -->|6. Retorna Dados| EndB
        EndB -->|7. Retorna HTTP Response| Middleware
        Middleware -->|8. Salva HTTP Response| StoreB
        Middleware -->|9. Envia HTTP Response| ClientB
    end
```