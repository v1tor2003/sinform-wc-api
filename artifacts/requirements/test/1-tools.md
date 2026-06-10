## Estrutura e Ferramentas de Teste

Para garantir a qualidade da API de Bolão, utilizaremos a seguinte stack de testes no ecossistema .NET:
- **xUnit**: O framework executor de testes padrão.
- **FluentAssertions**: Para asserções fluidas e legíveis (ex: `result.Should().BeEquivalentTo(...)`).
- **NSubstitute**: Para criação de Mocks e Stubs leves.
- **Microsoft.AspNetCore.Mvc.Testing**: Para criar servidores HTTP em memória através de `WebApplicationFactory` para testes de integração de ponta a ponta.
- **Testcontainers for .NET (Opcional/Recomendado)**: Para subir instâncias reais de PostgreSQL e Redis temporárias durante o ciclo de testes de integração no pipeline CI/CD.
