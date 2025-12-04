PagueVeloz API — Sistema de Processamento de Transações Financeiras
===================================================================

### API construída em .NET 9, usando Dapper + EF Core, com suporte a:

- Operações financeiras  
- Transferência entre contas  
- Idempotência  
- Retry + Backoff  
- Auditoria  
- Transações SQL  
- Health Checks  
- OpenTelemetry + Prometheus  
- RabbitMQ  
- Middlewares globais  
- Documentação Swagger  

### Sumário
-------

- Tecnologias utilizadas  
- Arquitetura  
- Execução via Docker  
- Endpoints principais  
- Exemplos de uso  
- RabbitMQ  
- Idempotência  
- Retry e Backoff  
- Health Checks  
- Métricas / OpenTelemetry / Prometheus  
- Swagger  
- Estrutura da solução  
- Licença  

### Tecnologias utilizadas
---------------------

### API construída com as seguintes tecnologias:

- .NET 9 Web API  
- EF Core + Dapper para acesso ao banco de dados  
- SQL Server como banco relacional  
- Serilog para logging estruturado  
- OpenTelemetry para métricas  
- Prometheus para coleta de métricas  
- FluentValidation para validação de DTOs  
- Swagger / OpenAPI para documentação  
- Polly para Retry/Backoff  
- RabbitMQ para envio de mensagens assíncronas  

### Arquitetura
-----------

### A arquitetura do sistema segue a divisão em camadas:

- PagueVeloz.Api  
  - Controllers  
  - Middlewares  
  - Validators  
  - Program.cs (configurações globais e DI)  
  - Swagger  
  - OpenTelemetry  
- PagueVeloz.Application  
  - Services (AccountService, TransactionService)  
  - Interfaces  
  - DTOs  
- PagueVeloz.Infrastructure  
  - Repositories (Dapper + EF)  
  - Idempotency  
  - Audit  
  - Database (DbContext)  
- PagueVeloz.Domain  
  - Entities  
  - Enums  
- PagueVeloz.Shared  
  - Middlewares  

### Execução via Docker
-------------------
- Imagem: reinaldofc80/pagueveloz-api
- Baixe ou copie o arquivo docker-compose.yml para uma pasta da sua máquina.  
- Abra um terminal dentro dessa pasta (onde o docker-compose.yml está).
- Execute o comando abaixo para baixar as imagens, construir a aplicação e iniciar todos os serviços: docker-compose up --build -d
- Baixar a imagem reinaldofc80/pagueveloz-api
- Baixar e iniciar as dependências (Banco de dados, RabbitMQ ... )
- Levantar toda a stack automaticamente

#Para visualizar a API no Swagger
 URL do Swagger: http://localhost:8080/swagger/index.html  

#Para remover containers e volumes, execute:
- docker-compose down -v

  
### Endpoints principais
-------------------

- Conta:  
  - POST /api/Account/CriarConta — Cria uma nova conta  
  - GET /api/Account/BuscarConta/{accountNumber} — Busca conta pelo número  
  - PUT /api/Account/AtualizarConta/{accountNumber} — Atualiza dados básicos  

- Transações:  
  - POST /api/Transaction/operacao — Processa qualquer operação financeira (Credit, Debit, Reserve, Capture, Reversal)  
  - POST /api/Transaction/transferencia — Realiza transferência entre contas  

### Exemplos de uso
---------------

- Criar conta:

  - POST /api/Account/CriarConta  
    ```json
    {
      "accountNumber": "CC-0001",
      "balance": 0,
      "reservedBalance": 0,
      "creditLimit": 0,
      "referenceId": "test-credit-001"
    }
    ```

- Operação financeira (Crédito):

  - POST /api/Transaction/operacao  
    ```json
    {
      "operation": "Credit",
      "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
      "amount": 100.00,
      "currency": "BRL",
      "referenceId": "credit-001"
    }
    ```

- Transferência:

  - POST /api/Transaction/transferencia  
    ```json
    {
      "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
      "targetAccountId": "32317970-9624-40B4-B9EE-80D0146D2E3B",
      "amount": 100.00,
      "currency": "BRL",
      "referenceId": "transfer-001"
    }
    ```

### RabbitMQ
--------

- Painel: http://localhost:15672  
- Usuário: guest  
- Senha: guest  

### Idempotência
------------

- Todas as operações usam Idempotency-Key no header  
- Se a chave já existir, retorna a mesma resposta gravada  
- Se a chave não existir, processa a operação e salva a resposta  

### Retry e Backoff
---------------

- Polly é utilizado para retry de transações  
- Lida com timeout, deadlocks e falhas momentâneas no banco  

### Health Checks
-------------

- /health — verifica se a API está ativa  
- /health/ready — verifica se o SQL Server está operacional  

### Métricas / OpenTelemetry / Prometheus
-------------------------------------

- Endpoint para scraping: /metrics  
- Inclui:  
  - Requests por endpoint  
  - Tempo médio das requisições  
  - Uso de CPU/memória  
  - Instrumentação de HttpClient  
  - Instrumentação do servidor ASP.NET  

### Swagger
-------

- URL do Swagger: http://localhost:8080/swagger/index.html  
- Permite testar todos os endpoints via interface web  

### Estrutura da solução
-------------------

- PagueVeloz  
  - Api  
  - Application  
  - Infrastructure  
  - Domain  
  - Shared  
    - Middlewares  

### Licença
-------

- Este projeto é licenciado sob MIT  

### Contribuições
-------------

- Pull requests são bem-vindos  
- Sugestões também são aceitas
