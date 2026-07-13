# ADR 001 - Manter stack atual do projeto

## Status

Aceito

## Contexto

O projeto foi pivotado para GeekTop, mas ja possuia uma base funcional em .NET. A avaliacao tambem considera continuidade, funcionamento e baixo risco de regressao.

## Decisao

Manter a stack atual:

- C#;
- .NET 9;
- ASP.NET Core Minimal API;
- Blazor Server / Razor Components;
- Dapper;
- SQLite;
- xUnit.

## Por que usar

| Escolha | Justificativa |
| --- | --- |
| C# e .NET | Stack principal da solucao e boa integracao entre API, App e testes |
| Minimal API | Simples para rotas diretas e adequada ao tamanho do projeto |
| Blazor Server | Permite criar interface com C# sem adicionar SPA externa |
| Dapper | Mantem SQL claro, leve e parametrizado |
| SQLite | Evita dependencia externa e facilita demonstracao local/Railway com volume |
| xUnit | Testes simples e compativeis com o ecossistema .NET |

## Alternativas consideradas

| Alternativa | Motivo para nao adotar agora |
| --- | --- |
| Entity Framework | Aumentaria mudanca estrutural sem necessidade imediata |
| MariaDB ou SQL Server | Exigiria nova infraestrutura e migracao |
| JWT completo | A entrega atual nao depende de autenticacao distribuida |
| React/Vue | Criaria segunda stack de frontend |

## Consequencias

- Menor risco de quebra perto da entrega.
- Mais facilidade para rodar localmente.
- `Program.cs` pode crescer e deve ser monitorado como divida tecnica.
- SQLite precisa de volume no Railway para persistencia.
