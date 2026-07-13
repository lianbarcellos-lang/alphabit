# Plano de Iteracao - GeekTop

## Objetivo

Organizar as proximas entregas em ciclos curtos, mantendo build, testes e documentacao em dia.

## Iteracao Atual - Entrega Academica

| Item | Objetivo | Resultado esperado | Status |
| --- | --- | --- | --- |
| I1 | Consolidar documentacao da rubrica | Arquivos em `docs/` com nomes e estruturas cobradas | Em andamento |
| I2 | Validar testes e build | Build e xUnit verdes | Planejado |
| I3 | Publicar GitHub | `origin/main` e remoto secundario atualizados | Planejado |
| I4 | Validar Railway | Dominio funcionando com fluxo principal | Planejado |

## Proximas Iteracoes

| Iteracao | Foco | Entregas |
| --- | --- | --- |
| 1 | Estabilidade | Refino de testes de integracao, revisao de logs e volume SQLite |
| 2 | Arquitetura | Quebrar `Program.cs` por modulos sem trocar stack |
| 3 | Seguranca | Autenticacao admin mais robusta e politica de senha |
| 4 | Operacao | Observabilidade basica e checklist de deploy |

## Criterio de Conclusao

Uma iteracao termina apenas quando:

- tarefas concluidas estao no GitHub;
- build passa;
- testes passam;
- documentos afetados foram atualizados;
- riscos novos foram registrados.
