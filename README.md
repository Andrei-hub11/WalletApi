# WalletAPI - Sistema de Carteira Digital

Uma API RESTful para gerenciamento de carteira digital, desenvolvida com ASP.NET Core 8.0 e Entity Framework Core.

## Tecnologias

- **.NET 8.0**: Escolhido pela robustez, performance e forte tipagem
- **Entity Framework Core**: ORM para acesso a dados
- **PostgreSQL**: Banco de dados relacional
- **Identity**: Framework de autenticação e autorização
- **JWT**: Para autenticação stateless
- **xUnit**: Framework de testes
- **Shouldly**: Biblioteca para asserções mais legíveis
- **Moq**: Framework de mocking para testes

## Por que .NET em vez de Django/Python?

Embora Python seja uma excelente escolha para automações e scripts, o .NET foi escolhido por:

1. **Experiência**: Maior proficiência em C# e no ecossistema .NET
2. **Performance**: O .NET oferece excelente performance para aplicações web
3. **Tipagem Forte**: Ajuda a prevenir erros em tempo de compilação
4. **Ferramentas de Desenvolvimento**: Visual Studio e Rider oferecem excelente suporte
5. **Ecossistema Maduro**: NuGet, Identity, Entity Framework são bem estabelecidos

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

(e outros...)
```

## 📌 Endpoints

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

### Carteira (`/api/v1/wallet`)

#### GET /balance

Obtém o saldo da carteira do usuário autenticado.

#### POST /deposit/{userId}

Adiciona saldo à carteira (requer role Admin).

```json
{
  "amount": 0.0
}
```

### Transações (`/api/v1/transaction`)

#### POST /

Cria uma nova transação.

```json
{
  "receiverId": "string",
  "amount": 0.0,
  "description": "string",
  "type": "Transfer"
}
```

#### GET /

Lista transações do usuário com filtros.

```
?senderId=string
&receiverId=string
&transactionType=Transfer
&startDate=2024-03-20
&endDate=2024-03-21
&page=1
&pageSize=10
```

#### GET /{id}

Obtém detalhes de uma transação específica.

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

## Setup do Projeto

1. Clone o repositório
2. Configure a string de conexão no `appsettings.json`
3. Execute as migrations:

```bash
dotnet ef database update
```

4. Execute o projeto:

```bash
dotnet run
```

## Segurança

- Senhas hasheadas com Identity
- Tokens JWT com expiração
- Validação de roles
- Proteção contra SQL Injection via EF Core
- Validações de modelo
