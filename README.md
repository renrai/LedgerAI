# LedgerAI

> API de finanças pessoais com categorização automática por IA, construída em **.NET 10 / C# 14** com arquitetura DDD.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp)](https://learn.microsoft.com/dotnet/csharp)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## O que é

O LedgerAI registra contas, transações e orçamentos e usa um modelo de linguagem para **classificar automaticamente cada lançamento** ("Uber 23/09" → *Transporte*). O projeto existe para exercitar, num cenário realista, o que há de mais novo no ecossistema .NET:

| Recurso | Onde aparece |
|---|---|
| **.NET 10 / C# 14** (`field`, extension members, GUID v7) | Domínio e Application |
| **Minimal APIs** com OpenAPI nativo + **Scalar** | `LedgerAI.API` |
| **EF Core 10 + Npgsql** (PostgreSQL) | `LedgerAI.Infrastructure` |
| **Microsoft.Extensions.AI** (`IChatClient`) para categorização | `LedgerAI.Infrastructure/AI` |
| **Server-Sent Events** para alertas de orçamento em tempo real | `LedgerAI.API/Endpoints/Events` |
| **HybridCache** para dashboards | `LedgerAI.API` |
| **Servidor MCP** (Model Context Protocol) para consultar finanças pelo Claude/Copilot | `LedgerAI.Mcp` *(em breve)* |
| **.NET Aspire** para orquestração local | `LedgerAI.AppHost` *(em breve)* |
| JWT, FluentValidation, Serilog, Health Checks, Docker, Testcontainers | transversal |

## Arquitetura

```
src/
├── LedgerAI.API             # Minimal API, endpoints, middlewares, DI
├── LedgerAI.Application     # Casos de uso, DTOs, validadores, interfaces de serviço
├── LedgerAI.Domain          # Entidades, value objects, regras de negócio (sem dependências)
└── LedgerAI.Infrastructure  # EF Core, repositórios, JWT, integração com IA
tests/
├── LedgerAI.UnitTests
└── LedgerAI.IntegrationTests
```

Dependências: `API → Application → Domain` e `Infrastructure → Application → Domain`.

## Rodando localmente

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) e Docker.

```bash
docker compose up -d db           # sobe o PostgreSQL
dotnet run --project src/LedgerAI.API
```

A documentação interativa fica em `https://localhost:7xxx/scalar`.

## Roadmap

- [x] Solução, camadas DDD e domínio
- [ ] Persistência (EF Core 10 + PostgreSQL) e migrations
- [ ] Autenticação JWT e endpoints de contas, categorias, transações e orçamentos
- [ ] Categorização automática com `Microsoft.Extensions.AI`
- [ ] Alertas de orçamento via Server-Sent Events
- [ ] Testes unitários e de integração (Testcontainers)
- [ ] Docker + docker-compose + GitHub Actions
- [ ] Servidor MCP
- [ ] .NET Aspire AppHost

## Licença

MIT
