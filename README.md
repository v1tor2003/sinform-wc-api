# Sinform World Cup Standings API

API de bolão para predição de posições das fases da Copa do Mundo, desenvolvida incrementalmente usando .NET Minimal API.

Este repositório foi construído seguindo um currículo passo a passo mapeado em branches Git.

## Estrutura do Repositório

- `artifacts/`: Especificações de requisitos, modelos de dados, documentação conceitual e ementa do curso.
- `code/`: O código-fonte do projeto ASP.NET Core Minimal API.
- `activity/`: Registros de atividades práticas realizadas para que cada etapa possa ser reproduzida facilmente.

## Currículo e Branches

1. **`main`**: Apenas documentação inicial.
2. **`step-01-boilerplate`**: Setup inicial do projeto .NET, Scalar, Docker Compose (Postgres e Redis).
3. **`step-02-auth-users`**: Autenticação via `X-API-KEY`, registro e login.
4. **`step-03-sweepstakes-crud`**: Criação de bolões com regras de limitação.
5. **`step-04-participants-invites`**: Associação de participantes via código.
6. **`step-05-betting-idempotency`**: Envio de palpites com controle de prazo e idempotência.
7. **`step-06-caching-redis`**: Caching do Leaderboard usando Redis e Output Caching.
8. **`step-07-background-cron`**: Worker em segundo plano para consolidação de pontuações.
9. **`step-08-tests`**: Cobertura de testes unitários e de integração (e2e).

## Como rodar o projeto localmente (Setup Inicial)

Consulte o arquivo `/activity/step-01-boilerplate.md` para instruções de reprodução.
