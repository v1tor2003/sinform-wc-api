## Autenticação e Autorização (API Key vs. JWT)

A API do Bolão exige segurança para garantir que usuários não joguem em bolões alheios sem permissão e que apenas o dono do bolão possa gerenciar suas configurações.

### Abordagem Conceitual: JSON Web Tokens (JWT)
O JWT é o padrão de mercado mais robusto para APIs em produção:
- **Estrutura**: Composto por Header, Payload (contendo as *Claims* de identidade como ID do usuário e e-mail) e Signature (assinatura criptográfica que evita adulterações).
- **Sem Estado (Stateless)**: A API não precisa consultar o banco de dados a cada requisição para saber quem é o usuário; ela apenas valida a assinatura criptográfica do token enviado no cabeçalho `Authorization: Bearer <token>`.

### Abordagem Prática no Curso: Autenticação baseada em API Key (`X-API-KEY`)
> [!NOTE]
> **Simplificação Didática**
> Embora o JWT seja o padrão ideal de produção, para simplificar a implementação e o entendimento do aluno iniciante (evitando a complexidade de configuração de chaves assimétricas, emissores, tempos de expiração e bibliotecas JWT adicionais), utilizaremos no código prático um cabeçalho customizado de autenticação chamado **`X-API-KEY`**.
>
> Cada usuário cadastrado terá uma chave de acesso única gerada no banco de dados (ex: um token seguro de formato `usr_live_9b1deb4d...`). Nas requisições autenticadas, o aluno enviará essa chave no cabeçalho:
> `X-API-KEY: usr_live_9b1deb4d...`
>
> Um middleware customizado interceptará a requisição, buscará o usuário associado a essa chave no banco de dados e adicionará a identidade do usuário ao contexto da requisição (`HttpContext.Items["User"]` ou claims do `ClaimsPrincipal`).