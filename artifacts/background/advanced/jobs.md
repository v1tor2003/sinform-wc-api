## Background Workers (Tarefas de Segundo Plano)

A data e hora limite para que as pessoas palpitem em um bolão é configurada pelo criador. Após essa data, nenhuma aposta pode ser feita e as pontuações e classificações finais devem ser calculadas.

### Implementação com `BackgroundService` no .NET
No .NET, podemos herdar da classe abstrata `BackgroundService` para criar workers duráveis que rodam de forma concorrente e assíncrona com o servidor HTTP:

- **Loop de Monitoramento**: O worker executa periodicamente (ex: a cada 60 segundos).
- **Processamento**:
  1. Consulta no banco de dados quais bolões ativos atingiram o prazo limite e ainda não foram finalizados.
  2. Altera o status do bolão para "Finalizado".
  3. Compara os palpites salvos (tabela de classificados) com o resultado oficial homologado da respectiva fase da Copa.
  4. Calcula as pontuações individuais e consolida a classificação final dos participantes.
  5. Atualiza o banco e invalida o cache do Output Cache correspondente para forçar a renderização da classificação final de vencedores.
  6. Envia notificações ou e-mails de encerramento (se aplicável).