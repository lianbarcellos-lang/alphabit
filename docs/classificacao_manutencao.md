# Classificacao de Manutencao - GeekTop

## Objetivo

Classificar os tipos de manutencao feitos e planejados no projeto, usando categorias comuns de engenharia de software.

## Categorias

| Categoria | Definicao | Exemplos no GeekTop |
| --- | --- | --- |
| Corretiva | Corrige defeitos encontrados | Ajustes em inscricao de atividades, assentos, QR Code, layout de cards e mapa de stands |
| Adaptativa | Adapta o sistema a ambiente ou requisito novo | Deploy Railway, dominio `geektop.store`, remocao de recuperacao de senha sem provedor real |
| Perfectiva | Melhora usabilidade ou valor funcional | Compra direta no detalhe, destaques em carrossel, busca funcional, organizacao automatica dos stands |
| Preventiva | Reduz risco futuro | Documentacao de ADR, riscos, SLO, tests de regras e validacoes transacionais |

## Registro de Manutencoes Recentes

| Item | Tipo | Motivo | Evidencia |
| --- | --- | --- | --- |
| Mapa de stands com drag/drop | Perfectiva | Melhorar gestao de expositores | Painel ADM e visualizacao do cliente |
| Inscricao em atividades com assento | Perfectiva/Corretiva | Vincular inscricao ao lugar escolhido | Rotas de atividades e UI do detalhe |
| Remocao de recuperar senha | Adaptativa/Preventiva | Evitar fluxo nao funcional no dominio | Login/cadastro sem promessa de e-mail |
| Documentacao de operacao | Preventiva | Atender avaliacao e reduzir risco de entrega | Arquivos em `docs/` |
| Organizacao automatica 3x3/4x4 | Perfectiva | Facilitar posicionamento de stands | Painel ADM |

## Politica

Toda manutencao que altera comportamento deve terminar com:

- build aprovado;
- testes aprovados;
- documentacao atualizada se afetar requisito;
- commit e push no GitHub.
