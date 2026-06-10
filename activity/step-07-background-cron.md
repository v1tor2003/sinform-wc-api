# Atividades do Passo 07: Processamento em Segundo Plano & Pontuação

Este guia detalha o passo a passo realizado para implementar o background worker (`SweepstakesProcessingWorker`) que processa periodicamente bolões expirados, pontua os participantes comparando seus palpites com os resultados oficiais homologados, encerra os bolões e limpa o cache de leaderboard correspondente.

---

## 1. Mapeamento dos Resultados Oficiais

### Criada a Entidade `OfficialPhaseResult`
Adicionamos o modelo em `api/Entities/OfficialPhaseResult.cs` contendo a tabela real homologada para as fases da copa:

```csharp
using System;

namespace SinformWcApi.Entities;

public class OfficialPhaseResult
{
    public string Phase { get; set; } = string.Empty;
    public string FirstPlace { get; set; } = string.Empty;
    public string SecondPlace { get; set; } = string.Empty;
    public string? ThirdPlace { get; set; }
    public bool IsHomologated { get; set; }
    public DateTime? HomologatedAt { get; set; }
}
```

### Atualização do `AppDbContext`
Mapeamos o `DbSet<OfficialPhaseResult>` em `api/AppDbContext.cs` definindo a chave primária como o nome da fase (`Phase`).

---

## 2. Endpoint Administrativo de Resultados (`POST /official-results`)

Implementamos o endpoint em `api/Features/OfficialResults/CreateOfficialResultEndpoint.cs` para permitir registrar a tabela final oficial de uma fase (e marcar como homologada). O endpoint faz um upsert na tabela `OfficialPhaseResults`.

---

## 3. Background Service de Processamento

Desenvolvemos o worker em `api/Workers/SweepstakesProcessingWorker.cs` herdando de `BackgroundService`:

*   **Periodicidade:** Executa a cada 10 segundos.
*   **Algoritmo de Processamento:**
    1.  Busca todos os bolões ativos (`IsActive == true`) cuja data limite de palpites (`GuessesDeadline`) já passou.
    2.  Para cada bolão, verifica se já existe o respectivo resultado homologado em `OfficialPhaseResults` para a fase associada.
    3.  Caso localizado, o worker busca todos os participantes e calcula suas pontuações individuais:
        *   **+10 pontos** se acertou a seleção em 1º lugar exato.
        *   **+10 pontos** se acertou a seleção em 2º lugar exato.
        *   **+10 pontos** se a regra `IncludeThird` estiver ativa e acertou o 3º lugar exato.
    4.  Atualiza `TotalScore` do participante.
    5.  Encerra o bolão marcando `IsActive = false`.
    6.  Limpa o cache de leaderboard no Redis utilizando o serviço `IOutputCacheStore.EvictByTagAsync("sb-leaderboard")`.

---

## 4. Configuração no Pipeline

Registramos o serviço hosted e o endpoint em `api/Program.cs`:

```csharp
// Registrar Worker
builder.Services.AddHostedService<SweepstakesProcessingWorker>();

// Registrar Endpoint
app.MapCreateOfficialResultEndpoint();
```

---

## 5. Aplicação das Migrations

Geramos e aplicamos o novo schema no banco de dados local:

```bash
# Criar a migration
dotnet ef migrations add AddOfficialPhaseResults

# Aplicar tabelas no PostgreSQL
dotnet ef database update
```
