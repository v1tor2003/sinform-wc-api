## Estrutura de Branches Git

Para que os alunos possam acompanhar o desenvolvimento de forma prática e incremental, utilizaremos o seguinte roteiro de branches. Cada branch representa o estado final do código para o respectivo tópico estudado.

### Comandos de Navegação
Para ir para uma etapa específica no terminal do aluno:
```bash
# Limpar modificações locais e alternar para a branch do passo
git checkout -f step-03-sweepstakes-crud
```

### Mapa de Branches

```
main (Docs & README)
  └── step-01-boilerplate (Minimal API básica & Docker Compose Postgres/Redis)
        └── step-02-auth-users (API Key, Registro e Login)
              └── step-03-sweepstakes-crud (Criação de Sweepstakes + Regra: 1 ativo simultâneo)
                    └── step-04-participants-invites (Convidar via código e Participar de vários)
                          └── step-05-betting-idempotency (Guesses, Deadline e Idempotency-Key)
                                └── step-06-caching-redis (Redis para Leaderboard)
                                      └── step-07-background-cron (Worker de encerramento & Top 3)
                                            └── step-08-tests (Testes unitários e integração)
```