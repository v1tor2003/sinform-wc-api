## REPR (Request-Endpoint-Response) Pattern vs. Clean Architecture

Ao construir uma API comercial com Minimal APIs, a falta de estrutura rígida pode levar a arquivos `Program.cs` gigantescos e difíceis de manter. Para resolver isso, discutiremos duas abordagens de organização:

### Clean Architecture (Arquitetura Limpa)
Focada na separação de conceitos e independência de frameworks externos (bancos de dados, UI).
- **Core (Domain & Application)**: Contém as entidades, regras de negócio e casos de uso (Handlers/Commands/Queries). Não conhece o banco de dados nem o ASP.NET.
- **Infrastructure**: Implementa detalhes técnicos (acesso ao banco via EF Core, envio de e-mails, cache).
- **Presentation (API)**: Ponto de entrada que recebe as requisições HTTP e as encaminha para a camada Application.
- *Desvantagem no aprendizado:* Exige múltiplos projetos na solution (`.csproj`) e faz o aluno navegar por muitos arquivos para entender um único fluxo (ex: de uma rota HTTP até o banco).

### Padrão REPR (Request-Endpoint-Response)
O padrão **REPR** (cunhado por Steve Smith / Ardalis) defende que APIs modernas são compostas por endpoints individuais que realizam uma tarefa específica:
$$\text{Endpoint} \approx \text{Request} + \text{Logic} + \text{Response}$$

No REPR:
- **Request**: DTO (Data Transfer Object) com os dados de entrada da requisição.
- **Endpoint**: Uma classe ou função isolada que recebe a Request, executa a lógica e retorna a Response.
- **Response**: DTO com os dados de saída retornados ao cliente.

> [!NOTE]
> **Por que usaremos REPR neste curso?**
> Para simplificar a curva de aprendizado inicial. O aluno foca na lógica do caso de uso de forma verticalizada. Em vez de abrir 4 projetos e criar 5 classes em diretórios distintos, toda a lógica daquele endpoint (ex: `CreateSweepstakes`) fica agrupada logicamente ou até em um único arquivo estruturado. Mantemos a separação de conceitos limpa, mas simplificamos a navegação no código.
