## Idempotência em APIs HTTP

### O que é Idempotência?
Um endpoint é considerado **idempotente** se o efeito de realizar múltiplas requisições idênticas for exatamente o mesmo que realizar uma única requisição. 
- Métodos como `GET`, `PUT` e `DELETE` são naturalmente idempotentes por definição.
- O método `POST` não é idempotente. Se o usuário clicar no botão "Enviar Palpite" (Place Guess) três vezes seguidas devido a uma lentidão na rede, a API pode processar três palpites e registrar incorretamente.

### Como Implementar?
1. O cliente gera um identificador único (ex: UUID/GUID) chamado de **Idempotency-Key** e o envia no cabeçalho da requisição HTTP:
   `Idempotency-Key: 9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d`
2. Ao receber a requisição, o Endpoint:
   - Verifica no cache se aquela chave já foi processada.
   - **Se sim**: Retorna imediatamente a resposta gravada anteriormente, sem reprocessar as regras de negócio.
   - **Se não**: Processa a requisição, salva o resultado no banco, grava o resultado no cache associado à chave (com um tempo de expiração/TTL) e retorna a resposta.