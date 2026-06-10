## Casos de Testes de Unidade (Domain Rules)

Os testes de unidade validam as funções puras de regras de domínio, sem acesso ao banco de dados ou rede.

### Suite A: Validador de Pontuação de Guesses (`IScoringStrategy`)
Valida o cálculo correto dos pontos baseados no guess do participante vs. a tabela final homologada da fase.

*Configuração do Bolão Base para os testes:* `qualifiedCount` = 2, `includeThird` = false (exceto UT-05).

| ID | Player's Prediction (`finalTable`) | Official Phase Result | Expected Points | Case Description |
| :--- | :--- | :--- | :--- | :--- |
| **UT-01** | 1st Brazil, 2nd Switzerland | 1st Brazil, 2nd Switzerland | **20 points** | Exact match for both positions (10 + 10). |
| **UT-02** | 1st Switzerland, 2nd Brazil | 1st Brazil, 2nd Switzerland | **0 points** | Both teams qualified, but swapped positions. Strict order validation yields 0 points. |
| **UT-03** | 1st Brazil, 2nd Cameroon | 1st Brazil, 2nd Switzerland | **10 points** | Exact match for 1st place (10) and incorrect 2nd place (0). |
| **UT-04** | 1st Cameroon, 2nd Serbia | 1st Brazil, 2nd Switzerland | **0 points** | Complete miss. None of the predicted teams qualified. |
| **UT-05** | 1st Brazil, 2nd Switzerland, 3rd Serbia | 1st Brazil, 2nd Serbia, 3rd Switzerland | **10 points** | *Sweepstakes with includeThird=true.* Exact 1st (10), 2nd & 3rd are out of position (0 + 0). |

### Suite B: Regra de Limite de Bolão Ativo
Garante que a lógica do domínio impede a criação de novos bolões se o usuário já possuir um ativo.

- **UT-06 (Sucesso):** Quando a contagem de bolões ativos do usuário for `0`, a criação deve ser permitida.
- **UT-07 (Falha):** Quando a contagem de bolões ativos for `>= 1`, a lógica de negócio deve lançar uma exceção de domínio (ex: `DomainException`).
