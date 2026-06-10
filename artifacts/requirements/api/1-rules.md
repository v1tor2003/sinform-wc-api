## Regras de Negócio do Domínio

### R01 - Usuários e Autenticação
- Qualquer usuário pode se cadastrar informando Nome, E-mail (único) e Senha.
- A autenticação gera uma chave de acesso única (**API Key**) associada ao usuário, que deve ser enviada nas requisições protegidas via cabeçalho `X-API-KEY`.

### R02 - Criação de Bolão (Previsão de Classificados)
- Um usuário autenticado pode criar um bolão (`Sweepstakes`). Ele se torna o **Owner** (Dono) daquele bolão.
- **Limitação Crítica:** Um usuário **só pode ter 1 bolão ativo simultaneamente** sob sua autoria. Se tentar criar outro enquanto possuir um ativo, a requisição é rejeitada.
- O dono define as seguintes configurações do bolão (`Sweepstakes`):
  - **name** e **description** do bolão.
  - **phase**: A fase da Copa do Mundo que o bolão monitora (ex: "Group A", "Group B", "Round of 16"). Este campo é validado contra uma lista oficial de fases da Copa do Mundo.
  - **qualifiedCount**: O número de seleções que passam para a próxima etapa (ex: 2 seleções para fase de grupos, 1 seleção para mata-mata).
  - **includeThird**: Booleano indicando se a previsão deve conter também o terceiro colocado (ex: útil para grupos que classificam os melhores terceiros lugares).
  - **guessesDeadline**: Prazo limite para os guesses/previsões (deve ser anterior ao início dos jogos daquela fase).
- O sistema gera automaticamente um **código de convite único e alfanumérico com 6 caracteres** (`inviteCode`) (ex: `A9X7WR`).

### R03 - Associação de Participantes
- Um usuário pode participar de **múltiplos bolões** simultaneamente.
- O ingresso em um bolão é feito fornecendo o código de convite (`inviteCode`) no endpoint `POST /sweepstakes/join`.
- O criador do bolão é registrado automaticamente como participante (`Participant`) ao criá-lo.

### R04 - Guesses de Classificados (Tabela Final)
- Um participante envia um guess indicando a classificação final projetada para a fase.
- O guess consiste em enviar um objeto de **Tabela Final** (`finalTable`):
  - `first`: Seleção projetada em 1º lugar.
  - `second`: Seleção projetada em 2º lugar.
  - `third`: Seleção projetada em 3º lugar (opcional, validado e exigido apenas se a regra do bolão `includeThird` for ativa).
- **Restrição de Tempo:** Nenhum guess pode ser criado ou alterado após a data e hora limite (`guessesDeadline`) configurada pelo dono.
- **Idempotência:** O registro de guess deve exigir o cabeçalho `Idempotency-Key`. Se a mesma chave for enviada dentro do intervalo de expiração (24h), a API deve retornar o guess cadastrado sem duplicar registros no banco de dados.

### R05 - Pontuação e Encerramento
- A classificação geral (Leaderboard) mostra os participantes ordenados pelos pontos acumulados.
- Os guesses não armazenam pontuação própria. A pontuação acumulada é atribuída diretamente ao participante (`Participant`) quando o resultado oficial da fase (`OfficialPhaseResult`) é homologado:
  - **Pontuação por Posição Exata**: O participante ganha **10 pontos** para cada posição em que a seleção prevista coincide exatamente com a tabela oficial (ex: previu Brasil em 1º e ficou em 1º).
  - **Ordem Incorreta**: Se a seleção prevista se classificar mas em posição diferente da indicada no guess, **nenhum ponto** é concedido por essa seleção (0 pontos).
- O encerramento do bolão é disparado pelo background worker assim que o tempo expira e a tabela oficial é inserida. O worker calcula os vencedores e atualiza o ranking definitivo, coroando o **Top 3** melhores colocados do bolão.