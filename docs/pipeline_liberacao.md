# Pipeline de Liberacao - GeekTop

## Objetivo

Definir o caminho padrao para uma mudanca sair do desenvolvimento local e chegar ao GitHub/Railway sem quebrar a apresentacao.

## Pipeline

| Etapa | Comando/Acao | Criterio de Saida |
| --- | --- | --- |
| 1. Planejar | Confirmar requisito e impacto | Escopo claro |
| 2. Implementar | Alterar codigo/docs no workspace | Mudanca pequena e revisavel |
| 3. Build | `dotnet build .\Alphabit.sln --no-restore /nr:false -m:1` | 0 erros |
| 4. Testes | `dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore /nr:false -m:1` | Todos os testes aprovados |
| 5. Revisao local | Verificar `git diff` e arquivos afetados | Sem alteracao acidental |
| 6. Commit | `git add` + `git commit` | Commit com mensagem objetiva |
| 7. Push GitHub | `git push origin main` e remoto secundario quando aplicavel | Remotes alinhados |
| 8. Deploy Railway | Railway redeploy pela integracao GitHub ou redeploy manual | Build online concluido |
| 9. Smoke test | Abrir `https://geektop.store/eventos` e fluxo principal | Site funcional |

## Gates Obrigatorios

| Gate | Bloqueia liberacao se |
| --- | --- |
| Build | houver erro de compilacao |
| Testes | algum teste falhar |
| Documentacao | requisito ou arquitetura mudar sem docs |
| Seguranca | credencial real aparecer em arquivo versionado |
| Operacao | Railway sem variaveis/volume necessario |

## Rollback

Se o deploy quebrar:

1. identificar ultimo commit estavel;
2. reverter a mudanca com novo commit;
3. fazer push;
4. acionar novo deploy no Railway;
5. validar `/eventos`, login, carrinho e painel ADM.
