# Atividades do Passo 04: Convites & Participantes

Este guia detalha o passo a passo realizado para implementar a lógica de ingresso de usuários em bolões existentes fornecendo o código de convite único.

---

## 1. Endpoint de Ingresso (`POST /sweepstakes/join`)

Isolamos o código em `api/Features/Sweepstakes/JoinSweepstakesEndpoint.cs`:

*   Protegido pelo cabeçalho `X-API-KEY`.
*   **Processo de Validação:**
    1.  O código recebido é normalizado para caixa alta (`ToUpper()`).
    2.  Busca o bolão associado. Se não for localizado ou o bolão estiver encerrado (`IsActive == false`), retorna `404 Not Found` (com mensagem descritiva).
    3.  Verifica se o usuário já faz parte do bolão. Se sim, retorna `400 Bad Request`.
    4.  Cria o registro associativo na tabela `Participants` iniciando o score do usuário como `0`.

---

## 2. Configuração no Pipeline da API

Atualizamos o arquivo `api/Program.cs` para registrar o novo endpoint de associação:

```csharp
app.MapJoinSweepstakesEndpoint();
```

---

## 3. Validação de Regras e Banco de Dados

*   Como a estrutura de banco de dados (tabelas e índice único composto) foi criada na etapa anterior, nenhuma migração adicional do Entity Framework foi necessária.
*   O índice composto `IX_Participants_SweepstakesId_UserId` atua como proteção na camada de persistência para evitar inserções concorrentes/duplicadas indesejadas.
