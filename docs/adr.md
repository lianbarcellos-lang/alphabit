# ADR - Persistencia com Minimal API, SQLite e Dapper

## Contexto

O projeto Alphabit precisava de um backend simples de executar, com baixo custo de infraestrutura, sem uso de ORM completo e aderente ao enunciado da disciplina. A aplicação também precisava usar rotas HTTP explícitas, script SQL manual e consultas protegidas contra SQL Injection.

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

## ADR complementar - Mapa de stands e expositores

### Contexto

Depois da pivotagem para GeekTop, o evento passou a precisar de informacoes internas alem do ingresso: expositores, lojas, arenas e atrações posicionadas no espaço do evento.

### Decisão

Foi adicionada a tabela `StandsEspacos`, vinculada a `Eventos`, com linhas/setores, codigo do stand, posição no mapa, dados do ocupante e dados comerciais. O evento também armazena a imagem da planta enviada pelo administrador em `MapaImagemUrl`. A alocacao e feita no painel administrativo e a visualizacao e publica no detalhe do evento.

O mapa passa a aceitar uma planta enviada manualmente pelo administrador. Os stands são posicionados por drag/drop sobre a imagem usando coordenadas percentuais, sem detecção automática por imagem. Para acelerar a montagem inicial, o painel administrativo também oferece organização automática por grades 2x2, 3x3, 4x4, 5x5 e 8x8, aplicando posições proporcionais sem bloquear ajustes manuais posteriores.

### Consequências

Prós:
- separa ingresso geral de organizacao interna do evento;
- permite demonstrar empresas e atrações dentro do mapa;
- reduz trabalho manual inicial quando existem muitos stands;
- mantém SQLite, Dapper e Minimal API.

Contras:
- o mapa com imagem ainda depende do administrador conferir e ajustar o layout final;
- layouts muito especificos podem exigir editor visual mais avançado no futuro.
