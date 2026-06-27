# Deploy no Railway

Este documento descreve o deploy atual do GeekTop no Railway.

## Modelo atual

O projeto esta publicado como um unico servico Docker no Railway.

- O `Dockerfile` da raiz publica a API e o App.
- O script `railway-start.sh` inicia os dois processos.
- A API roda internamente em `http://127.0.0.1:8081`.
- O App Blazor fica exposto na porta publica definida pelo Railway em `$PORT`.
- O App usa `AlphabitApi__BaseUrl=http://127.0.0.1:8081/` para chamar a API dentro do mesmo container.

Essa escolha reduz configuracao para apresentacao, evita dois servicos separados e mantem a stack da aplicacao igual: .NET, Minimal API, Blazor Server, SQLite, Dapper e xUnit.

## Variaveis obrigatorias

Configure no servico Railway:

```env
AdminAccess__Token=troque-por-um-token-forte
AdminAccess__Login=admin
AdminAccess__Password=troque-por-uma-senha-forte
```

## Banco SQLite persistente

Para nao perder o banco em redeploy/restart, adicione um volume no servico Railway.

A API detecta `RAILWAY_VOLUME_MOUNT_PATH` automaticamente quando o volume existe e cria o SQLite nesse caminho. Sem volume, o sistema pode funcionar para demonstracao, mas os dados podem ser perdidos quando o container reiniciar.

## Dominio

Dominio de apresentacao configurado:

- `https://geektop.store`

No Railway, o dominio foi associado ao servico do App. Na Hostinger, os registros DNS apontam o dominio para o Railway conforme as instrucoes do painel do Railway.

## Validacao em producao

Apos cada deploy, validar:

- abrir `https://geektop.store/eventos`;
- clicar em um card de evento e confirmar que abre `/eventos/{id}`;
- confirmar que o botao `Ver ingressos` tambem abre o detalhe;
- abrir login e cadastro;
- confirmar que nao existe fluxo de recuperacao de senha exposto;
- abrir `/admin`;
- validar que a pagina carrega com CSS e imagens.

## Validacao local antes do deploy

Rode na raiz do projeto:

```powershell
dotnet build .\Alphabit.sln --no-restore /nr:false -m:1
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore /nr:false -m:1
```

## Observacoes importantes

- O `Dockerfile` e apenas empacotamento de deploy; ele nao muda a arquitetura da aplicacao.
- O projeto nao depende de email em producao nesta entrega.
- O fluxo de recuperacao de senha foi removido da interface para evitar demonstrar uma funcionalidade incompleta.
- A camera do QR Code exige HTTPS, por isso funciona melhor no dominio publicado do que em HTTP local.
