# ADR - Decisoes Arquiteturais do GeekTop

Este documento registra as principais decisoes tecnicas do projeto GeekTop. O objetivo e deixar claro por que certas escolhas foram feitas, quais alternativas foram evitadas e quais consequencias elas trazem para manutencao, apresentacao e deploy.

## Resumo das escolhas e justificativas

| Escolha | Por que foi usada |
| --- | --- |
| C# e .NET 9 | Mantem o projeto dentro da stack exigida, oferece boa produtividade e integra API, frontend e testes na mesma solucao. |
| ASP.NET Core Minimal API | Permite criar rotas HTTP diretas, simples de apresentar e adequadas ao tamanho academico do projeto. |
| Blazor Server / Razor Components | Permite construir a interface em C# com componentes reaproveitaveis e integracao direta com servicos do app. |
| SQLite | Facilita execucao local, testes e apresentacao sem depender de servidor externo de banco. |
| Dapper | Da controle sobre SQL, usa parametros contra SQL Injection e evita a complexidade de um ORM completo. |
| xUnit | Garante testes automatizados para regras principais e reduz risco de regressao durante as pivotagens. |
| JavaScript interop | Foi usado apenas onde Blazor sozinho nao era suficiente, como camera para QR Code e drag/drop no mapa. |
| Railway | Foi escolhido como opcao simples de deploy para apresentacao, com baixo atrito para publicar a aplicacao. O Dockerfile e apenas empacotamento do deploy. |
| Links reais nos cards | Foram usados para que a navegacao dos eventos continue funcional mesmo se o circuito Blazor reconectar ou atrasar. |

## ADR 001 - Persistencia com Minimal API, SQLite e Dapper

### Contexto

O projeto precisava de um backend simples de executar, com baixo custo de infraestrutura, sem uso de ORM completo e aderente ao enunciado da disciplina. A aplicacao tambem precisava usar rotas HTTP explicitas, script SQL manual e consultas protegidas contra SQL Injection.

### Por que usamos

- Minimal API: porque o projeto precisa de endpoints claros, diretos e faceis de demonstrar.
- SQLite: porque reduz custo e complexidade para rodar localmente e em apresentacao.
- Dapper: porque atende ao requisito de acesso a dados com SQL parametrizado sem trazer Entity Framework.
- SQL manual: porque deixa explicito o modelo de dados e facilita explicar tabelas, indices e consultas.

### Decisao

Foi adotada uma arquitetura baseada em Minimal API com .NET 9, SQLite para armazenamento local e Dapper para o acesso a dados. O banco é inicializado com script SQL manual e as consultas são executadas com parâmetros nomeados.

### Consequencias

Prós:
- implementação direta e rápida para o escopo acadêmico
- aderência ao requisito de uso de Dapper com parâmetros
- facilidade de execução local sem dependência de servidor de banco externo
- menor complexidade de infraestrutura para testes e apresentação

Contras:
- crescimento da aplicação pode exigir maior separação em camadas no futuro
- SQLite não representa todos os cenários de concorrência de um banco corporativo
- o uso de Minimal API em um único arquivo aumenta o risco de concentração de responsabilidades

## ADR 002 - Pivotagem para plataforma de eventos geek

### Contexto

O projeto deixou de ser apenas uma base generica de ingressos e passou a representar uma plataforma de eventos geek, com foco em anime, games, cosplay, cultura pop, card games, convidados, atividades internas e stands.

### Decisao

Manter a stack existente e pivotar dominio, textos, imagens, fluxos e documentacao para GeekTop. A mudanca preservou a estrutura tecnica do projeto e concentrou a evolucao na experiencia do cliente e do administrador.

### Por que usamos essa abordagem

A pivotagem por adaptacao foi escolhida porque trocar stack, banco ou arquitetura ao mesmo tempo aumentaria muito o risco. O objetivo era mudar o dominio do produto sem perder o que ja estava funcional.

### Consequencias

Prós:
- reduz risco tecnico porque reaproveita a base existente;
- torna o projeto mais coerente e demonstravel;
- aproxima as funcionalidades de um cenario real de evento geek.

Contras:
- alguns nomes internos ainda podem carregar historico do projeto original;
- exige revisao constante de textos para evitar mistura de identidades.

## ADR 003 - Compra com entrada geral sem assento marcado

### Contexto

Eventos geek costumam ter ingresso principal de entrada geral e atividades internas separadas, como workshop, torneio, meet and greet, painel e competicao.

### Decisao

O ingresso principal do evento foi mantido como entrada geral sem assento marcado. A selecao de lugares foi aplicada nas atividades internas, onde a capacidade e menor e a escolha de assento faz mais sentido.

### Por que usamos essa abordagem

Eventos geek grandes normalmente funcionam com entrada geral. Reservar assento no ingresso principal criaria complexidade desnecessaria e confundiria o cliente. A selecao de lugar foi mantida onde agrega valor: atividades com vagas limitadas.

### Consequencias

Prós:
- fluxo de compra fica mais simples;
- evita reserva de milhares de assentos para eventos de entrada geral;
- permite controlar vagas limitadas nas atividades internas.

Contras:
- se no futuro houver evento com assento numerado real, sera necessario criar regra especifica para esse tipo de evento.

## ADR 004 - Programacao interna com capacidade e assentos dinamicos

### Contexto

O painel administrativo precisava permitir cadastro de atividades do evento com nome, tipo, dia, horario, descricao e capacidade. No cliente, a inscricao deveria reservar um lugar especifico dentro da atividade.

### Decisao

As atividades usam a capacidade cadastrada pelo administrador para gerar dinamicamente a lista de lugares disponiveis. A inscricao salva os assentos escolhidos e impede duplicidade para o mesmo lugar.

### Por que usamos essa abordagem

Gerar lugares pela capacidade evita cadastrar cadeiras manualmente e mantem a regra simples: se o administrador define 20 vagas, o sistema oferece 20 lugares. Isso atende ao fluxo sem criar uma tabela separada de assentos neste momento.

### Consequencias

Prós:
- o administrador controla a lotacao de cada atividade;
- o cliente escolhe lugar no momento da inscricao;
- nao e necessario cadastrar manualmente cada assento no banco.

Contras:
- layouts de cadeira mais complexos ainda dependem de uma evolucao futura;
- a numeracao segue uma grade simples gerada pela capacidade.

## ADR 005 - Mapa de stands e expositores

### Contexto

Depois da pivotagem para GeekTop, o evento passou a precisar de informacoes internas alem do ingresso: expositores, lojas, arenas e atracoes posicionadas no espaco do evento.

### Decisao

Foi adicionada a tabela `StandsEspacos`, vinculada a `Eventos`, com linhas/setores, codigo do stand, posicao no mapa, dados do ocupante e dados comerciais. O evento tambem armazena a imagem da planta enviada pelo administrador em `MapaImagemUrl`. A alocacao e feita no painel administrativo e a visualizacao e publica no detalhe do evento.

O mapa aceita uma planta enviada manualmente pelo administrador. Os stands sao posicionados por drag/drop sobre a imagem usando coordenadas percentuais, sem deteccao automatica por imagem. Para acelerar a montagem inicial, o painel administrativo oferece organizacao automatica por grades compactas e centralizadas, sem bloquear ajustes manuais posteriores. Na interface atual, o administrador usa as opcoes visiveis de 3x3 e 4x4.

### Por que usamos essa abordagem

Foi evitada deteccao automatica por imagem porque ela poderia falhar com plantas diferentes, baixa resolucao, linhas de arquitetura e textos. O drag/drop com coordenadas percentuais e mais previsivel, funciona em telas diferentes e ainda permite que o administrador ajuste manualmente.

### Consequencias

Prós:
- separa ingresso geral de organizacao interna do evento;
- permite demonstrar empresas e atracoes dentro do mapa;
- reduz trabalho manual inicial quando existem muitos stands;
- mantém SQLite, Dapper e Minimal API.

Contras:
- o mapa com imagem ainda depende do administrador conferir e ajustar o layout final;
- layouts muito especificos podem exigir editor visual mais avançado no futuro.

## ADR 006 - QR Code e check-in administrativo

### Contexto

Cada reserva precisa ter um codigo de entrada unico para validar a presenca no evento e impedir uso duplicado.

### Decisao

Cada reserva gera um codigo unico em `Checkins`. O cliente visualiza o QR Code nas reservas e o administrador pode validar por digitacao manual ou camera/webcam.

### Por que usamos essa abordagem

QR Code melhora a entrada do evento, mas a digitacao manual continua necessaria porque nem todo navegador ou dispositivo libera camera. Assim o fluxo funciona tanto em ambiente ideal quanto em ambiente limitado.

### Consequencias

Prós:
- melhora a experiencia de entrada;
- impede check-in duplicado;
- mantem alternativa manual caso a camera do navegador nao esteja disponivel.

Contras:
- leitura por camera depende de suporte do navegador e permissao do dispositivo;
- em deploy real, HTTPS e necessario para liberar camera em navegadores modernos.

## ADR 007 - Manutencao de performance incremental

### Contexto

Com o crescimento de funcionalidades, algumas partes passaram a ter risco de lentidao: dashboard com muitas compras, filtros recalculados no Blazor, imagens grandes em base64, chamadas JS repetidas e falta de indices especificos.

### Decisao

Aplicar manutencao incremental sem trocar a arquitetura principal. Foram priorizados cache de filtros, limite/paginacao no dashboard, indices SQLite idempotentes, validacao de imagens e controle do ciclo de vida do mapa de stands.

### Por que usamos essa abordagem

O projeto ja estava funcional. Uma refatoracao grande poderia criar regressao perto da entrega. Por isso, as melhorias foram pequenas, testaveis e focadas nos pontos que realmente causavam lentidao: consultas grandes, renderizacoes repetidas, imagens pesadas e chamadas JS duplicadas.

### Consequencias

Prós:
- reduz travamentos sem reescrever o projeto;
- melhora compatibilidade com Railway;
- mantem rotas e fluxos existentes.

Contras:
- uma separacao completa em camadas ainda pode ser desejavel no futuro;
- carregamento preguiçoso total de abas do admin ficou como melhoria posterior.

## ADR 008 - Deploy Railway em servico unico

### Contexto

O Railway precisava hospedar API e App com o minimo de configuracao para apresentacao. A alternativa de dois servicos exigia URL publica da API, mais variaveis e mais pontos de falha.

### Decisao

Usar um unico servico Docker no Railway. O container inicia a API internamente em `8081` e o App Blazor publicamente na porta `$PORT`. O dominio `https://geektop.store` aponta para esse servico.

### Por que usamos essa abordagem

Ela simplifica o deploy, reduz custo operacional e evita problema de comunicacao entre dois servicos. O Dockerfile nao altera a stack do projeto; ele apenas empacota a execucao da API e do App para o Railway.

### Consequencias

Prós:
- menos variaveis para configurar;
- menor chance de erro na URL da API;
- deploy mais simples para apresentacao.

Contras:
- API e App escalam juntos;
- para producao maior, separar servicos pode voltar a ser interessante.

## ADR 009 - Remocao da recuperacao de senha da interface

### Contexto

O sistema tinha referencias a recuperacao de senha, mas o fluxo de email nao estava configurado e validado no dominio de producao.

### Decisao

Remover a recuperacao de senha das telas e da documentacao de funcionalidades entregues.

### Por que usamos essa abordagem

Uma funcionalidade incompleta poderia prejudicar a apresentacao. E melhor entregar login/cadastro consistentes do que exibir uma opcao que depende de infraestrutura externa nao configurada.

### Consequencias

Prós:
- evita fluxo quebrado para o usuario;
- reduz risco na avaliacao;
- mantem a demonstracao objetiva.

Contras:
- recuperacao de senha fica como melhoria futura, dependente de provedor de email e validacao ponta a ponta.

## ADR 010 - Navegacao por cards com links reais

### Contexto

Os cards de eventos dependiam apenas de clique Blazor. Em producao, eventuais atrasos/reconexoes poderiam deixar o card parecendo clicavel sem navegar.

### Decisao

Transformar cards de eventos e destaques em links reais para `/eventos/{id}`, mantendo o botao visual `Ver ingressos`.

### Por que usamos essa abordagem

Links reais sao mais robustos, funcionam com clique comum, abrir em nova aba e carregamento normal do navegador. Isso tambem melhora acessibilidade e reduz dependencia do estado do circuito Blazor.

### Consequencias

Prós:
- navegacao mais previsivel;
- melhora uso em producao;
- reduz chance de travamento percebido pelo cliente.

Contras:
- exige cuidado para manter o estilo visual de card sem parecer link cru.
