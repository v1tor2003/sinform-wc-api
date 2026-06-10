# Atividades do Passo 09: Refatoração com Middlewares e Validação Nativa

Este guia detalha o passo a passo realizado para desacoplar a lógica transversal (validação de payloads, obtenção do usuário logado e verificação de idempotência) utilizando middlewares, filtros e validação nativa do .NET 10.

---

## 1. Instalação do Pacote de Validação Nativa

No .NET 10, a validação de minimal APIs foi integrada nativamente na stack do ASP.NET Core usando o pacote oficial:

```bash
dotnet add api/api.csproj package Microsoft.Extensions.Validation
```

---

## 2. Contexto de Usuário Desacoplado (`IUserContext`)

Para evitar ler diretamente o `ClaimsPrincipal` ou `HttpContext` nos endpoints e regras de negócio:

1.  Criamos a interface `api/Contexts/IUserContext.cs`:
    *   `UserId` (Guid?)
    *   `Name` (string?)
    *   `Email` (string?)
    *   `IsAuthenticated` (bool)
2.  Implementamos em `api/Contexts/UserContext.cs`.
3.  Criamos o middleware `api/Middleware/AttachUserContextMiddleware.cs` que resolve o `UserContext` escopado e injeta as informações a partir do usuário autenticado no pipeline do ASP.NET Core.

---

## 3. Middleware Genérico de Idempotência (`IdempotencyActionMiddleware`)

Substituímos o fluxo de idempotência inline no endpoint por um middleware reutilizável:

1.  Criamos o atributo marcador `api/Attributes/IdempotentAttribute.cs`.
2.  Implementamos o middleware `api/Middleware/IdempotencyActionMiddleware.cs`:
    *   Verifica se o endpoint atual possui o metadado `IdempotentAttribute`.
    *   Caso possua, exige o cabeçalho `Idempotency-Key`.
    *   Faz o lookup no `IDistributedCache`. Em caso de hit, retorna `200 OK` com o payload retornado da cache.
    *   Em caso de miss, captura o buffer de escrita do pipeline HTTP e salva na cache em caso de retorno `200 OK` ou `201 Created`.

---

## 4. Validação por Data Annotations e Custom Attributes

Removemos as validações manuais com throw de `DomainException` nos endpoints:

1.  Criamos o atributo customizado `api/Validation/FutureDateAttribute.cs` para validar se datas informadas estão no futuro.
2.  Atualizamos os records de `Request` com anotações de validação:
    *   `[Required]`, `[StringLength]`, `[Range]` e `[FutureDate]`.
3.  Ativamos o comportamento nativo no `Program.cs`:
    ```csharp
    builder.Services.AddValidation();
    ```

---

## 5. Configuração da Pipeline no `Program.cs`

A ordem correta do pipeline de requisições foi configurada:

```csharp
// Autentica via ApiKey
app.UseApiKeyAuthentication();

// Popula IUserContext
app.UseMiddleware<AttachUserContextMiddleware>();

// Intercepta requisições idempotentes marcadas
app.UseMiddleware<IdempotencyActionMiddleware>();
```

---

## 6. Desativação da Paralelização de Testes no xUnit

Como as conexões do SQLite em memória concorrem e podem gerar erros de estado compartilhado no driver do SQLite, criamos o arquivo `tests/xunit.runner.json` para desativar a paralelização e configuramos no `tests.csproj` para copiá-lo para a pasta de build:

```json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "parallelizeAssembly": false,
  "parallelizeTestCollections": false
}
```

---

## 7. Executando os Testes

Para executar toda a suíte de testes de unidade, integração e validação:

```bash
dotnet test
```
