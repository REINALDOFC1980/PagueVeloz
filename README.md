
# **PagueVeloz API — Sistema de Processamento de Transações Financeiras**

API construída em **.NET 9**, utilizando **Dapper + EF Core**, com foco em **operações financeiras seguras, rápidas e escaláveis**.

## 🚀 **Recursos principais**

- Operações financeiras completas  
- Transferência entre contas  
- Idempotência nativa  
- Retry com Backoff (Polly)  
- Auditoria automática  
- Transações SQL  
- Health Checks  
- OpenTelemetry + Prometheus  
- RabbitMQ  
- Middlewares globais  
- Documentação Swagger  

## 📚 **Sumário**

1. Tecnologias utilizadas  
2. Arquitetura  
3. Execução via Docker  
4. Endpoints principais  
5. Exemplos de uso  
6. RabbitMQ  
7. Idempotência  
8. Retry e Backoff  
9. Health Checks  
10. Métricas / OpenTelemetry / Prometheus  
11. Swagger  
12. Estrutura da solução  
13. Licença  
14. Contribuições  

## 🛠 **Tecnologias utilizadas**

- **.NET 9 Web API**  
- **EF Core + Dapper**  
- **SQL Server**  
- **Serilog**  
- **OpenTelemetry**  
- **Prometheus**  
- **FluentValidation**  
- **Swagger / OpenAPI**  
- **Polly**  
- **RabbitMQ**  

## 🧱 **Arquitetura**

```
PagueVeloz
 ├── Api
 │    ├── Controllers
 │    ├── Middlewares
 │    ├── Validators
 │    ├── Program.cs
 │    ├── Swagger
 │    └── OpenTelemetry
 ├── Application
 │    ├── Services
 │    ├── Interfaces
 │    └── DTOs
 ├── Infrastructure
 │    ├── Repositories (Dapper + EF)
 │    ├── Idempotency
 │    ├── Audit
 │    └── DbContext
 ├── Domain
 │    ├── Entities
 │    └── Enums
 └── Shared
      └── Middlewares
```

## 🐳 **Execução via Docker**

Imagem:
```
reinaldofc80/pagueveloz-api
```

<<<<<<< HEAD
Execute:
```bash
docker-compose up -d
```
=======
### Execução via Docker
-------------------
- Imagem: reinaldofc80/pagueveloz-api
- Baixe ou copie o arquivo docker-compose.yml para uma pasta da sua máquina.  
- Abra um terminal dentro dessa pasta (onde o docker-compose.yml está).
Execute o comando:
bash
docker-compose up -d
- Baixar a imagem reinaldofc80/pagueveloz-api
- Baixar e iniciar as dependências (Banco de dados, RabbitMQ ... )
- Levantar toda a stack automaticamente
>>>>>>> d744e1045c3ac67bb52d0c907f01b8dd02797a05
Swagger:
```
http://localhost:8080/swagger/index.html
```

Remover containers:
```bash
docker-compose down -v
```

### Execução via Local
-------------------
dotnet run --project PagueVeloz.api
Swagger:
```
http://localhost:5247/swagger/index.html
```

## 📌 **Endpoints principais**

### Conta
- POST `/api/Account/CriarConta`  
- GET `/api/Account/BuscarConta/{accountNumber}`  
- PUT `/api/Account/AtualizarConta/{accountNumber}`  

### Transações
- POST `/api/Transaction/operacao`  
- POST `/api/Transaction/transferencia`  

## 🧪 **Exemplos de uso**

### Criar conta
```json
{
  "accountNumber": "CC-0001",
  "balance": 0,
  "reservedBalance": 0,
  "creditLimit": 0,
  "referenceId": "test-credit-001"
}
```

### Crédito
```json
{
  "operation": "Credit",
  "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
  "amount": 100.00,
  "currency": "BRL",
  "referenceId": "credit-001"
}
```

### Transferência
```json
{
  "accountId": "4D1746D2-5770-4820-8381-18EDB119846B",
  "targetAccountId": "32317970-9624-40B4-B9EE-80D0146D2E3B",
  "amount": 100.00,
  "currency": "BRL",
  "referenceId": "transfer-001"
}
```

## 📬 **RabbitMQ**

- Painel: `http://localhost:15672`  
- Usuário: `guest`  
- Senha: `guest`  

## 🔁 **Idempotência**

- Header obrigatório:  
```
Idempotency-Key: <guid>
```

## 🔄 **Retry e Backoff**

- Implementado com Polly  

## ❤️ **Health Checks**

- `/health`  
- `/health/ready`  

## 📊 **Métricas / OpenTelemetry / Prometheus**

- Endpoint: `/metrics`

## 📘 **Swagger**

```
http://localhost:8080/swagger/index.html
```

## 🗂 **Estrutura da solução**

```
PagueVeloz
 ├── Api
 ├── Application
 ├── Infrastructure
 ├── Domain
 └── Shared
```

## 📄 **Licença**

MIT

## 🤝 **Contribuições**

Pull requests são bem-vindos!
