# WalletAPI - Sistema de Carteira Digital

Uma API RESTful conceitual e muito simples para gerenciamento de carteira digital, desenvolvida com ASP.NET Core 8.0 e Entity Framework Core.

## Tecnologias

- **.NET 8.0**: Escolhido pela robustez, performance e forte tipagem
- **Entity Framework Core**: ORM para acesso a dados
- **PostgreSQL**: Banco de dados relacional
- **Identity**: Framework de autenticação e autorização
- **JWT**: Para autenticação stateless
- **xUnit**: Framework de testes
- **Shouldly**: Biblioteca para asserções mais legíveis
- **Docker**: Para iniciar o banco de dados

## Por que .NET em vez de Django/Python?

Embora Python seja uma excelente escolha para automações e scripts, o .NET foi escolhido por:

1. **Experiência**: Maior proficiência em C# e no ecossistema .NET
2. **Performance**: O .NET oferece excelente performance para aplicações web
3. **Tipagem Forte**: Ajuda a prevenir erros em tempo de compilação

## Setup do Projeto

### Opção 1: Setup com Docker (Recomendado)

#### Pré-requisitos

- Docker instalado
- .NET SDK 8.0 instalado (para gerar certificado)

### Setup Local

1. Clone o repositório
2. Abra a pasta do projeto e faça 'docker-compose up -d'
3. Execute as migrations:
   ```bash
   dotnet ef database update
   ```
4. Execute o projeto:
   ```bash
   dotnet run
   ```

## Testes de API com Postman

Para facilitar a execução dos testes, você pode importar a coleção do Postman diretamente.

1. Baixe a coleção do Postman [aqui](./WalletAPI.postman_collection.json).
2. Importe a coleção no Postman:
   - Abra o Postman.
   - Vá em **File > Import** e selecione o arquivo JSON da coleção.
3. Execute os testes de API conforme necessário.

A coleção contém os testes básicos para todas as rotas da API.

## Autenticação

A API usa JWT (JSON Web Tokens) para autenticação. O token deve ser enviado no header `Authorization` como `Bearer {token}`.

### Dados de Seed

O sistema é inicializado com alguns dados para demonstração:

#### Usuário Admin

```
Email: admin@email.com
Senha: Admin@123&&
```

#### Usuários Comuns

```
Email: joao@email.com
Senha: Senha@123

Email: maria@email.com
Senha: Senha@123
```

## Endpoints

### Autenticação (`/api/v1/auth`)

#### POST /login

Login no sistema.

```json
{
  "email": "string",
  "password": "string"
}
```

#### POST /register

Registro de novo usuário.

```json
{
  "email": "string",
  "password": "string",
  "name": "string"
}
```

### Carteira (`/api/v1/wallets`)

#### GET / (Requer role Admin)

Lista todas as carteiras com filtros avançados.

```
?userId=string
&minBalance=0
&maxBalance=1000
&createdStartDate=2024-03-20
&createdEndDate=2024-03-21
&updatedStartDate=2024-03-20
&updatedEndDate=2024-03-21
&page=1
&pageSize=10
```

#### GET /user/balance (Requer autenticação)

Obtém o saldo da carteira do usuário autenticado.

#### POST /deposit/{userId} (Requer role Admin)

Adiciona saldo à carteira de um usuário específico.

```json
{
  "amount": 0.0
}
```

### Transações (`/api/v1/transactions`)

#### GET / (Requer role Admin)

Lista todas as transações com filtros avançados.

```
?senderId=string
&receiverId=string
&transactionType=Transfer
&startDate=2024-03-20
&endDate=2024-03-21
&page=1
&pageSize=10
```

#### GET /user (Requer autenticação)

Lista as transações do usuário autenticado.

#### POST / (Requer autenticação)

Cria uma nova transação.

```json
{
  "receiverId": "string",
  "amount": 0.0,
  "description": "string",
  "type": "Transfer"
}
```

## Fluxo de Dados

1. **Registro**:

   - Cria usuário
   - Adiciona à role "User"
   - Cria carteira zerada

2. **Login**:

   - Valida credenciais
   - Retorna token JWT
   - Inclui saldo e últimas transações

3. **Transações**:
   - Verifica saldo
   - Atualiza carteiras
   - Registra transação

## Testes

O projeto inclui testes unitários para:

- Serviços (AuthService, WalletService)
- Entidades (User, Wallet, Transaction)
- Validações de negócio

Para executar os testes:

```bash
dotnet test
```

## Segurança

- Senhas hasheadas com Identity
- Tokens JWT com expiração
- Validação de roles
- Proteção contra SQL Injection via EF Core
- Validações de modelo
