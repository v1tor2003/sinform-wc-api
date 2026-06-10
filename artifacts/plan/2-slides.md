## Conteúdo Detalhado dos Slides e Requisitos Práticos

Abaixo está o detalhamento enriquecido dos slides e onde cada requisito técnico do projeto prático será abordado em paralelo.

### Slide 0: Capa (Título do curso)
*   **Título sugerido:** Construindo APIs Resilientes com .NET Minimal API: Do Zero ao Cache, Autenticação e Testes.
*   **Objetivo Pedagógico:** Apresentar a ementa do curso e instigar os alunos sobre o que é uma API de nível de produção.

### Slide 1: Sobre mim
*   **Conteúdo:** Breve histórico do instrutor, experiências com .NET/C# e stack de tecnologia atual.

### Slide 2: O que abordaremos
*   **Conteúdo:** A jornada do curso (Conceitos de redes/servidores -> Construção da API -> Acesso a dados -> Segurança -> Otimização com cache -> Testes de software).
*   **Destaque:** Apresentar o projeto do **Bolão** como o fio condutor de todo o aprendizado.

### Slide 3: Requisitos (Opcionais)
*   **Conteúdo:** Nivelamento sobre C#, SQL, e noções de conteinerização.
*   **Destaque:** Explicar que conceitos complexos serão desmistificados na prática.

### Slide 4: Ferramentas que usaremos
*   **Conteúdo:** VS Code (com C# Dev Kit), Docker Engine (para PostgreSQL e Redis), Git CLI.
*   **Destaque:** Preparação do ambiente de desenvolvimento local dos alunos.

### Slide 5: Contextualização
*   **Conteúdo:** O que é programação back-end, arquiteturas cliente-servidor, protocolo HTTP/S (Verbos, Status Codes, Headers) e o conceito de APIs RESTful.

### Slide 6: .NET (Standard, Framework e Core)
*   **Conteúdo:** Evolução histórica da plataforma .NET, o papel do CLR (Common Language Runtime), tipos de projetos (.NET CLI, ClassLib, WebAPI) e a arquitetura multiplataforma moderna.

### Slide 7: Minimal API & Início do Projeto
*   **Conteúdo:** MVC vs. Minimal APIs (Route Handlers), Injeção de Dependências (DI) e Inversão de Controle, Padrão REPR vs. Clean Architecture.
*   **Destaque de Requisito em Paralelo (Regras de Negócio e Estrutura):**
    *   Criação da estrutura de pastas baseada no padrão REPR.
    *   **Prática (`step-01-boilerplate`):** Inicialização do projeto web, criação do primeiro endpoint (`/health-check`) e configuração básica do contêiner DI.

### Slide 8: Acesso a Dados & Banco de Dados
*   **Conteúdo:** Bancos relacionais vs NoSQL. O que é um ORM (Object-Relational Mapper). Entity Framework Core vs Dapper. Migrations e Seed de dados. Padrão Repository.
*   **Destaque de Requisito em Paralelo (Database & Regras de Negócio):**
    *   Modelagem do Banco de Dados no PostgreSQL (Entidades: `User`, `Sweepstakes`, `Participant`, `Guess`, `OfficialPhaseResult`).
    *   Configuração do EF Core com migrations.
    *   **Prática (`step-03-sweepstakes-crud`):** Validação da regra: *"um usuário só pode criar 1 bolão ativo simultaneamente"*. Criação do código de convite seguro (`inviteCode`) com validação de fase (`phase`).

### Slide 9: Autenticação, Autorização e Documentação de API
*   **Conteúdo:** Teoria de JWT (JSON Web Tokens), Claims e Autorização corporativa vs. Prática Simplificada com X-API-KEY no cabeçalho. OpenAPI (Swagger vs. Scalar) para expor documentação interativa.
*   **Destaque de Requisito em Paralelo (Auth & Segurança):**
    *   Criação do endpoint `/auth/register` (retornando a chave X-API-KEY de acesso).
    *   Proteção de endpoints do bolão usando cabeçalho `X-API-KEY`.
    *   **Prática (`step-02-auth-users` & `step-04-participants-invites`):** Resgate do ID do usuário autenticado a partir do cabeçalho de autenticação para criar e associar participantes aos bolões.

### Slide 10: Otimização de Performance, Idempotência e Cache
*   **Conteúdo:** Por que usar cache distribuído (Redis/Valkey). ASP.NET Core OutputCaching vs. Padrão Cache-Aside. Introdução à Idempotência de APIs para garantir integridade sob falhas de rede.
*   **Destaque de Requisito em Paralelo (Cache & Idempotência):**
    *   **Prática (`step-05-betting-idempotency`):** Uso do cabeçalho `Idempotency-Key` no registro de palpites para evitar palpites duplicados.
    *   **Prática (`step-06-caching-redis`):** Configuração do ASP.NET Core OutputCaching para o endpoint `/leaderboard`, usando Redis como back-end distribuído para cache de respostas HTTP.

### Slide 11: Tarefas Assíncronas, Testes e Qualidade
*   **Conteúdo:** Execução em segundo plano com `BackgroundService` (.NET Workers). Princípios de TDD. Testes de Unidade (xUnit + NSubstitute) vs Testes de Integração (Web Application Factory).
*   **Destaque de Requisito em Paralelo (Workers & Tests):**
    *   **Prática (`step-07-background-cron`):** Desenvolvimento de um worker que roda periodicamente para verificar se bolões expiraram e possuem resultados oficiais homologados, fechar apostas, calcular pontuações de colocação e consolidar a classificação no banco e no cache.
    *   **Prática (`step-08-tests`):** Escrita de testes que validam cenários críticos: *usuário tenta enviar palpite após o deadline do bolão (guessesDeadline) (deve falhar)*; *usuário tenta criar um segundo bolão ativo (deve falhar)*; *teste de concorrência com Idempotency-Key*.
    *   **Tendências:** Uso de IA e pair programming (GitHub Copilot, Cursor, Vibe Coding).
