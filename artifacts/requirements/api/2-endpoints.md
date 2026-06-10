## Contratos dos Endpoints (API Spec)

### Autenticação

#### `POST /auth/register` (Público)
*   **Request Body:**
    ```json
    {
      "name": "João Silva",
      "email": "joao@email.com",
      "password": "SenhaSegura123!"
    }
    ```
*   **Responses:**
    - `201 Created`: Usuário registrado com sucesso.
    - `400 Bad Request`: Dados inválidos ou e-mail já cadastrado.

#### `POST /auth/login` (Público)
*   **Request Body:**
    ```json
    {
      "email": "joao@email.com",
      "password": "SenhaSegura123!"
    }
    ```
*   **Responses:**
    - `200 OK`: Retorna a API Key gerada para autenticação.
      ```json
      {
        "apiKey": "usr_live_9b1deb4d3b7d4badbdd2b0d7b3dcb6d"
      }
      ```
    - `401 Unauthorized`: E-mail ou senha incorretos.

---

### Gerenciamento de Bolões

#### `POST /sweepstakes` (Requer Autenticação via `X-API-KEY`)
*   **Request Body:**
    ```json
    {
      "name": "Bolão do Grupo A da Copa",
      "description": "Previsões para a fase de grupos do Grupo A",
      "phase": "Group A",
      "guessesDeadline": "2026-06-15T18:00:00Z",
      "qualifiedCount": 2,
      "includeThird": false
    }
    ```
*   **Responses:**
    - `201 Created`: Bolão criado com sucesso. Retorna os dados com o código gerado.
      ```json
      {
        "id": "e2a3928e-5b12-421f-82bb-78bc283f218a",
        "name": "Bolão do Grupo A da Copa",
        "description": "Previsões para a fase de grupos do Grupo A",
        "phase": "Group A",
        "inviteCode": "DF89RE",
        "guessesDeadline": "2026-06-15T18:00:00Z",
        "qualifiedCount": 2,
        "includeThird": false,
        "isActive": true
      }
      ```
    - `400 Bad Request`: Dono já possui outro bolão ativo, dados de entrada inválidos ou fase inválida.

#### `POST /sweepstakes/join` (Requer Autenticação via `X-API-KEY`)
*   **Request Body:**
    ```json
    {
      "inviteCode": "DF89RE"
    }
    ```
*   **Responses:**
    - `200 OK`: Associação efetuada. Retorna o ID do participante cadastrado.
      ```json
      {
        "participantId": "c0a2948a-6b22-411a-96cc-89de384f519b",
        "sweepstakesId": "e2a3928e-5b12-421f-82bb-78bc283f218a",
        "sweepstakesName": "Bolão do Grupo A da Copa"
      }
      ```
    - `404 Not Found`: Código de convite não localizado ou bolão já encerrado.
    - `400 Bad Request`: Usuário já é participante desse bolão.

#### `GET /sweepstakes/{id}/leaderboard` (Requer Autenticação via `X-API-KEY`)
*   **Headers:** `X-API-KEY: <api-key>`
*   **Responses:**
    - `200 OK`: Retorna a classificação. *Nota: Este endpoint utiliza o ASP.NET Core OutputCaching para cachear a resposta HTTP.*
      ```json
      {
        "sweepstakesId": "e2a3928e-5b12-421f-82bb-78bc283f218a",
        "leaderboard": [
          { "position": 1, "name": "Maria Santos", "score": 20 },
          { "position": 2, "name": "João Silva", "score": 15 },
          { "position": 3, "name": "Ana Costa", "score": 10 }
        ]
      }
      ```

---

### Guesses e Apostas

#### `POST /guesses` (Requer Autenticação via `X-API-KEY` e Idempotência)
*   **Headers:** 
    - `X-API-KEY: <api-key>`
    - `Idempotency-Key: <guid>`
*   **Request Body:**
    ```json
    {
      "sweepstakesId": "e2a3928e-5b12-421f-82bb-78bc283f218a",
      "finalTable": {
        "first": "Brasil",
        "second": "Alemanha",
        "third": null
      }
    }
    ```
*   **Responses:**
    - `201 Created`: Guess registrado com sucesso.
      ```json
      {
        "id": "7a8b8c8d-9e9f-0a0b-1c1d-2e2f3a3b4c4d",
        "sweepstakesId": "e2a3928e-5b12-421f-82bb-78bc283f218a",
        "finalTable": {
          "first": "Brasil",
          "second": "Alemanha",
          "third": null
        },
        "guessedAt": "2026-06-12T14:30:00Z"
      }
      ```
    - `400 Bad Request`: Data limite excedida, usuário não participa deste bolão, ou o campo `third` é nulo mas o bolão exige o terceiro colocado.
    - `409 Conflict`: Conflito ou erro de idempotência.