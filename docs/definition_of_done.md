# Definition of Done - GeekTop

## Objetivo

Definir quando uma tarefa pode ser considerada realmente concluida.

## DoD Geral

Uma tarefa esta pronta quando:

- requisito foi entendido e implementado no escopo combinado;
- codigo compila;
- testes automatizados passam;
- fluxo principal foi validado manualmente quando a mudanca afeta UI;
- documentacao foi atualizada quando a mudanca altera regra, arquitetura ou operacao;
- nao foram adicionadas credenciais reais ao repositorio;
- commit foi feito com mensagem clara;
- GitHub foi atualizado;
- Railway foi validado quando a mudanca afeta producao.

## DoD por Tipo

| Tipo de tarefa | Criterios extras |
| --- | --- |
| Funcionalidade cliente | Deve funcionar logado/deslogado quando aplicavel e ser responsiva |
| Funcionalidade ADM | Deve exigir acesso administrativo e nao quebrar fluxo do cliente |
| API | Deve validar entrada, usar SQL parametrizado e transacao quando necessario |
| Banco | Deve ter migracao/idempotencia no startup ou script documentado |
| Documento | Deve estar em `docs/`, com titulo, objetivo e criterio claro |
| Deploy | Deve abrir no dominio final e passar smoke test |

## Evidencias Aceitas

- Saida de `dotnet build`.
- Saida de `dotnet test`.
- `git status` limpo apos commit.
- Hash local igual ao hash remoto.
- Link funcional do dominio.
