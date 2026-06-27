# Budget - Custos e Recursos do Projeto

## Objetivo

Este documento registra a estimativa de custos e recursos para manter o GeekTop funcionando em ambiente local, apresentacao academica e deploy simples.

## Premissas

- O projeto usa SQLite, entao nao exige banco externo pago.
- O frontend e backend rodam em .NET.
- O deploy de apresentacao esta em Railway.
- O projeto nao depende de servicos pagos obrigatorios.
- Imagens podem usar URLs externas ou uploads pequenos em base64.

## Custos locais

Para rodar localmente:

- .NET SDK instalado.
- Navegador moderno.
- Espaco local para o banco SQLite.

Custo financeiro direto: R$ 0,00.

## Custos de deploy

Em Railway, o custo depende do plano e do uso. Para apresentacao academica e testes leves, o objetivo e manter consumo baixo:

- um servico Docker no Railway executando API e App .NET;
- banco SQLite em arquivo local do container;
- sem servico externo de banco;
- sem storage dedicado para imagens.

Observacao importante: SQLite em ambiente de deploy pode perder dados se o volume nao for persistente. Para apresentacao isso pode ser aceitavel, mas para producao real seria necessario configurar volume persistente ou migrar para banco gerenciado.

Dominio atual de apresentacao: `https://geektop.store`.

## Recursos que mais consomem

- Imagens grandes em base64.
- Dashboard com muitas compras.
- Recalculo de listas no Blazor.
- Chamadas JS repetidas no mapa.
- Consultas sem indice em tabelas de reservas, check-ins e avaliacoes.

## Medidas adotadas para reduzir custo/performance

- Limite de imagem em novos uploads.
- Dashboard com limite de compras.
- Totais calculados no SQL.
- Indices SQLite idempotentes.
- Cache de filtros na pagina de eventos.
- Controle do ciclo de vida do mapa de stands.

## Melhorias futuras

- Usar storage externo para imagens.
- Usar banco persistente gerenciado se o projeto virar producao.
- Separar API e frontend em servicos independentes se o volume crescer.
- Adicionar monitoramento de logs e metricas no deploy.
