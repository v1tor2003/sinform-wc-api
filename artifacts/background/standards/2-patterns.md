## Design Patterns Utilizados na API

### A. Mediator Pattern (com MediatR)
O padrão Mediator reduz o acoplamento caótico entre objetos, fazendo com que eles se comuniquem por meio de um objeto mediador central.
- **Como usamos**: As requisições HTTP da Minimal API são transformadas em objetos C# (Commands/Queries) e passadas ao MediatR via `mediator.Send(command)`. O MediatR encaminha a requisição para o respectivo Handler de negócio.
- **Benefício**: Desacopla o ASP.NET Core (infraestrutura web) da lógica de negócios. Nossos Handlers podem ser testados unitariamente sem carregar o contexto HTTP.

```csharp
// Endpoint Minimal API
app.MapPost("/sweepstakes", async (CreateSweepstakesCommand command, IMediator mediator) => 
{
    var result = await mediator.Send(command);
    return Results.Created($"/sweepstakes/{result.Id}", result);
});
```

### B. Repository Pattern
Mediar entre as camadas de domínio e mapeamento de dados usando uma interface parecida com uma coleção de objetos de domínio na memória.
- **Como usamos**: Criamos interfaces como `ISweepstakesRepository` para abstrair operações de banco de dados do Entity Framework Core ou Dapper.
- **Benefício**: Permite mockar o banco facilmente em testes de unidade e isolar queries complexas de SQL/LINQ.

### C. Strategy Pattern
Define uma família de algoritmos, encapsula cada um deles e os torna intercambiáveis.
- **Como usamos**: Para o cálculo de pontos do Bolão. Podemos ter regras diferentes baseadas no tipo de bolão (ex: `SoccerScoringStrategy`, `BasketballScoringStrategy`).
- **Benefício**: Evita estruturas complexas de `switch` ou `if/else` no código principal.

```csharp
public interface IScoringStrategy
{
    int Calculate(Guess guess, OfficialPhaseResult result);
}
```

### D. Factory Pattern
Define uma interface para criar um objeto, mas deixa as subclasses decidirem qual classe instanciar.
- **Como usamos**: Para a geração automática de códigos de convite únicos e seguros para os bolões (`inviteCode`), ou criação de instâncias de `Guess` com estados iniciais válidos baseados na fase da Copa.

### E. Facade Pattern
Fornece uma interface unificada para um conjunto de interfaces em um subsistema.
- **Como usamos**: Para encapsular a verificação de regras complexas que envolvem múltiplos serviços (como verificar se um usuário já possui bolão ativo no banco, se a cota dele permite novos participantes, unificando DB, Cache e Auth em um validador de domínio).

### F. Singleton Pattern
Garante que uma classe tenha apenas uma instância e fornece um ponto global de acesso a ela.
- **Como usamos**: Para o gerenciador de conexões com o Redis (`IConnectionMultiplexer`) e para serviços de background/workers globais que monitoram o encerramento de bolões em tempo real.
