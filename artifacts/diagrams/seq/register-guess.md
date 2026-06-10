## Diagrama de Sequência: Registrar Palpite (Idempotência e Deadline)

Ilustra o uso do cabeçalho `Idempotency-Key` e a validação do prazo final para palpites baseados em tabelas de classificação de fases.

```mermaid
sequenceDiagram
    autonumber
    actor Player as Sweepstakes Participant
    participant API as Minimal API Endpoint
    participant Cache as Redis Cache
    participant DB as Database (PostgreSQL)

    Player->>API: POST /guesses (sweepstakesId, finalTable)<br/>Headers: [X-API-KEY, Idempotency-Key]
    
    API->>Cache: GET idempotency:key:<value>
    alt Key exists in Cache (Replay Request)
        Cache-->>API: Returns cached response
        API-->>Player: Returns 200 OK (with original response from cache)
    else Key does not exist
        API->>DB: Query: Get sweepstakes and check if guesses_deadline > UTCNow
        DB-->>API: Sweepstakes details
        alt Deadline passed (Expired)
            API-->>Player: Returns 400 Bad Request ("Guesses deadline has expired.")
        else Within deadline
            Note over API: Validates if 'third' is provided based on the 'includeThird' rule
            API->>DB: INSERT/UPDATE guesses (predicted_first, predicted_second, predicted_third)
            DB-->>API: Success
            API->>Cache: SET idempotency:key:<value> (TTL = 24h)
            Cache-->>API: OK
            API-->>Player: Returns 201 Created (Guess registered)
        end
    end
```