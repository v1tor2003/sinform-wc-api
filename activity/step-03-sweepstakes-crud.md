# Atividades do Passo 03: Criação de Bolões & Erros Globais

Este guia detalha o passo a passo realizado para implementar o middleware de erros globais (com respostas padronizadas em JSON com timestamp ISO) e a criação de bolões com a restrição de no máximo um bolão ativo por usuário.

---

## 1. Tratamento Global de Erros

Criamos uma exceção de domínio personalizada e o middleware para interceptação de erros:

### Exceção de Domínio (`Exceptions/DomainException.cs`)
```csharp
using System;

namespace SinformWcApi.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
```

### Middleware (`Middleware/ExceptionHandlingMiddleware.cs`)
Adicionamos o middleware `ExceptionHandlingMiddleware` que intercepta requisições HTTP e trata `DomainException` retornando `400 Bad Request` com resposta padronizada ISO 8601, e outras exceções como `500 Internal Server Error`.

---

## 2. Entidades de Domínio (`Sweepstakes` e `Participant`)

Criamos os modelos sob a pasta `api/Entities/`:

*   **`Sweepstakes.cs`**: Representa o bolão. Salva configurações (prazo de guesses, quantidade classificados, etc.) e o `InviteCode` único.
*   **`Participant.cs`**: Tabela associativa relacionando usuários a bolões com score acumulado.

---

## 3. Configuração do DbContext

Atualizamos `api/AppDbContext.cs` adicionando os `DbSet` correspondentes e configurando chaves compostas e índices únicos:

*   Índice único em `InviteCode` para buscas rápidas.
*   Índice único composto em `(SweepstakesId, UserId)` para impedir a duplicidade de um participante no mesmo bolão.

---

## 4. Endpoint de Criação (`POST /sweepstakes`)

Isolamos o código em `api/Features/Sweepstakes/CreateSweepstakesEndpoint.cs`:

*   Protegido pelo cabeçalho `X-API-KEY`.
*   Valida a regra de negócio: Se o usuário criador já possuir qualquer outro bolão onde `IsActive == true`, lança uma `DomainException` (capturada pelo middleware que retorna `400 Bad Request`).
*   Gera um código de convite de 6 caracteres alfanuméricos em caixa alta.
*   Registra automaticamente o criador do bolão como o primeiro participante com score `0`.

---

## 5. Aplicação das Migrations

Geramos e aplicamos o schema no banco rodando no Docker:

```bash
# Criar a migration
dotnet ef migrations add AddSweepstakesAndParticipants

# Aplicar tabelas no PostgreSQL
dotnet ef database update
```
*   **Tabelas criadas:** `Sweepstakes`, `Participants`.
