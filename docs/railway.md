# Deploy no Railway

Este projeto tem duas aplicacoes e deve ser publicado como dois servicos no Railway:

- `GeekTop API`: backend Minimal API em `src/Alphabit.API`
- `GeekTop App`: frontend Blazor em `src/Alphabit.App`

## Servico 1: API

Configure um servico apontando para o mesmo repositorio e use:

- Build command: `dotnet publish src/Alphabit.API/Alphabit.API.csproj -c Release -o out`
- Start command: `dotnet out/Alphabit.API.dll`

Variaveis obrigatorias:

```env
AdminAccess__Token=troque-por-um-token-forte
AdminAccess__Login=admin
AdminAccess__Password=troque-por-uma-senha-forte
```

Variaveis opcionais de email:

```env
EmailSettings__SmtpHost=
EmailSettings__SmtpPort=587
EmailSettings__SenderEmail=
EmailSettings__AppPassword=
EmailSettings__SenderName=GeekTop
```

Para manter o banco SQLite depois de redeploy/restart, crie um Volume no servico da API. O codigo ja usa `RAILWAY_VOLUME_MOUNT_PATH` automaticamente quando o volume existe.

## Servico 2: App

Configure outro servico apontando para o mesmo repositorio e use:

- Build command: `dotnet publish src/Alphabit.App/Alphabit.App.csproj -c Release -o out`
- Start command: `dotnet out/Alphabit.App.dll`

Variavel obrigatoria:

```env
AlphabitApi__BaseUrl=https://URL-DA-API.up.railway.app/
```

Depois que a API estiver publicada, copie a URL publica da API e coloque nessa variavel do App.

## Observacoes importantes

- O Railway injeta a variavel `PORT` em runtime. A API e o App ja estao preparados para escutar em `0.0.0.0:$PORT`.
- Sem volume na API, o projeto pode funcionar, mas o banco SQLite pode ser perdido em redeploy/restart.
- A compra, login, carrinho, QR Code, check-in, dashboard e mapa de stands dependem do App conseguir chamar a API pela variavel `AlphabitApi__BaseUrl`.

## Validacao antes de subir

Rode localmente:

```powershell
dotnet build .\Alphabit.sln -c Release
dotnet test .\tests\Alphabit.Tests\Alphabit.Tests.csproj --no-restore
```
