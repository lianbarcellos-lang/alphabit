# Checklist de Entrega - Alphabit Geek / Anime

Documento de conferencia final para apresentacao e entrega da pivotagem.

## Escopo confirmado

- Projeto trabalhado: `alphabit-main`.
- Stack mantida: C#, .NET 9, ASP.NET Core Minimal API, Blazor Server / Razor Components, SQLite, Dapper e xUnit.
- Nao foram adicionados JWT, Swagger obrigatorio, MariaDB, SQL Server, Entity Framework, CQRS, MediatR ou Clean Architecture.
- O `Dockerfile` existe apenas como empacotamento para o Railway; ele nao altera a stack da aplicacao.
- Banco local usado em desenvolvimento: `%LOCALAPPDATA%/Alphabit/Alphabit.db`.

## Funcionalidades para demonstracao

- Vitrine publica de eventos geek/anime com filtros.
- Detalhe do evento com tipos de ingresso, atividades, convidados, mapa de stands e avaliacoes.
- Cadastro e login de cliente, sem recuperacao de senha exposta na interface.
- Compra/reserva com tipo e quantidade de ingresso, cupom e forma de pagamento.
- Historico de reservas com QR Code e avaliacao do evento.
- Painel administrativo para eventos, cidades, categorias, cupons, atividades, convidados e mapa de stands com grades automaticas 3x3/4x4 e ajuste manual.
- Check-in administrativo por QR Code, com bloqueio de duplicidade.
- Dashboard administrativo com receita, reservas, ingressos vendidos, check-ins, capacidade, cupons, eventos populares e avaliacoes.
- Curadoria administrativa de avaliacoes recentes.

## Roteiro rapido de apresentacao

1. Subir a API em `http://localhost:5248`.
2. Subir o Blazor em `http://localhost:5072`.
3. Abrir a vitrine de eventos.
4. Entrar no detalhe de um evento e mostrar convidados, atividades, mapa de stands, tipos de ingresso e avaliacoes.
5. Fazer login ou cadastro de cliente.
6. Simular uma reserva com tipo de ingresso, quantidade, pagamento e cupom opcional.
7. Abrir reservas e mostrar historico, QR Code e avaliacao.
8. Entrar como administrador com login `admin` e senha `admin`.
9. Mostrar dashboard, gestao de eventos, cupons, atividades, convidados, mapa de stands, grades automaticas e check-in.
10. Validar `https://geektop.store/eventos` e o clique nos cards de eventos.
11. Executar build e testes antes da entrega final.

## Comandos de validacao

```powershell
dotnet build .\Alphabit.sln /nr:false -p:BuildInParallel=false -m:1
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```

## Criterios de aceite

- Build sem erros.
- Testes automatizados passando.
- Paginas principais abrem sem erro visual evidente.
- Fluxo principal de evento, mapa de stands com grade automatica/drag-drop, reserva, historico, check-in, dashboard e avaliacao permanece funcional.
- Cards de eventos e o botao `Ver ingressos` abrem o detalhe por link real.
- Login/cadastro nao exibem recuperacao de senha incompleta.
- Documentacao aponta para SQLite e para a stack real do projeto.
- Dados QA locais removidos ou claramente separados antes da apresentacao.
