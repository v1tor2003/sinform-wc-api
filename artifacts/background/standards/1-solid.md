## Princípios SOLID (Contexto .NET)

Os princípios SOLID garantem que o software seja fácil de manter, estender e testar ao longo do tempo.

### Single Responsibility Principle (Princípio da Responsabilidade Única - SRP)
*Uma classe deve ter apenas um motivo para mudar.*
- **Na API**: Em vez de concentrar toda a lógica de um bolão em um único `SweepstakesService` com centenas de linhas, separamos em comandos e manipuladores específicos (ex: `CreateSweepstakesCommandHandler`, `PlaceGuessCommandHandler`). Cada endpoint REPR tem uma única função.

### Open/Closed Principle (Princípio do Aberto/Fechado - OCP)
*Entidades de software devem estar abertas para extensão, mas fechadas para modificação.*
- **Na API**: Se quisermos adicionar novas regras de pontuação (ex: acerto exato da posição dá 10 pontos, bônus para gabaritar a fase, etc.), podemos implementar uma interface `IScoringStrategy`. Novas regras são adicionadas criando novas classes que implementam essa interface, sem alterar o código que processa a pontuação geral.

### Liskov Substitution Principle (Princípio da Substituição de Liskov - LSP)
*Subclasses devem ser substituíveis por suas superclasses sem alterar a corretude do programa.*
- **Na API**: Qualquer repositório que implemente `ISweepstakesRepository` deve se comportar da mesma forma sob o ponto de vista da aplicação, seja ele implementado em memória (para testes rápidos) ou no PostgreSQL (via EF Core).

### Interface Segregation Principle (Princípio da Segregação de Interfaces - ISP)
*Clientes não devem ser forçados a depender de métodos que não utilizam.*
- **Na API**: Em vez de criar uma interface gigante `IDataService` com todos os métodos do banco, criamos interfaces focadas: `IUserRepository` (somente métodos de usuário) e `ISweepstakesRepository` (somente métodos do bolão).

### Dependency Inversion Principle (Princípio da Inversão de Dependência - DIP)
*Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações.*
- **Na API**: Nossos Handlers de negócio dependem de interfaces (`ISweepstakesRepository`), não de implementações concretas (`EfSweepstakesRepository`). O container de DI do .NET injeta as implementações em tempo de execução.
