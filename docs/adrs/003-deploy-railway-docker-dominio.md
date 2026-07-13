# ADR 003 - Deploy no Railway com Docker e dominio proprio

## Status

Aceito

## Contexto

O professor pediu publicacao online e dominio. O projeto usa API e App Blazor, entao era necessario empacotar a execucao de forma simples.

## Decisao

Publicar no Railway usando o `Dockerfile` da raiz, mantendo API e App no mesmo servico. O dominio publico configurado e `https://geektop.store`.

## Por que usar

| Item | Justificativa |
| --- | --- |
| Railway | Plataforma simples para deploy academico e integracao com GitHub |
| Dockerfile raiz | Padroniza build e execucao do App/API |
| Um servico | Reduz configuracao e custo operacional |
| Dominio proprio | Atende requisito de apresentacao e facilita acesso |

## Alternativas consideradas

| Alternativa | Motivo para nao usar agora |
| --- | --- |
| Dois servicos separados | Exigiria coordenar URLs e variaveis entre API e App |
| VPS manual | Mais controle, mas mais operacao |
| Deploy so local | Nao atende pedido de dominio/publicacao |

## Consequencias

- A entrega fica acessivel por dominio.
- O SQLite precisa de volume.
- Logs e variaveis devem ser conferidos no painel Railway.
- Se a escala crescer, separar API e App sera uma nova decisao.
