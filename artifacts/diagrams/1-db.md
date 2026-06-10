## Modelo Entidade-Relacionamento (DER / Banco de Dados)

O banco de dados relacional (PostgreSQL) é modelado para gerenciar usuários, bolões, membros e palpites baseados em previsões de classificação de fases.

```mermaid
erDiagram
    users ||--o{ sweepstakes : "creates/manages (Owner)"
    users ||--o{ participants : "joins"
    sweepstakes ||--|{ participants : "contains"
    participants ||--o{ guesses : "registers"
    official_phase_results ||--o{ sweepstakes : "determines points of"

    users {
        uuid id PK
        string name
        string email
        string password_hash
        datetime created_at
    }

    sweepstakes {
        uuid id PK
        string name
        string description
        string phase
        string invite_code UK
        uuid creator_id FK
        int qualified_count
        boolean include_third
        datetime guesses_deadline
        boolean is_active
        datetime created_at
    }

    participants {
        uuid id PK
        uuid sweepstakes_id FK
        uuid user_id FK
        int total_score
        datetime joined_at
    }

    official_phase_results {
        string phase PK
        string first_place
        string second_place
        string third_place "NULL"
        boolean is_homologated
        datetime homologated_at
    }

    guesses {
        uuid id PK
        uuid participant_id FK
        string predicted_first
        string predicted_second
        string predicted_third "NULL"
        datetime guessed_at
    }
```