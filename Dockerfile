FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Alphabit.sln ./
COPY src/Alphabit.API/Alphabit.API.csproj src/Alphabit.API/
COPY src/Alphabit.App/Alphabit.App.csproj src/Alphabit.App/
COPY tests/Alphabit.Tests/Alphabit.Tests.csproj tests/Alphabit.Tests/
RUN dotnet restore Alphabit.sln

COPY . ./
RUN dotnet publish src/Alphabit.API/Alphabit.API.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/Alphabit.App/Alphabit.App.csproj -c Release -o /app/web --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/api ./api
COPY --from=build /app/web ./web
COPY railway-start.sh ./railway-start.sh

RUN chmod +x /app/railway-start.sh

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV AlphabitApi__BaseUrl=http://127.0.0.1:8081/

EXPOSE 8080

ENTRYPOINT ["/app/railway-start.sh"]
