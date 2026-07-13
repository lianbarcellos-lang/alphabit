# Divida Tecnica - GeekTop

## Objetivo

Listar as dividas tecnicas conhecidas, sua causa, impacto e mitigacao atual. A existencia de divida nao significa falha imediata; significa trabalho consciente para evolucao segura.

| ID | Divida | Local | Causa | Impacto | Classificacao | Mitigacao atual | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| DT-01 | `Program.cs` grande | `src/Alphabit.API/Program.cs` | Minimal API cresceu com muitas funcionalidades | Dificulta navegacao e manutencao | Manutencao preventiva | Testes de risco e documentacao arquitetural | Aberta |
| DT-02 | Testes arquiteturais por leitura textual | `tests/Alphabit.Tests/AlphabitRiskTests.cs` | Garantir regras sem criar infra de integracao | Pode gerar falsos positivos/negativos por texto | Manutencao perfectiva | Manter nomes estaveis e complementar com testes de regra | Aberta |
| DT-03 | SQLite em ambiente publicado | Railway | Simplicidade academica | Menor concorrencia e exige volume | Manutencao adaptativa | Volume persistente e transacoes | Monitorada |
| DT-04 | Autenticacao admin simplificada | API/App | Escopo academico sem Identity completo | Menos robusta que provedor dedicado | Seguranca evolutiva | Token/configuracao e validacao admin | Monitorada |
| DT-05 | App e API no mesmo servico | Railway | Reduzir configuracao | Menor isolamento operacional | Operacional | Dockerfile e smoke tests | Aceita |
| DT-06 | Mapa de stands usa coordenadas percentuais manuais | App/Admin | Evitar complexidade de visao computacional | Requer ajuste manual do admin | UX/Manutencao | Grade automatica compacta e drag/drop | Aceita |

## Criterio de Remocao

Uma divida e considerada removida quando:

1. existe alteracao implementada;
2. os testes passam;
3. a documentacao relacionada e atualizada;
4. a mudanca e publicada no GitHub;
5. se afetar producao, o Railway e validado.
