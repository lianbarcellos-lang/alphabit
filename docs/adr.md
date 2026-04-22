# ADR - Persistencia com Minimal API, SQLite e Dapper

## Contexto

O projeto TicketPrime precisava de um backend simples de executar, com baixo custo de infraestrutura, sem uso de ORM completo e aderente ao enunciado da disciplina. A aplicação também precisava usar rotas HTTP explícitas, script SQL manual e consultas protegidas contra SQL Injection.

## Decisão

Foi adotada uma arquitetura baseada em Minimal API com .NET 9, SQLite para armazenamento local e Dapper para o acesso a dados. O banco é inicializado com script SQL manual e as consultas são executadas com parâmetros nomeados.

## Consequências

Prós:
- implementação direta e rápida para o escopo acadêmico
- aderência ao requisito de uso de Dapper com parâmetros
- facilidade de execução local sem dependência de servidor de banco externo
- menor complexidade de infraestrutura para testes e apresentação

Contras:
- crescimento da aplicação pode exigir maior separação em camadas no futuro
- SQLite não representa todos os cenários de concorrência de um banco corporativo
- o uso de Minimal API em um único arquivo aumenta o risco de concentração de responsabilidades
