# ADR 002 - Usar Dapper com SQLite

## Status

Aceito

## Contexto

O GeekTop precisa persistir usuarios, eventos, reservas, cupons, atividades, convidados, stands, check-ins e avaliacoes. A equipe precisava de uma solucao leve, previsivel e simples para apresentar.

## Decisao

Usar SQLite como banco principal e Dapper como ferramenta de acesso a dados.

## Por que usar

| Item | Justificativa |
| --- | --- |
| SQLite | Banco local simples, sem servidor separado, bom para demonstracao academica |
| Dapper | Permite SQL explicito e parametrizado com pouco overhead |
| Transacoes manuais | Dão controle nos fluxos de checkout, check-in e exclusao |
| Migracoes idempotentes no startup | Facilitam evoluir tabelas sem ferramenta externa |

## Alternativas consideradas

| Alternativa | Motivo para adiar |
| --- | --- |
| SQL Server | Mais robusto, mas exige infraestrutura e configuracao extra |
| MariaDB | Bom para producao, mas nao era necessario para a entrega atual |
| Entity Framework | Melhor para dominios maiores, mas aumentaria refatoracao |

## Consequencias

- O projeto continua facil de clonar, rodar e demonstrar.
- O banco deve ficar em volume persistente no Railway.
- Consultas precisam continuar parametrizadas.
- Em uma evolucao real, uma migracao para banco cliente-servidor pode virar ADR novo.
