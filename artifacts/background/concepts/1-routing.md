## Minimal APIs vs. MVC

### MVC (Abordagem Tradicional)
Historicamente, o ASP.NET Core utilizava o padrão **Model-View-Controller (MVC)** para estruturar APIs de forma opinativa:
- **Controllers**: Classes que herdam de `ControllerBase`, agrupando diversos métodos (Actions) para lidar com recursos semelhantes (ex: `UsersController` gerencia criação, listagem, atualização e deleção).
- **Roteamento**: Baseado em atributos (`[Route("api/[controller]")]`) ou rotas convencionadas.
- **Overhead**: Requer reflection pesado na inicialização da aplicação para descobrir controllers, carregar filtros globais e montar a árvore de rotas.

### Minimal APIs
Introduzidas no .NET 6, as **Minimal APIs** fornecem uma sintaxe simplificada e de altíssima performance para criar APIs HTTP com o mínimo de código e cerimônia:
- **Route Handlers**: Mapeamento direto de rotas HTTP usando lambdas ou métodos estáticos (ex: `app.MapPost("/users", ...)`).
- **Performance**: Menor alocação de memória e tempo de inicialização (startup) drasticamente reduzido.
- **Flexibilidade**: Permite organizar a API sem a rigidez de herdar de classes base ou seguir estruturas de pastas fixas.