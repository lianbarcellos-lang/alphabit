# Roadmap - GeekTop

Este roadmap resume as especificacoes do projeto na ordem recomendada de execucao, considerando dependencias entre funcionalidades e o estado atual da implementacao.

Legenda:

- Construido: a estrutura da funcionalidade ja foi criada no projeto.
- Implementado: a funcionalidade ja esta funcionando na aplicacao.

| Ordem | Spec | Dependencias | Construido | Implementado | Status |
| --- | --- | --- | --- | --- | --- |
| 1 | Rebranding para GeekTop | Nenhuma | Sim | Sim | Concluido |
| 2 | Vitrine de eventos geek | Rebranding | Sim | Sim | Concluido |
| 3 | Cadastro/login de cliente | Base de usuarios | Sim | Sim | Concluido |
| 4 | Login administrativo | Base de usuarios | Sim | Sim | Concluido |
| 5 | Gestao administrativa de eventos | Login admin | Sim | Sim | Concluido |
| 6 | Catalogo de cidades | Gestao de eventos | Sim | Sim | Concluido |
| 7 | Catalogo de categorias geek | Gestao de eventos | Sim | Sim | Concluido |
| 8 | Imagens/capas de eventos geek | Vitrine e eventos | Sim | Sim | Concluido |
| 9 | Filtros de eventos | Vitrine, cidades e categorias | Sim | Sim | Concluido |
| 10 | Detalhe do evento | Vitrine | Sim | Sim | Concluido |
| 11 | Tipos de ingresso | Eventos | Sim | Sim | Concluido |
| 12 | Selecao de assentos | Eventos e reservas | Sim | Sim | Concluido |
| 13 | Carrinho | Eventos, assentos e tipos de ingresso | Sim | Sim | Concluido |
| 14 | Cupons de desconto | Carrinho | Sim | Sim | Concluido |
| 15 | Finalizacao de reserva | Usuario, evento, carrinho e cupom | Sim | Sim | Concluido |
| 16 | Historico de reservas | Reservas | Sim | Sim | Concluido |
| 17 | Atividades internas | Eventos | Sim | Sim | Concluido |
| 18 | Inscricao em atividades | Atividades e usuarios | Sim | Sim | Concluido |
| 19 | Convidados | Eventos | Sim | Sim | Concluido |
| 20 | Associacao de convidados a eventos | Convidados e eventos | Sim | Sim | Concluido |
| 21 | QR Code da reserva | Reservas | Sim | Sim | Concluido |
| 22 | Check-in manual | QR Code e admin | Sim | Sim | Concluido |
| 23 | Check-in com camera/webcam | QR Code e admin | Sim | Sim | Concluido |
| 24 | Dashboard/relatorio administrativo | Reservas, cupons e eventos | Sim | Sim | Concluido |
| 25 | Avaliacoes pos-evento | Reservas e usuarios | Sim | Sim | Concluido |
| 26 | Media de avaliacoes no evento | Avaliacoes | Sim | Sim | Concluido |
| 27 | Moderacao de avaliacoes | Avaliacoes e admin | Sim | Sim | Concluido |
| 28 | Correcao visual e textos em portugues | Telas implementadas | Sim | Sim | Concluido |
| 29 | Documentacao final | Projeto implementado | Sim | Em andamento | Em andamento |
| 30 | Limpeza final de dados de QA | Testes manuais locais | Parcial | Parcial | Opcional |

## Ordem recomendada para futuras melhorias

1. Revisar documentacao final e manter `docs` atualizada.
2. Fazer uma passada visual completa em Home, Eventos, Detalhe, Reservas e Admin.
3. Limpar dados locais de teste antes de apresentacao, se necessario.
4. Adicionar campos literais opcionais do PDF, caso sejam exigidos:
   - `FaixaEtaria`;
   - `Organizadora`.
5. Criar aliases publicos opcionais para categorias, caso a banca exija nomes literais:
   - `GET /api/categorias`;
   - `POST /api/categorias`.

## Regra de conclusao por fase

Cada fase so deve ser considerada concluida quando:

- o projeto compila;
- os testes automatizados passam;
- o fluxo principal continua funcionando;
- a documentacao e atualizada quando houver mudanca relevante.

Comandos de verificacao:

```powershell
dotnet build .\Alphabit.sln
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

