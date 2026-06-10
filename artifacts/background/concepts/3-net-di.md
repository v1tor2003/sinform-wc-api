## Injeção de Dependência (DI) e Ciclo de Vida de Serviços

O .NET possui um container de **Inversão de Controle (IoC)** nativo altamente robusto que gerencia a criação e destruição de dependências.

### Ciclos de Vida (Service Lifetimes)

Para registrar um serviço no container, precisamos definir seu tempo de vida:

| Lifetime | Descrição | Caso de Uso Comum |
| :--- | :--- | :--- |
| **Transient** | Uma nova instância é criada **toda vez** que o serviço é solicitado. | Validadores leves, formatadores, serviços utilitários sem estado. |
| **Scoped** | Uma única instância é criada **uma vez por requisição HTTP** (escopo). | Contexto de Banco de Dados (`DbContext`), Repositórios, Unidade de Trabalho (Unit of Work). |
| **Singleton** | Uma única instância é criada na inicialização e **compartilhada por toda a aplicação** até que ela pare. | Clientes de conexão (Redis, RabbitMQ), caches em memória compartilhados, serviços de log globais. |

### Exemplo Prático de Registro

```csharp
var builder = WebApplication.CreateBuilder(args);

// Registro de dependências com diferentes ciclos de vida
builder.Services.AddTransient<IValidator<CreateSweepstakesRequest>, CreateSweepstakesValidator>();
builder.Services.AddScoped<ISweepstakesRepository, SweepstakesRepository>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect("localhost:6379"));

var app = builder.Build();
```

> [!WARNING]
> **Captive Dependency (Dependência Cativa):**
> Nunca injete um serviço com tempo de vida **Scoped** (como o `DbContext`) dentro de um serviço registrado como **Singleton**. Como o Singleton vive para sempre, a instância do Scoped também viverá para sempre associada a ele, causando vazamento de conexões de banco e problemas de concorrência multi-thread.
