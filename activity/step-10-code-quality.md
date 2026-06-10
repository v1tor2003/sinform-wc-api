# Atividades do Passo 10: Qualidade de Código (Padrões de Projeto e Serviços)

Este guia detalha o passo a passo realizado para refatorar a camada de controle/endpoint e persistência de dados. Introduzimos os padrões de Repositório (Repository), Fábrica (Factory), e uma camada de Serviços (Service Layer) coesa, além de agrupar todas as rotas da API sob um prefixo versionado `/api/v1`.

---

## 1. Padrão de Repositório (Repository Pattern)

Para isolar o acesso ao banco de dados (`AppDbContext`) e simplificar a manutenção do esquema relacional, criamos as interfaces e implementações de repositório:

*   `IUserRepository` & `UserRepository`
*   `ISweepstakesRepository` & `SweepstakesRepository`
*   `IParticipantRepository` & `ParticipantRepository`
*   `IGuessRepository` & `GuessRepository`
*   `IOfficialResultRepository` & `OfficialResultRepository`

Cada repositório encapsula as consultas LINQ e manipulações de dados, centralizando-as e expondo apenas métodos limpos.

---

## 2. Fábrica de Entidades (Factory Pattern)

Para encapsular a criação da entidade complexa `Guess`, criamos o `GuessFactory`. Ele encapsula as regras de inicialização de palpites (como inicializar ou omitir o terceiro lugar com base na configuração do bolão).

---

## 3. Camada de Serviços Coesos (Service Layer)

Em vez de criar uma classe separada para cada caso de uso simples, optamos por construir classes de serviços coesas agrupando operações de domínio correlacionadas:

*   **`AuthService`**: Gerencia o registro (`RegisterAsync`) e login (`LoginAsync`) de usuários.
*   **`SweepstakesService`**: Gerencia a criação (`CreateAsync`), entrada de participantes (`JoinAsync`) e obtenção do placar de líderes (`GetLeaderboardAsync`).
*   **`GuessService`**: Encapsula as regras de validação de palpites e cria/atualiza os registros (`CreateOrUpdateAsync`).
*   **`OfficialResultService`**: Homologa os resultados oficiais de fases específicas (`CreateOrUpdateAsync`).

---

## 4. Agrupamento de Rotas (`/api/v1`)

Centralizamos o mapeamento de endpoints em `Program.cs` sob um grupo de rotas versionadas:

```csharp
var apiV1 = app.MapGroup("api/v1");
apiV1.MapRegisterEndpoint();
apiV1.MapLoginEndpoint();
apiV1.MapCreateSweepstakesEndpoint();
apiV1.MapJoinSweepstakesEndpoint();
apiV1.MapCreateOrUpdateGuessEndpoint();
apiV1.MapGetLeaderboardEndpoint();
apiV1.MapCreateOfficialResultEndpoint();
```

Os caminhos relativos dos endpoints e os testes foram atualizados automaticamente para bater em `/api/v1/...`.

---

## 5. Exceções Customizadas e Tratamento de Erros

Para garantir que a camada de serviços possa sinalizar erros de HTTP de forma idiomática sem usar retornos acoplados do ASP.NET Core:

*   Criamos `NotFoundException` para erros 404 (ex: bolão não encontrado).
*   Criamos `ForbiddenException` para erros 403 (ex: usuário não participa do bolão).
*   Atualizamos o `ExceptionHandlingMiddleware` para mapear essas exceções para os status HTTP corretos.

---

## 6. Executando os Testes

Para garantir que toda a arquitetura de banco de dados, injeção de dependência e mapeamento de rotas continue funcionando perfeitamente:

```bash
dotnet test
```
