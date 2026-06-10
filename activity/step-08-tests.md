# Atividades do Passo 08: Testes Automatizados

Este guia detalha o passo a passo realizado para implementar a suíte completa de testes automatizados do bolão da Copa do Mundo, dividida entre testes de unidade (Domain Rules) e testes de integração de ponta a ponta (e2e).

---

## 1. Configuração do Projeto de Testes

Criamos e vinculamos o projeto de testes `tests` ao arquivo de solução do projeto:

```bash
# Inicializar o projeto xUnit na pasta /tests
dotnet new xunit -o tests

# Adicionar o projeto ao arquivo de solução da raiz
dotnet sln add tests/tests.csproj

# Adicionar a referência do projeto api
dotnet add tests/tests.csproj reference api/api.csproj
```

Instalamos os seguintes pacotes no projeto `tests`:
*   `Moq` (para mockar serviços como o `IOutputCacheStore`)
*   `Microsoft.AspNetCore.Mvc.Testing` (para subir o host em memória para testes de integração)
*   `Microsoft.EntityFrameworkCore.Sqlite` (para banco de dados em memória SQLite nos testes)

---

## 2. Testes de Unidade (Suite A & Suite B)

Implementamos testes puramente focados na lógica e regras de domínio sem tocar em persistência ou rede:

### Suite A: Validador de Pontuação (`IScoringStrategy`)
Mapeado em `tests/ScoringStrategyTests.cs`:
*   **UT-01 (Sucesso):** Acerto exato do 1º e 2º lugar (+20 pontos).
*   **UT-02 (Zero pontos):** Times corretos mas posições invertidas (0 pontos).
*   **UT-03 (Parcial):** Acerto apenas do 1º lugar (+10 pontos).
*   **UT-04 (Zero pontos):** Erro completo na classificação geral.
*   **UT-05 (Parcial):** Bolão configurado com 3º lugar, acertou apenas o 1º (+10 pontos).

### Suite B: Limite de Bolão Ativo (`IActiveSweepstakesRule`)
Mapeado em `tests/ActiveSweepstakesRuleTests.cs`:
*   **UT-06 (Sucesso):** Criação permitida se contagem de bolões ativos for 0.
*   **UT-07 (Erro):** Lança `DomainException` se usuário já possuir bolão ativo (`>= 1`).

---

## 3. Testes de Integração (Suite C, D, E & F)

Criamos uma base comum `tests/IntegrationTestBase.cs` que inicializa o `WebApplicationFactory<Program>`, substitui o `DbContext` por SQLite in-memory, desativa o Redis fornecendo mocks de `IOutputCacheStore` e `IDistributedCache` em memória, e provê helpers de autenticação/seed.

### Suite C: Autenticação e Segurança (Middlewares)
Mapeado em `tests/AuthAndSecurityTests.cs`:
*   **IT-01 (Não Autorizado):** Chamada de criação de bolão sem header `X-API-KEY` (retorna `401 Unauthorized`).
*   **IT-02 (Chave Inválida):** Chamada com chave incorreta (retorna `401 Unauthorized`).

### Suite D: Criação e Ingresso no Bolão
Mapeado em `tests/SweepstakesFlowTests.cs`:
*   **IT-03 (Bloqueio no Endpoint):** Valida que chamadas HTTP duplicadas do mesmo criador para iniciar outro bolão ativo retornam `400 Bad Request`.
*   **IT-04 (Join via Código):** Cria o bolão como `Usuario A` e ingressa como `Usuario B` validando inserção do vínculo no banco.

### Suite E: Lógica de Guesses e Prazo Limite
Mapeado em `tests/GuessesAndIdempotencyTests.cs`:
*   **IT-05 (Deadline Expirada):** Cria o bolão, altera a deadline no banco para o passado e tenta postar um guess (retorna `400 Bad Request`).

### Suite F: Mecanismo de Idempotência
Mapeado em `tests/GuessesAndIdempotencyTests.cs`:
*   **IT-06 (Chave de Idempotência):** Envia palpites idênticos repetidos com a mesma `Idempotency-Key` e valida que a API retorna sucesso sem duplicar inserções na tabela `Guesses`.

---

## 4. Executando os Testes

Para executar toda a suíte de testes de unidade e integração:

```bash
dotnet test
```
