using Dapper;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Alphabit.API;
using Alphabit.API.modelos;

var builder = WebApplication.CreateBuilder(args);
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

builder.Services.AddHttpClient();
var app = builder.Build();

var adminToken = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Token");
var adminLogin = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Login");
var adminPassword = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Password");
var smtpHost = builder.Configuration["EmailSettings:SmtpHost"];
var smtpPort = int.TryParse(builder.Configuration["EmailSettings:SmtpPort"], out var parsedSmtpPort) ? parsedSmtpPort : 587;
var smtpUser = builder.Configuration["EmailSettings:SenderEmail"];
var smtpPassword = builder.Configuration["EmailSettings:AppPassword"];
var smtpDisplayName = builder.Configuration["EmailSettings:SenderName"] ?? "GeekTop";
var emailApiBaseUrl = builder.Configuration["EmailApiSettings:BaseUrl"];
var emailApiKey = builder.Configuration["EmailApiSettings:ApiKey"];
var emailApiSenderEmail = builder.Configuration["EmailApiSettings:SenderEmail"];
var emailApiSenderName = builder.Configuration["EmailApiSettings:SenderName"] ?? "GeekTop";

var legacyDbPath = Path.Combine(builder.Environment.ContentRootPath, "db", "Alphabit.db");
var configuredDbDirectory = builder.Configuration["DatabaseSettings:Directory"];
var railwayVolumeMountPath = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
var runtimeDbDirectory = !string.IsNullOrWhiteSpace(configuredDbDirectory)
    ? configuredDbDirectory
    : !string.IsNullOrWhiteSpace(railwayVolumeMountPath)
        ? Path.Combine(railwayVolumeMountPath, "alphabit-data")
        : Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Alphabit");
Directory.CreateDirectory(runtimeDbDirectory);

var dbPath = Path.Combine(runtimeDbDirectory, "Alphabit.db");
if (!File.Exists(dbPath) && File.Exists(legacyDbPath))
{
    File.Copy(legacyDbPath, dbPath);
}

var staleJournalPath = $"{dbPath}-journal";
if (File.Exists(staleJournalPath))
{
    try
    {
        File.Delete(staleJournalPath);
    }
    catch
    {
        // If the journal is still in use, SQLite will recreate/manage it on demand.
    }
}

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,
    ForeignKeys = true
}.ToString();

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Usuarios (
            Cpf TEXT PRIMARY KEY,
            Nome TEXT NOT NULL,
            Email TEXT NOT NULL,
            SenhaHash TEXT NOT NULL DEFAULT '',
            Sobrenome TEXT NOT NULL DEFAULT '',
            PaisResidencia TEXT NOT NULL DEFAULT 'Brasil',
            TipoDocumento TEXT NOT NULL DEFAULT 'CPF',
            CodigoPais TEXT NOT NULL DEFAULT '+55',
            Telefone TEXT NOT NULL DEFAULT '',
            DataNascimento TEXT NULL,
            Sexo TEXT NOT NULL DEFAULT ''
        );

        CREATE TABLE IF NOT EXISTS Eventos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL,
            LocalEvento TEXT NOT NULL DEFAULT '',
            CidadeEvento TEXT NOT NULL DEFAULT '',
            Artista TEXT NOT NULL DEFAULT '',
            GeneroMusical TEXT NOT NULL DEFAULT '',
            CapacidadeTotal INTEGER NOT NULL,
            DataEvento TEXT NOT NULL,
            PrecoPadrao REAL NOT NULL,
            ImagemUrl TEXT,
            MapaImagemUrl TEXT NOT NULL DEFAULT '',
            EhDestaque INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS Cupons (
            Codigo TEXT PRIMARY KEY,
            PorcentagemDesconto REAL NOT NULL,
            ValorMinimoRegra REAL NOT NULL
        );

        CREATE TABLE IF NOT EXISTS TiposIngresso (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EventoId INTEGER NOT NULL,
            Nome TEXT NOT NULL,
            Beneficios TEXT NOT NULL DEFAULT '',
            Preco REAL NOT NULL,
            QuantidadeDisponivel INTEGER NOT NULL,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS GenerosMusicais (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL COLLATE NOCASE UNIQUE
        );

        CREATE TABLE IF NOT EXISTS CidadesEventos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL COLLATE NOCASE UNIQUE
        );

        CREATE TABLE IF NOT EXISTS Atividades (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EventoId INTEGER NOT NULL,
            Nome TEXT NOT NULL,
            Horario TEXT NOT NULL,
            HorarioFim TEXT NOT NULL DEFAULT '',
            Tipo TEXT NOT NULL,
            Descricao TEXT NOT NULL DEFAULT '',
            LimiteParticipantes INTEGER NOT NULL,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS InscricoesAtividades (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AtividadeId INTEGER NOT NULL,
            UsuarioCpf TEXT NOT NULL,
            Quantidade INTEGER NOT NULL DEFAULT 1,
            Assentos TEXT NOT NULL DEFAULT '',
            CriadoEm TEXT NOT NULL,
            FOREIGN KEY (AtividadeId) REFERENCES Atividades(Id) ON DELETE CASCADE,
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Convidados (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL,
            Tipo TEXT NOT NULL,
            Bio TEXT NOT NULL DEFAULT '',
            FotoUrl TEXT NOT NULL DEFAULT ''
        );

        CREATE TABLE IF NOT EXISTS EventoConvidados (
            EventoId INTEGER NOT NULL,
            ConvidadoId INTEGER NOT NULL,
            PRIMARY KEY (EventoId, ConvidadoId),
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
            FOREIGN KEY (ConvidadoId) REFERENCES Convidados(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS StandsEspacos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EventoId INTEGER NOT NULL,
            Setor TEXT NOT NULL,
            Codigo TEXT NOT NULL,
            PosicaoX INTEGER NOT NULL,
            PosicaoY INTEGER NOT NULL,
            Largura INTEGER NOT NULL DEFAULT 1,
            Altura INTEGER NOT NULL DEFAULT 1,
            TipoArea TEXT NOT NULL DEFAULT 'Stand',
            AreaX REAL NOT NULL DEFAULT 0,
            AreaY REAL NOT NULL DEFAULT 0,
            AreaLargura REAL NOT NULL DEFAULT 12,
            AreaAltura REAL NOT NULL DEFAULT 8,
            AreaMetrosQuadrados REAL NOT NULL DEFAULT 0,
            PrecoPorMetroQuadrado REAL NOT NULL DEFAULT 0,
            PrecoFixo REAL NOT NULL DEFAULT 0,
            Reservado INTEGER NOT NULL DEFAULT 0,
            NomeOcupante TEXT NOT NULL DEFAULT '',
            TipoOcupante TEXT NOT NULL DEFAULT '',
            Descricao TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS StandSetores (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EventoId INTEGER NOT NULL,
            Nome TEXT NOT NULL,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Checkins (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ReservaId INTEGER NOT NULL,
            QrCode TEXT NOT NULL,
            DataCheckin TEXT NULL,
            Status TEXT NOT NULL DEFAULT 'Pendente',
            FOREIGN KEY (ReservaId) REFERENCES Reservas(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Avaliacoes (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EventoId INTEGER NOT NULL,
            UsuarioCpf TEXT NOT NULL,
            Nota INTEGER NOT NULL,
            Comentario TEXT NOT NULL DEFAULT '',
            CriadoEm TEXT NOT NULL,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS Reservas (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf TEXT NOT NULL,
            EventoId INTEGER NOT NULL,
            TipoIngressoId INTEGER NULL,
            CupomUtilizado TEXT NULL,
            Assentos TEXT NOT NULL DEFAULT '',
            Quantidade INTEGER NOT NULL DEFAULT 1,
            PrecoUnitario REAL NOT NULL DEFAULT 0,
            ValorFinalPago REAL NOT NULL,
            FormaPagamento TEXT NOT NULL DEFAULT 'Pix',
            StatusPagamento TEXT NOT NULL DEFAULT 'Pago',
            CodigoPedido TEXT NOT NULL DEFAULT '',
            DataReserva TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
            FOREIGN KEY (TipoIngressoId) REFERENCES TiposIngresso(Id) ON DELETE SET NULL,
            FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo) ON DELETE SET NULL
        );

        CREATE TABLE IF NOT EXISTS RecuperacoesSenha (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf TEXT NOT NULL,
            CodigoHash TEXT NOT NULL,
            EmailDestino TEXT NOT NULL,
            ExpiraEm TEXT NOT NULL,
            CriadoEm TEXT NOT NULL,
            TentativasInvalidas INTEGER NOT NULL DEFAULT 0,
            UsadoEm TEXT NULL,
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE
        );
    ";
    cmd.ExecuteNonQuery();

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_TiposIngresso_Evento_Nome
        ON TiposIngresso (EventoId, Nome COLLATE NOCASE);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_InscricoesAtividades_Atividade_Usuario
        ON InscricoesAtividades (AtividadeId, UsuarioCpf);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Convidados_Nome_Tipo
        ON Convidados (Nome COLLATE NOCASE, Tipo COLLATE NOCASE);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_StandsEspacos_Evento_Codigo
        ON StandsEspacos (EventoId, Codigo COLLATE NOCASE);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_StandSetores_Evento_Nome
        ON StandSetores (EventoId, Nome COLLATE NOCASE);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Checkins_Reserva
        ON Checkins (ReservaId);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Checkins_QrCode
        ON Checkins (QrCode);");

    connection.Execute(@"
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Avaliacoes_Evento_Usuario
        ON Avaliacoes (EventoId, UsuarioCpf);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Eventos_DataEvento
        ON Eventos (DataEvento);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Eventos_Destaque_Data
        ON Eventos (EhDestaque, DataEvento);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Atividades_Evento_Horario
        ON Atividades (EventoId, Horario, Id);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_InscricoesAtividades_Usuario
        ON InscricoesAtividades (UsuarioCpf, AtividadeId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_Usuario_Data
        ON Reservas (UsuarioCpf, DataReserva DESC);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_Evento
        ON Reservas (EventoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_StandsEspacos_Evento_Setor
        ON StandsEspacos (EventoId, Setor, Codigo);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_EventoConvidados_Convidado
        ON EventoConvidados (ConvidadoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_EventoId
        ON Reservas (EventoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_UsuarioCpf
        ON Reservas (UsuarioCpf);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_TipoIngressoId
        ON Reservas (TipoIngressoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_DataReserva
        ON Reservas (DataReserva DESC);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Reservas_StatusPagamento
        ON Reservas (StatusPagamento);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Checkins_Status
        ON Checkins (Status);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Avaliacoes_EventoId
        ON Avaliacoes (EventoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_StandsEspacos_EventoId
        ON StandsEspacos (EventoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_Atividades_EventoId
        ON Atividades (EventoId);");

    connection.Execute(@"
        CREATE INDEX IF NOT EXISTS IX_InscricoesAtividades_UsuarioCpf
        ON InscricoesAtividades (UsuarioCpf);");

    EnsureColumnExists(connection, "Usuarios", "SenhaHash", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "Sobrenome", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "PaisResidencia", "TEXT NOT NULL DEFAULT 'Brasil'");
    EnsureColumnExists(connection, "Usuarios", "TipoDocumento", "TEXT NOT NULL DEFAULT 'CPF'");
    EnsureColumnExists(connection, "Usuarios", "CodigoPais", "TEXT NOT NULL DEFAULT '+55'");
    EnsureColumnExists(connection, "Usuarios", "Telefone", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "DataNascimento", "TEXT NULL");
    EnsureColumnExists(connection, "Usuarios", "Sexo", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "ImagemUrl", "TEXT");
    EnsureColumnExists(connection, "Eventos", "MapaImagemUrl", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "LocalEvento", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "CidadeEvento", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "Artista", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "GeneroMusical", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "EhDestaque", "INTEGER NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "Atividades", "HorarioFim", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Atividades", "Descricao", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "InscricoesAtividades", "Quantidade", "INTEGER NOT NULL DEFAULT 1");
    EnsureColumnExists(connection, "InscricoesAtividades", "Assentos", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "StandsEspacos", "TipoArea", "TEXT NOT NULL DEFAULT 'Stand'");
    EnsureColumnExists(connection, "StandsEspacos", "AreaX", "REAL NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "StandsEspacos", "AreaY", "REAL NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "StandsEspacos", "AreaLargura", "REAL NOT NULL DEFAULT 12");
    EnsureColumnExists(connection, "StandsEspacos", "AreaAltura", "REAL NOT NULL DEFAULT 8");
    EnsureColumnExists(connection, "StandsEspacos", "AreaMetrosQuadrados", "REAL NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "StandsEspacos", "PrecoPorMetroQuadrado", "REAL NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "StandsEspacos", "PrecoFixo", "REAL NOT NULL DEFAULT 0");

    connection.Execute(@"
        UPDATE Atividades
        SET HorarioFim = datetime(Horario, '+1 hour')
        WHERE TRIM(COALESCE(HorarioFim, '')) = '';");
    EnsureColumnExists(connection, "Reservas", "TipoIngressoId", "INTEGER NULL");
    EnsureReservasSchema(connection);
    EnsureMusicGenresCatalog(connection);
    EnsureCitiesCatalog(connection);

    EnsureDemoEvents(connection);
    EnsureTicketTypesCatalog(connection);
    EnsureDemoActivities(connection);
    EnsureDemoGuests(connection);
    EnsureDemoStands(connection);
    EnsureCheckinsForExistingReservations(connection);
    EnsureMusicGenresCatalog(connection);
    EnsureCitiesCatalog(connection);
}

app.MapGet("/", () => "API funcionando!");

app.MapPost("/api/usuarios", (Usuario usuario) =>
{
    if (string.IsNullOrWhiteSpace(usuario.Cpf) ||
        string.IsNullOrWhiteSpace(usuario.Nome) ||
        string.IsNullOrWhiteSpace(usuario.Email))
    {
        return Results.BadRequest("Preencha CPF, nome e email.");
    }

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var cpfExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE Cpf = @cpf",
        new { cpf = usuario.Cpf });

    if (cpfExiste > 0)
        return Results.BadRequest("Já existe um usuário com este CPF.");

    var emailExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE lower(Email) = lower(@email)",
        new { email = usuario.Email });

    if (emailExiste > 0)
        return Results.BadRequest("Já existe um usuário com este email.");

    connection.Execute(@"
        INSERT INTO Usuarios (Cpf, Nome, Email, SenhaHash)
        VALUES (@cpf, @nome, @email, @senhaHash)", new
    {
        cpf = usuario.Cpf,
        nome = usuario.Nome,
        email = usuario.Email,
        senhaHash = string.Empty
    });

    return Results.Ok("Usuário criado com sucesso. Para acessar a conta, finalize o cadastro pelo primeiro acesso.");
});

app.MapPost("/api/auth/usuarios/cadastro", (UsuarioCadastroRequest request) =>
{
    if (!AlphabitRules.IsValidUserRegistration(request))
    {
        return Results.BadRequest("Preencha CPF, nome, email e senha.");
    }

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var userByCpf = connection.QueryFirstOrDefault<(string Cpf, string Email, string SenhaHash)>(@"
        SELECT Cpf, Email, SenhaHash
        FROM Usuarios
        WHERE Cpf = @cpf", new { cpf = request.Cpf });

    var userByEmail = connection.QueryFirstOrDefault<(string Cpf, string Email, string SenhaHash)>(@"
        SELECT Cpf, Email, SenhaHash
        FROM Usuarios
        WHERE lower(Email) = lower(@email)", new { email = request.Email });

    if (!string.IsNullOrWhiteSpace(userByEmail.Cpf) &&
        !string.Equals(userByEmail.Cpf, request.Cpf, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Já existe uma conta com este email.");
    }

    if (!string.IsNullOrWhiteSpace(userByCpf.Cpf))
    {
        var senhaHashAtual = userByCpf.SenhaHash;
        if (!string.IsNullOrWhiteSpace(senhaHashAtual))
            return Results.BadRequest("Já existe uma conta com este CPF.");

        connection.Execute(@"
            UPDATE Usuarios
            SET Nome = $nome,
                Email = $email,
                SenhaHash = $senhaHash
            WHERE Cpf = $cpf", new
        {
            cpf = request.Cpf,
            nome = request.Nome,
            email = request.Email,
            senhaHash = AlphabitRules.HashPassword(request.Senha)
        });

        return Results.Ok(new AuthResponse
        {
            Sucesso = true,
            Perfil = "cliente",
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Mensagem = "Conta atualizada com sucesso."
        });
    }

    connection.Execute(@"
        INSERT INTO Usuarios (Cpf, Nome, Email, SenhaHash)
        VALUES ($cpf, $nome, $email, $senhaHash)", new
    {
        cpf = request.Cpf,
        nome = request.Nome,
        email = request.Email,
        senhaHash = AlphabitRules.HashPassword(request.Senha)
    });

    return Results.Ok(new AuthResponse
    {
        Sucesso = true,
        Perfil = "cliente",
        Nome = request.Nome,
        Cpf = request.Cpf,
        Email = request.Email,
        Mensagem = "Conta criada com sucesso."
    });
});

app.MapPost("/api/auth/usuarios/login", (UsuarioLoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Senha))
        return Results.BadRequest("Informe seu email ou CPF e a senha.");

    if (AlphabitRules.IsAdminCredential(request.Login, request.Senha, adminLogin, adminPassword))
    {
        return Results.Ok(new AuthResponse
        {
            Sucesso = true,
            Perfil = "admin",
            Token = adminToken,
            Nome = "Administrador GeekTop",
            Email = "admin@geektop.com",
            Mensagem = "Acesso administrativo liberado."
        });
    }

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuario = connection.QueryFirstOrDefault<(string Cpf, string Nome, string Email, string SenhaHash)>(@"
        SELECT Cpf, Nome, Email, SenhaHash
        FROM Usuarios
        WHERE Cpf = @login OR lower(Email) = lower(@login)", new { login = request.Login });

    if (string.IsNullOrWhiteSpace(usuario.Cpf))
        return Results.BadRequest("Cadastro não encontrado.");

    if (usuario.SenhaHash != AlphabitRules.HashPassword(request.Senha))
        return Results.BadRequest("Senha incorreta.");

    return Results.Ok(new AuthResponse
    {
        Sucesso = true,
        Perfil = "cliente",
        Mensagem = "Login realizado com sucesso.",
        Cpf = usuario.Cpf,
        Nome = usuario.Nome,
        Email = usuario.Email
    });
});

app.MapPost("/api/auth/usuarios/recuperar-senha", async (UsuarioRecuperacaoSenhaRequest request, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(request.Login))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Informe o e-mail ou CPF da conta."
        });

    if (!CanSendPasswordResetEmail(emailApiBaseUrl, emailApiKey, emailApiSenderEmail, smtpHost, smtpUser, smtpPassword))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "O envio de e-mail ainda não foi configurado no sistema."
        });

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuario = connection.QueryFirstOrDefault<(string Cpf, string Email, string Nome)>(@"
        SELECT Cpf, Email, Nome
        FROM Usuarios
        WHERE Cpf = @login OR lower(Email) = lower(@login)", new { login = request.Login });

    if (string.IsNullOrWhiteSpace(usuario.Cpf) || string.IsNullOrWhiteSpace(usuario.Email))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Não encontramos uma conta com esse e-mail ou CPF."
        });

    connection.Execute(@"
        UPDATE RecuperacoesSenha
        SET UsadoEm = @agora
        WHERE UsuarioCpf = @cpf AND UsadoEm IS NULL",
        new
        {
            cpf = usuario.Cpf,
            agora = DateTime.UtcNow.ToString("s")
        });

    var codigo = GenerateResetCode();
    var agoraUtc = DateTime.UtcNow;
    var expiraEmUtc = agoraUtc.AddMinutes(15);

    connection.Execute(@"
        INSERT INTO RecuperacoesSenha (UsuarioCpf, CodigoHash, EmailDestino, ExpiraEm, CriadoEm, TentativasInvalidas, UsadoEm)
        VALUES (@cpf, @codigoHash, @emailDestino, @expiraEm, @criadoEm, 0, NULL)", new
    {
        cpf = usuario.Cpf,
        codigoHash = AlphabitRules.HashPassword(codigo),
        emailDestino = usuario.Email,
        expiraEm = expiraEmUtc.ToString("s"),
        criadoEm = agoraUtc.ToString("s")
    });

    var corpo = $"""
Olá, {usuario.Nome}.

Recebemos um pedido para redefinir a senha da sua conta GeekTop.

Código de verificação: {codigo}

Esse código expira em 15 minutos.
Se você não solicitou a redefinição, ignore está mensagem.
""";

    try
    {
        if (CanSendPasswordResetEmailApi(emailApiBaseUrl, emailApiKey, emailApiSenderEmail))
        {
            await SendPasswordResetEmailByApiAsync(
                httpClientFactory.CreateClient(),
                emailApiBaseUrl!,
                emailApiKey!,
                emailApiSenderEmail!,
                emailApiSenderName,
                usuario.Email,
                "GeekTop - Código para redefinir senha",
                corpo);
        }
        else
        {
            await SendPasswordResetEmailBySmtpAsync(
                smtpHost!,
                smtpPort,
                smtpUser!,
                smtpPassword!,
                smtpDisplayName,
                usuario.Email,
                "GeekTop - Código para redefinir senha",
                corpo);
        }
    }
    catch (SmtpException)
    {
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Não foi possível enviar o e-mail de redefinição agora. Revise a configuração do Gmail e tente novamente."
        });
    }
    catch
    {
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Não foi possível enviar o e-mail de redefinição agora. Tente novamente em instantes."
        });
    }

    return Results.Ok(new RecuperacaoSenhaResponse
    {
        Sucesso = true,
        Mensagem = "Enviamos um código de redefinição para o e-mail cadastrado.",
        EmailMascarado = MaskEmail(usuario.Email)
    });
});

app.MapPost("/api/auth/usuarios/redefinir-senha", (UsuarioRedefinirSenhaRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Login) ||
        string.IsNullOrWhiteSpace(request.Codigo) ||
        string.IsNullOrWhiteSpace(request.NovaSenha))
    {
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Informe o login, o código e a nova senha."
        });
    }

    if (request.NovaSenha.Length < 4)
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "A nova senha precisa ter pelo menos 4 caracteres."
        });

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuario = connection.QueryFirstOrDefault<(string Cpf, string Email)>(@"
        SELECT Cpf, Email
        FROM Usuarios
        WHERE Cpf = @login OR lower(Email) = lower(@login)", new { login = request.Login });

    if (string.IsNullOrWhiteSpace(usuario.Cpf))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Não encontramos uma conta com esse e-mail ou CPF."
        });

    var recuperacao = connection.QueryFirstOrDefault<(int Id, string CodigoHash, string ExpiraEm, int TentativasInvalidas, string? UsadoEm)>(@"
        SELECT Id, CodigoHash, ExpiraEm, TentativasInvalidas, UsadoEm
        FROM RecuperacoesSenha
        WHERE UsuarioCpf = @cpf
        ORDER BY Id DESC
        LIMIT 1", new { cpf = usuario.Cpf });

    if (recuperacao.Id == 0)
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Solicite um novo código de redefinição."
        });

    if (!string.IsNullOrWhiteSpace(recuperacao.UsadoEm))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Esse código já foi utilizado. Solicite um novo código."
        });

    if (!DateTime.TryParse(recuperacao.ExpiraEm, out var expiraEmLocal) || expiraEmLocal < DateTime.UtcNow)
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "O código expirou. Solicite um novo envio."
        });

    if (recuperacao.TentativasInvalidas >= 5)
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Esse código foi bloqueado por excesso de tentativas. Solicite um novo."
        });

    var codigoHash = AlphabitRules.HashPassword(request.Codigo.Trim());
    if (!string.Equals(codigoHash, recuperacao.CodigoHash, StringComparison.Ordinal))
    {
        connection.Execute(@"
            UPDATE RecuperacoesSenha
            SET TentativasInvalidas = TentativasInvalidas + 1
            WHERE Id = @id", new { id = recuperacao.Id });

        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "O código informado está incorreto."
        });
    }

    using var transaction = connection.BeginTransaction();

    connection.Execute(@"
        UPDATE Usuarios
        SET SenhaHash = @senhaHash
        WHERE Cpf = @cpf", new
    {
        cpf = usuario.Cpf,
        senhaHash = AlphabitRules.HashPassword(request.NovaSenha)
    }, transaction);

    connection.Execute(@"
        UPDATE RecuperacoesSenha
        SET UsadoEm = @agora
        WHERE Id = @id", new
    {
        id = recuperacao.Id,
        agora = DateTime.UtcNow.ToString("s")
    }, transaction);

    transaction.Commit();

    return Results.Ok(new RecuperacaoSenhaResponse
    {
        Sucesso = true,
        Mensagem = "Senha redefinida com sucesso. Você já pode entrar na sua conta."
    });
});

app.MapPost("/api/auth/admin/login", (AdminLoginRequest request) =>
{
    if (!AlphabitRules.IsAdminCredential(request.Login, request.Senha, adminLogin, adminPassword))
        return Results.BadRequest("Login de administrador inválido.");

    return Results.Ok(new AuthResponse
    {
        Sucesso = true,
        Perfil = "admin",
        Token = adminToken,
        Nome = "Administrador GeekTop",
        Mensagem = "Acesso administrativo liberado."
    });
});

app.MapGet("/api/usuarios", (HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuarios = connection.Query<Usuario>("SELECT Cpf, Nome, Email FROM Usuarios").ToList();

    return Results.Ok(usuarios);
});

app.MapGet("/api/usuarios/{cpf}/perfil", (string cpf, HttpContext httpContext) =>
{
    var accessResult = EnsureUserAccess(httpContext, cpf);
    if (accessResult is not null)
        return accessResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var perfil = connection.QueryFirstOrDefault<UsuarioPerfil>(@"
        SELECT
            Cpf,
            Nome,
            Sobrenome,
            Email,
            PaisResidencia,
            TipoDocumento,
            CodigoPais,
            Telefone,
            DataNascimento,
            Sexo
        FROM Usuarios
        WHERE Cpf = @cpf", new { cpf });

    return perfil is null
        ? Results.NotFound("Usuário não encontrado.")
        : Results.Ok(perfil);
});

app.MapPut("/api/usuarios/{cpf}/perfil", (string cpf, UsuarioPerfil perfil, HttpContext httpContext) =>
{
    var accessResult = EnsureUserAccess(httpContext, cpf);
    if (accessResult is not null)
        return accessResult;

    if (string.IsNullOrWhiteSpace(perfil.Nome) ||
        string.IsNullOrWhiteSpace(perfil.Email))
    {
        return Results.BadRequest("Preencha pelo menos nome e email para atualizar o perfil.");
    }

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var updated = connection.Execute(@"
        UPDATE Usuarios
        SET Nome = @nome,
            Sobrenome = @sobrenome,
            Email = @email,
            PaisResidencia = @paisResidencia,
            TipoDocumento = @tipoDocumento,
            CodigoPais = @codigoPais,
            Telefone = @telefone,
            DataNascimento = @dataNascimento,
            Sexo = @sexo
        WHERE Cpf = @cpf", new
    {
        cpf,
        nome = perfil.Nome,
        sobrenome = perfil.Sobrenome,
        email = perfil.Email,
        paisResidencia = string.IsNullOrWhiteSpace(perfil.PaisResidencia) ? "Brasil" : perfil.PaisResidencia,
        tipoDocumento = string.IsNullOrWhiteSpace(perfil.TipoDocumento) ? "CPF" : perfil.TipoDocumento,
        codigoPais = string.IsNullOrWhiteSpace(perfil.CodigoPais) ? "+55" : perfil.CodigoPais,
        telefone = perfil.Telefone ?? string.Empty,
        dataNascimento = perfil.DataNascimento?.ToString("yyyy-MM-dd"),
        sexo = perfil.Sexo ?? string.Empty
    });

    if (updated == 0)
        return Results.NotFound("Usuário não encontrado.");

    var perfilAtualizado = connection.QueryFirst<UsuarioPerfil>(@"
        SELECT
            Cpf,
            Nome,
            Sobrenome,
            Email,
            PaisResidencia,
            TipoDocumento,
            CodigoPais,
            Telefone,
            DataNascimento,
            Sexo
        FROM Usuarios
        WHERE Cpf = @cpf", new { cpf });

    return Results.Ok(perfilAtualizado);
});

app.MapPost("/api/eventos", (Evento evento, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!AlphabitRules.IsValidEvent(evento))
        return Results.BadRequest("Preencha os dados do evento corretamente.");

    if (!IsRegularImagePayloadWithinLimit(evento.ImagemUrl))
        return Results.BadRequest("A imagem do evento é muito grande. Use uma imagem de até 500 KB ou um link externo.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventId = connection.ExecuteScalar<int>(@"
        INSERT INTO Eventos (Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl, EhDestaque)
        VALUES (@nome, @local, @cidade, @artista, @genero, @capacidade, @data, @preco, @imagem, @ehDestaque);
        SELECT last_insert_rowid();", new
    {
        nome = evento.Nome,
        local = evento.LocalEvento,
        cidade = evento.CidadeEvento,
        artista = evento.Artista,
        genero = evento.GeneroMusical,
        capacidade = evento.CapacidadeTotal,
        data = evento.DataEvento.ToString("s"),
        preco = evento.PrecoPadrao,
        imagem = string.IsNullOrWhiteSpace(evento.ImagemUrl) ? null : evento.ImagemUrl,
        ehDestaque = evento.EhDestaque ? 1 : 0
    });

    EnsureDefaultTicketTypesForEvent(connection, eventId, evento.PrecoPadrao, evento.CapacidadeTotal);
    EnsureDefaultActivitiesForEvent(connection, eventId, evento.Nome, evento.DataEvento);
    EnsureDefaultStandSectorsForEvent(connection, eventId);

    if (!string.IsNullOrWhiteSpace(evento.GeneroMusical))
    {
        connection.Execute(
            "INSERT OR IGNORE INTO GenerosMusicais (Nome) VALUES (@nome)",
            new { nome = evento.GeneroMusical.Trim() });
    }

    if (!string.IsNullOrWhiteSpace(evento.CidadeEvento))
    {
        connection.Execute(
            "INSERT OR IGNORE INTO CidadesEventos (Nome) VALUES (@nome)",
            new { nome = evento.CidadeEvento.Trim() });
    }

    return Results.Ok("Evento criado com sucesso!");
});

app.MapGet("/api/admin/eventos", (HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var stopwatch = Stopwatch.StartNew();
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventos = connection.Query<Evento>(@"
        SELECT
            e.Id,
            e.Nome,
            e.LocalEvento,
            e.CidadeEvento,
            e.Artista,
            e.GeneroMusical,
            e.CapacidadeTotal,
            e.DataEvento,
            e.PrecoPadrao,
            e.ImagemUrl,
            e.EhDestaque,
            CAST(COALESCE(ROUND(AVG(a.Nota), 1), 0.0) AS REAL) AS MediaAvaliacoes,
            COUNT(a.Id) AS TotalAvaliacoes
        FROM Eventos e
        LEFT JOIN Avaliacoes a ON a.EventoId = e.Id
        GROUP BY e.Id, e.Nome, e.LocalEvento, e.CidadeEvento, e.Artista, e.GeneroMusical, e.CapacidadeTotal, e.DataEvento, e.PrecoPadrao, e.ImagemUrl, e.EhDestaque
        ORDER BY e.DataEvento").ToList();

    app.Logger.LogInformation("Eventos admin listados em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

    return Results.Ok(eventos);
});

app.MapGet("/api/eventos/{id:int}/stands", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id }) > 0;

    if (!eventoExiste)
        return Results.NotFound("Evento não encontrado.");

    return Results.Ok(GetStandsForEvent(connection, id));
});

app.MapGet("/api/eventos/{id:int}/stand-setores", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id }) > 0;

    if (!eventoExiste)
        return Results.NotFound("Evento não encontrado.");

    var sectors = connection.Query<string>(@"
        SELECT Nome
        FROM StandSetores
        WHERE EventoId = @id
        ORDER BY Nome",
        new { id }).ToList();

    return Results.Ok(sectors);
});

app.MapPut("/api/admin/eventos/{id:int}/mapa-imagem", (int id, EventoMapaImagemRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var mapaImagemUrl = (request.MapaImagemUrl ?? string.Empty).Trim();
    if (!IsMapImagePayloadWithinLimit(mapaImagemUrl))
        return Results.BadRequest("A imagem da planta é muito grande. Use uma imagem de até 1 MB ou um link externo.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id }) > 0;

    if (!eventoExiste)
        return Results.NotFound("Evento não encontrado.");

    connection.Execute(
        "UPDATE Eventos SET MapaImagemUrl = @mapaImagemUrl WHERE Id = @id",
        new { id, mapaImagemUrl });

    return Results.Ok("Planta do evento atualizada com sucesso.");
});

app.MapPost("/api/admin/eventos/{id:int}/stand-setores", (int id, StandSetorRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nome = (request.Nome ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(nome))
        return Results.BadRequest("Informe o nome da linha.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id }) > 0;

    if (!eventoExiste)
        return Results.NotFound("Evento não encontrado.");

    try
    {
        connection.Execute(
            "INSERT INTO StandSetores (EventoId, Nome) VALUES (@id, @nome)",
            new { id, nome });
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        return Results.BadRequest("Já existe uma linha com este nome.");
    }

    return Results.Ok("Linha criada com sucesso.");
});

app.MapPut("/api/admin/eventos/{id:int}/stand-setores/{nomeAtual}", (int id, string nomeAtual, StandSetorRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var currentName = Uri.UnescapeDataString(nomeAtual).Trim();
    var newName = (request.Nome ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(currentName) || string.IsNullOrWhiteSpace(newName))
        return Results.BadRequest("Informe o nome da linha.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();
    try
    {
        var rows = connection.Execute(@"
            UPDATE StandSetores
            SET Nome = @newName
            WHERE EventoId = @id AND Nome = @currentName COLLATE NOCASE",
            new { id, currentName, newName },
            transaction);

        if (rows == 0)
        {
            connection.Execute(
                "INSERT INTO StandSetores (EventoId, Nome) VALUES (@id, @newName)",
                new { id, newName },
                transaction);
        }

        connection.Execute(@"
            UPDATE StandsEspacos
            SET Setor = @newName
            WHERE EventoId = @id AND Setor = @currentName COLLATE NOCASE",
            new { id, currentName, newName },
            transaction);

        transaction.Commit();
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        transaction.Rollback();
        return Results.BadRequest("Já existe uma linha com este nome.");
    }

    return Results.Ok("Linha renomeada com sucesso.");
});

app.MapDelete("/api/admin/eventos/{id:int}/stand-setores/{nome}", (int id, string nome, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var sectorName = Uri.UnescapeDataString(nome).Trim();
    if (string.IsNullOrWhiteSpace(sectorName))
        return Results.BadRequest("Informe o nome da linha.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var hasStands = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM StandsEspacos
        WHERE EventoId = @id AND Setor = @sectorName COLLATE NOCASE",
        new { id, sectorName }) > 0;

    if (hasStands)
        return Results.BadRequest("Esta linha ainda possui stands. Mova ou apague os stands antes de excluir a linha.");

    var rows = connection.Execute(@"
        DELETE FROM StandSetores
        WHERE EventoId = @id AND Nome = @sectorName COLLATE NOCASE",
        new { id, sectorName });

    return rows == 0
        ? Results.NotFound("Linha não encontrada.")
        : Results.Ok("Linha excluída com sucesso.");
});

app.MapPost("/api/admin/eventos/{id:int}/stands", (int id, StandAlocacaoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var codigo = (request.Codigo ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(codigo))
        return Results.BadRequest("Informe o código do stand.");

    var setor = string.IsNullOrWhiteSpace(request.Setor) ? "Linha 1" : request.Setor.Trim();
    var tipoArea = string.IsNullOrWhiteSpace(request.TipoArea) ? "Stand" : request.TipoArea.Trim();
    var nomeOcupante = (request.NomeOcupante ?? string.Empty).Trim();
    var tipoOcupante = (request.TipoOcupante ?? string.Empty).Trim();
    var descricao = (request.Descricao ?? string.Empty).Trim();

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id }) > 0;

    if (!eventoExiste)
        return Results.NotFound("Evento não encontrado.");

    var codigoDuplicado = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM StandsEspacos
        WHERE EventoId = @id
          AND Codigo = @codigo COLLATE NOCASE",
        new { id, codigo }) > 0;

    if (codigoDuplicado)
        return Results.BadRequest("Já existe outro stand com este código.");

    if (!string.IsNullOrWhiteSpace(nomeOcupante))
    {
        var nomeDuplicado = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*)
            FROM StandsEspacos
            WHERE EventoId = @id
              AND NomeOcupante = @nomeOcupante COLLATE NOCASE",
            new { id, nomeOcupante }) > 0;

        if (nomeDuplicado)
            return Results.BadRequest("Já existe um stand cadastrado com esse nome.");
    }

    connection.Execute(@"
        INSERT OR IGNORE INTO StandSetores (EventoId, Nome)
        VALUES (@id, @setor)",
        new { id, setor });

    var nextPosition = connection.ExecuteScalar<int>(
        "SELECT COALESCE(MAX(PosicaoX), 0) + 1 FROM StandsEspacos WHERE EventoId = @id AND Setor = @setor COLLATE NOCASE",
        new { id, setor });

    connection.Execute(@"
        INSERT INTO StandsEspacos (
            EventoId, Setor, Codigo, PosicaoX, PosicaoY, Largura, Altura, TipoArea,
            AreaX, AreaY, AreaLargura, AreaAltura, AreaMetrosQuadrados, PrecoPorMetroQuadrado, PrecoFixo,
            Reservado, NomeOcupante, TipoOcupante, Descricao)
        VALUES (
            @id, @setor, @codigo, @posicaoX, 9, 1, 1, @tipoArea,
            @areaX, @areaY, @areaLargura, @areaAltura, @areaMetrosQuadrados, @precoPorMetroQuadrado, @precoFixo,
            @reservado, @nomeOcupante, @tipoOcupante, @descricao)",
        new
        {
            id,
            setor,
            codigo,
            posicaoX = Math.Max(1, nextPosition),
            tipoArea,
            areaX = Math.Clamp(request.AreaX, 0, 100),
            areaY = Math.Clamp(request.AreaY, 0, 100),
            areaLargura = Math.Clamp(request.AreaLargura <= 0 ? 12 : request.AreaLargura, 1, 100),
            areaAltura = Math.Clamp(request.AreaAltura <= 0 ? 8 : request.AreaAltura, 1, 100),
            areaMetrosQuadrados = Math.Max(0, request.AreaMetrosQuadrados),
            precoPorMetroQuadrado = Math.Max(0, request.PrecoPorMetroQuadrado),
            precoFixo = Math.Max(0, request.PrecoFixo),
            reservado = !string.IsNullOrWhiteSpace(nomeOcupante) ? 1 : 0,
            nomeOcupante = string.IsNullOrWhiteSpace(nomeOcupante) ? string.Empty : nomeOcupante,
            tipoOcupante = string.IsNullOrWhiteSpace(nomeOcupante) ? string.Empty : tipoOcupante,
            descricao = string.IsNullOrWhiteSpace(nomeOcupante) ? string.Empty : descricao
        });

    return Results.Ok("Stand cadastrado com sucesso.");
});

app.MapPut("/api/admin/eventos/{id:int}/stands/{standId:int}", (int id, int standId, StandAlocacaoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOcupante = (request.NomeOcupante ?? string.Empty).Trim();
    var tipoOcupante = (request.TipoOcupante ?? string.Empty).Trim();
    var descricao = (request.Descricao ?? string.Empty).Trim();
    var setor = string.IsNullOrWhiteSpace(request.Setor) ? "Linha 1" : request.Setor.Trim();
    var codigo = (request.Codigo ?? string.Empty).Trim();
    var tipoArea = string.IsNullOrWhiteSpace(request.TipoArea) ? "Stand" : request.TipoArea.Trim();
    var areaX = Math.Clamp(request.AreaX, 0, 100);
    var areaY = Math.Clamp(request.AreaY, 0, 100);
    var areaLargura = Math.Clamp(request.AreaLargura <= 0 ? 12 : request.AreaLargura, 1, 100);
    var areaAltura = Math.Clamp(request.AreaAltura <= 0 ? 8 : request.AreaAltura, 1, 100);
    var areaMetrosQuadrados = Math.Max(0, request.AreaMetrosQuadrados);
    var precoPorMetroQuadrado = Math.Max(0, request.PrecoPorMetroQuadrado);
    var precoFixo = Math.Max(0, request.PrecoFixo);

    if (request.Reservado && string.IsNullOrWhiteSpace(nomeOcupante))
        return Results.BadRequest("Informe a empresa, marca ou atração que ficará neste stand.");

    if (string.IsNullOrWhiteSpace(codigo))
        return Results.BadRequest("Informe o código do stand.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var standExiste = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM StandsEspacos
        WHERE Id = @standId AND EventoId = @id",
        new { standId, id }) > 0;

    if (!standExiste)
        return Results.NotFound("Stand não encontrado para este evento.");

    var codigoDuplicado = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM StandsEspacos
        WHERE EventoId = @id
          AND Id <> @standId
          AND Codigo = @codigo COLLATE NOCASE",
        new { id, standId, codigo }) > 0;

    if (codigoDuplicado)
        return Results.BadRequest("Já existe outro stand com este código.");

    if (!string.IsNullOrWhiteSpace(nomeOcupante))
    {
        var nomeDuplicado = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*)
            FROM StandsEspacos
            WHERE EventoId = @id
              AND Id <> @standId
              AND NomeOcupante = @nomeOcupante COLLATE NOCASE",
            new { id, standId, nomeOcupante }) > 0;

        if (nomeDuplicado)
            return Results.BadRequest("Já existe um stand cadastrado com esse nome.");
    }

    connection.Execute(@"
        INSERT OR IGNORE INTO StandSetores (EventoId, Nome)
        VALUES (@id, @setor)",
        new { id, setor });

    connection.Execute(@"
        UPDATE StandsEspacos
        SET Setor = @setor,
            Codigo = @codigo,
            Reservado = @reservado,
            TipoArea = @tipoArea,
            AreaX = @areaX,
            AreaY = @areaY,
            AreaLargura = @areaLargura,
            AreaAltura = @areaAltura,
            AreaMetrosQuadrados = @areaMetrosQuadrados,
            PrecoPorMetroQuadrado = @precoPorMetroQuadrado,
            PrecoFixo = @precoFixo,
            NomeOcupante = @nomeOcupante,
            TipoOcupante = @tipoOcupante,
            Descricao = @descricao
        WHERE Id = @standId AND EventoId = @id",
        new
        {
            id,
            standId,
            setor,
            codigo,
            reservado = request.Reservado ? 1 : 0,
            tipoArea,
            areaX,
            areaY,
            areaLargura,
            areaAltura,
            areaMetrosQuadrados,
            precoPorMetroQuadrado,
            precoFixo,
            nomeOcupante = request.Reservado ? nomeOcupante : string.Empty,
            tipoOcupante = request.Reservado ? tipoOcupante : string.Empty,
            descricao = request.Reservado ? descricao : string.Empty
        });

    return Results.Ok("Stand atualizado com sucesso.");
});

app.MapDelete("/api/admin/eventos/{id:int}/stands/{standId:int}", (int id, int standId, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var rows = connection.Execute(
        "DELETE FROM StandsEspacos WHERE Id = @standId AND EventoId = @id",
        new { id, standId });

    return rows == 0
        ? Results.NotFound("Stand não encontrado para este evento.")
        : Results.Ok("Stand apagado com sucesso.");
});

app.MapGet("/api/admin/generos", (HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var generos = connection.Query<string>(
        "SELECT Nome FROM GenerosMusicais ORDER BY Nome").ToList();

    return Results.Ok(generos);
});

app.MapPost("/api/admin/generos", (GeneroMusicalCatalogoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nome = (request.Nome ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(nome))
        return Results.BadRequest("Informe o nome da categoria geek.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome });

    if (!string.IsNullOrWhiteSpace(existente))
        return Results.Ok(new { mensagem = "Categoria geek já cadastrada.", nome = existente });

    connection.Execute(
        "INSERT INTO GenerosMusicais (Nome) VALUES (@nome)",
        new { nome });

    return Results.Ok(new { mensagem = "Categoria geek cadastrada com sucesso.", nome });
});

app.MapPut("/api/admin/generos/{nomeAtual}", (string nomeAtual, GeneroMusicalCatalogoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    var novoNome = (request.Nome ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe a categoria geek atual.");

    if (string.IsNullOrWhiteSpace(novoNome))
        return Results.BadRequest("Informe o novo nome da categoria geek.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal });

    if (string.IsNullOrWhiteSpace(existente))
        return Results.NotFound("Categoria geek não encontrada.");

    var conflito = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome) AND lower(Nome) <> lower(@original)",
        new { nome = novoNome, original = nomeOriginal });

    if (!string.IsNullOrWhiteSpace(conflito))
        return Results.BadRequest("Já existe uma categoria geek com esse nome.");

    using var transaction = connection.BeginTransaction();

    connection.Execute(
        "UPDATE GenerosMusicais SET Nome = @novoNome WHERE lower(Nome) = lower(@nomeOriginal)",
        new { novoNome, nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET GeneroMusical = @novoNome WHERE lower(GeneroMusical) = lower(@nomeOriginal)",
        new { novoNome, nomeOriginal },
        transaction);

    transaction.Commit();

    return Results.Ok(new { mensagem = "Categoria geek atualizada com sucesso.", nome = novoNome });
});

app.MapDelete("/api/admin/generos/{nomeAtual}", (string nomeAtual, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe a categoria geek que sera excluida.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var próximoGenero = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) <> lower(@nome) ORDER BY Nome LIMIT 1",
        new { nome = nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET GeneroMusical = @novoGenero WHERE lower(GeneroMusical) = lower(@nome)",
        new { nome = nomeOriginal, novoGenero = próximoGenero ?? string.Empty },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal },
        transaction);

    if (deleted == 0)
        return Results.NotFound("Categoria geek não encontrada.");

    transaction.Commit();

    var mensagem = string.IsNullOrWhiteSpace(próximoGenero)
        ? "Categoria geek excluida com sucesso."
        : $"Categoria geek excluida com sucesso. Eventos vinculados foram atualizados para {próximoGenero}.";

    return Results.Ok(new { mensagem });
});

app.MapGet("/api/admin/cidades", (HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var cidades = connection.Query<string>(
        "SELECT Nome FROM CidadesEventos ORDER BY Nome").ToList();

    return Results.Ok(cidades);
});

app.MapPost("/api/admin/cidades", (CidadeCatalogoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nome = (request.Nome ?? string.Empty).Trim();
    if (string.IsNullOrWhiteSpace(nome))
        return Results.BadRequest("Informe o nome da cidade.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) = lower(@nome)",
        new { nome });

    if (!string.IsNullOrWhiteSpace(existente))
        return Results.Ok(new { mensagem = "Cidade já cadastrada.", nome = existente });

    connection.Execute(
        "INSERT INTO CidadesEventos (Nome) VALUES (@nome)",
        new { nome });

    return Results.Ok(new { mensagem = "Cidade cadastrada com sucesso.", nome });
});

app.MapPut("/api/admin/cidades/{nomeAtual}", (string nomeAtual, CidadeCatalogoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    var novoNome = (request.Nome ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe a cidade atual.");

    if (string.IsNullOrWhiteSpace(novoNome))
        return Results.BadRequest("Informe o novo nome da cidade.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal });

    if (string.IsNullOrWhiteSpace(existente))
        return Results.NotFound("Cidade não encontrada.");

    var conflito = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) = lower(@nome) AND lower(Nome) <> lower(@original)",
        new { nome = novoNome, original = nomeOriginal });

    if (!string.IsNullOrWhiteSpace(conflito))
        return Results.BadRequest("Já existe uma cidade com esse nome.");

    using var transaction = connection.BeginTransaction();

    connection.Execute(
        "UPDATE CidadesEventos SET Nome = @novoNome WHERE lower(Nome) = lower(@nomeOriginal)",
        new { novoNome, nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET CidadeEvento = @novoNome WHERE lower(CidadeEvento) = lower(@nomeOriginal)",
        new { novoNome, nomeOriginal },
        transaction);

    transaction.Commit();

    return Results.Ok(new { mensagem = "Cidade atualizada com sucesso.", nome = novoNome });
});

app.MapDelete("/api/admin/cidades/{nomeAtual}", (string nomeAtual, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe a cidade que sera excluida.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var próximaCidade = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) <> lower(@nome) ORDER BY Nome LIMIT 1",
        new { nome = nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET CidadeEvento = @novaCidade WHERE lower(CidadeEvento) = lower(@nome)",
        new { nome = nomeOriginal, novaCidade = próximaCidade ?? string.Empty },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM CidadesEventos WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal },
        transaction);

    if (deleted == 0)
        return Results.NotFound("Cidade não encontrada.");

    transaction.Commit();

    var mensagem = string.IsNullOrWhiteSpace(próximaCidade)
        ? "Cidade excluida com sucesso."
        : $"Cidade excluida com sucesso. Eventos vinculados foram atualizados para {próximaCidade}.";

    return Results.Ok(new { mensagem });
});

app.MapPut("/api/admin/eventos/{id:int}", (int id, EventoAdminRequest evento, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!AlphabitRules.IsValidEvent(new Evento
    {
        Nome = evento.Nome,
        LocalEvento = evento.LocalEvento,
        CidadeEvento = evento.CidadeEvento,
        Artista = evento.Artista,
        GeneroMusical = evento.GeneroMusical,
        CapacidadeTotal = evento.CapacidadeTotal,
        DataEvento = evento.DataEvento,
        PrecoPadrao = evento.PrecoPadrao,
        ImagemUrl = evento.ImagemUrl,
        EhDestaque = evento.EhDestaque
    }))
        return Results.BadRequest("Preencha os dados do evento corretamente.");

    if (!IsRegularImagePayloadWithinLimit(evento.ImagemUrl))
        return Results.BadRequest("A imagem do evento é muito grande. Use uma imagem de até 500 KB ou um link externo.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var updated = connection.Execute(@"
        UPDATE Eventos
        SET Nome = $nome,
            LocalEvento = $local,
            CidadeEvento = $cidade,
            Artista = $artista,
            GeneroMusical = $genero,
            CapacidadeTotal = $capacidade,
            DataEvento = $data,
            PrecoPadrao = $preco,
            ImagemUrl = $imagem,
            EhDestaque = $ehDestaque
        WHERE Id = $id", new
    {
        id,
        nome = evento.Nome,
        local = evento.LocalEvento,
        cidade = evento.CidadeEvento,
        artista = evento.Artista,
        genero = evento.GeneroMusical,
        capacidade = evento.CapacidadeTotal,
        data = evento.DataEvento.ToString("s"),
        preco = evento.PrecoPadrao,
        imagem = string.IsNullOrWhiteSpace(evento.ImagemUrl) ? null : evento.ImagemUrl,
        ehDestaque = evento.EhDestaque ? 1 : 0
    });
    if (updated == 0)
        return Results.NotFound("Evento não encontrado.");

    EnsureDefaultTicketTypesForEvent(connection, id, evento.PrecoPadrao, evento.CapacidadeTotal);

    if (!string.IsNullOrWhiteSpace(evento.GeneroMusical))
    {
        connection.Execute(
            "INSERT OR IGNORE INTO GenerosMusicais (Nome) VALUES (@nome)",
            new { nome = evento.GeneroMusical.Trim() });
    }

    if (!string.IsNullOrWhiteSpace(evento.CidadeEvento))
    {
        connection.Execute(
            "INSERT OR IGNORE INTO CidadesEventos (Nome) VALUES (@nome)",
            new { nome = evento.CidadeEvento.Trim() });
    }

    return Results.Ok("Evento atualizado com sucesso!");
});

app.MapPut("/api/admin/eventos/{id:int}/destaque", (int id, EventoAdminRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var updated = connection.Execute(
        "UPDATE Eventos SET EhDestaque = @ehDestaque WHERE Id = @id",
        new { id, ehDestaque = request.EhDestaque ? 1 : 0 });

    if (updated == 0)
        return Results.NotFound("Evento não encontrado.");

    return Results.Ok(request.EhDestaque
        ? "Evento adicionado aos destaques."
        : "Evento removido dos destaques.");
});

app.MapDelete("/api/admin/eventos/{id:int}", (int id, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id },
        transaction);

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    connection.Execute(
        "DELETE FROM Reservas WHERE EventoId = @id",
        new { id },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM Eventos WHERE Id = @id",
        new { id },
        transaction);

    transaction.Commit();

    if (deleted == 0)
        return Results.NotFound("Evento não encontrado.");

    return Results.Ok("Evento removido com sucesso!");
});

app.MapGet("/api/eventos", () =>
{
    var stopwatch = Stopwatch.StartNew();
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventos = connection.Query<Evento>(@"
        SELECT
            e.Id,
            e.Nome,
            e.LocalEvento,
            e.CidadeEvento,
            e.Artista,
            e.GeneroMusical,
            e.CapacidadeTotal,
            e.DataEvento,
            e.PrecoPadrao,
            e.ImagemUrl,
            e.EhDestaque,
            CAST(COALESCE(ROUND(AVG(a.Nota), 1), 0.0) AS REAL) AS MediaAvaliacoes,
            COUNT(a.Id) AS TotalAvaliacoes
        FROM Eventos e
        LEFT JOIN Avaliacoes a ON a.EventoId = e.Id
        GROUP BY e.Id, e.Nome, e.LocalEvento, e.CidadeEvento, e.Artista, e.GeneroMusical, e.CapacidadeTotal, e.DataEvento, e.PrecoPadrao, e.ImagemUrl, e.EhDestaque").ToList();

    app.Logger.LogInformation("Eventos públicos listados em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

    return Results.Ok(eventos);
});

app.MapGet("/api/eventos/{id:int}", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var evento = connection.QueryFirstOrDefault<Evento>(
        @"
        SELECT
            e.Id,
            e.Nome,
            e.LocalEvento,
            e.CidadeEvento,
            e.Artista,
            e.GeneroMusical,
            e.CapacidadeTotal,
            e.DataEvento,
            e.PrecoPadrao,
            e.ImagemUrl,
            e.EhDestaque,
            CAST(COALESCE(ROUND(AVG(a.Nota), 1), 0.0) AS REAL) AS MediaAvaliacoes,
            COUNT(a.Id) AS TotalAvaliacoes
        FROM Eventos e
        LEFT JOIN Avaliacoes a ON a.EventoId = e.Id
        WHERE e.Id = @id
        GROUP BY e.Id, e.Nome, e.LocalEvento, e.CidadeEvento, e.Artista, e.GeneroMusical, e.CapacidadeTotal, e.DataEvento, e.PrecoPadrao, e.ImagemUrl, e.EhDestaque",
        new { id });

    return evento is null ? Results.NotFound("Evento não encontrado.") : Results.Ok(evento);
});

app.MapGet("/api/eventos/{id:int}/assentos-ocupados", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id });

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var assentos = connection.Query<string>(
        "SELECT Assentos FROM Reservas WHERE EventoId = @id AND Assentos <> ''",
        new { id })
        .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(value => value)
        .ToList();

    return Results.Ok(new
    {
        EventoId = id,
        AssentosOcupados = assentos
    });
});

app.MapGet("/api/eventos/{id:int}/tipos-ingresso", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id });

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var tipos = connection.Query<TipoIngresso>(@"
        SELECT Id, EventoId, Nome, Beneficios, Preco, QuantidadeDisponivel
        FROM TiposIngresso
        WHERE EventoId = @id
        ORDER BY Preco, Id",
        new { id }).ToList();

    return Results.Ok(tipos);
});

app.MapGet("/api/eventos/{id:int}/atividades", (int id, HttpContext httpContext) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id });

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var usuarioCpf = httpContext.Request.Headers.TryGetValue("X-User-Cpf", out var userCpfHeader)
        ? userCpfHeader.ToString().Trim()
        : string.Empty;

    var atividades = connection.Query<AtividadeResumoResponse>(@"
        SELECT
            a.Id,
            a.EventoId,
            a.Nome,
            a.Horario,
            a.HorarioFim,
            a.Tipo,
            a.Descricao,
            a.LimiteParticipantes,
            COALESCE(SUM(CASE WHEN i.Id IS NULL THEN 0 ELSE COALESCE(i.Quantidade, 1) END), 0) AS Inscritos,
            MAX(a.LimiteParticipantes - (
                SELECT COALESCE(SUM(COALESCE(total.Quantidade, 1)), 0) FROM InscricoesAtividades total
                WHERE total.AtividadeId = a.Id
            ), 0) AS VagasRestantes,
            CASE
                WHEN EXISTS (
                    SELECT 1 FROM InscricoesAtividades ui
                    WHERE ui.AtividadeId = a.Id AND ui.UsuarioCpf = @usuarioCpf
                ) THEN 1
                ELSE 0
            END AS UsuarioInscrito,
            COALESCE((
                SELECT COALESCE(uiq.Quantidade, 1) FROM InscricoesAtividades uiq
                WHERE uiq.AtividadeId = a.Id AND uiq.UsuarioCpf = @usuarioCpf
                LIMIT 1
            ), 0) AS UsuarioQuantidade,
            COALESCE((
                SELECT uiq.Assentos FROM InscricoesAtividades uiq
                WHERE uiq.AtividadeId = a.Id AND uiq.UsuarioCpf = @usuarioCpf
                LIMIT 1
            ), '') AS UsuarioAssentos,
            COALESCE((
                SELECT GROUP_CONCAT(NULLIF(TRIM(total.Assentos), ''), ',')
                FROM InscricoesAtividades total
                WHERE total.AtividadeId = a.Id
            ), '') AS AssentosOcupados
        FROM Atividades a
        LEFT JOIN InscricoesAtividades i ON i.AtividadeId = a.Id
        WHERE a.EventoId = @id
        GROUP BY a.Id, a.EventoId, a.Nome, a.Horario, a.HorarioFim, a.Tipo, a.Descricao, a.LimiteParticipantes
        ORDER BY a.Horario, a.Id",
        new { id, usuarioCpf }).ToList();

    return Results.Ok(atividades);
});

app.MapGet("/api/convidados", () =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var convidados = connection.Query<Convidado>(@"
        SELECT Id, Nome, Tipo, Bio, FotoUrl
        FROM Convidados
        ORDER BY Nome, Tipo").ToList();

    return Results.Ok(convidados);
});

app.MapGet("/api/eventos/{id:int}/convidados", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id });

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var convidados = connection.Query<Convidado>(@"
        SELECT c.Id, c.Nome, c.Tipo, c.Bio, c.FotoUrl
        FROM EventoConvidados ec
        INNER JOIN Convidados c ON c.Id = ec.ConvidadoId
        WHERE ec.EventoId = @id
        ORDER BY c.Tipo, c.Nome",
        new { id }).ToList();

    return Results.Ok(convidados);
});

app.MapGet("/api/eventos/{id:int}/avaliacoes", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var avaliacoes = connection.Query<AvaliacaoResumoResponse>(@"
        SELECT
            a.Id,
            a.EventoId,
            a.UsuarioCpf,
            u.Nome AS UsuarioNome,
            a.Nota,
            a.Comentario,
            a.CriadoEm
        FROM Avaliacoes a
        INNER JOIN Usuarios u ON u.Cpf = a.UsuarioCpf
        WHERE a.EventoId = @id
        ORDER BY a.CriadoEm DESC",
        new { id }).ToList();

    foreach (var avaliacao in avaliacoes)
    {
        avaliacao.CriadoEm = ConvertUtcLikeToBrasília(avaliacao.CriadoEm);
    }

    return Results.Ok(avaliacoes);
});

app.MapPost("/api/avaliacoes", (AvaliacaoRequest request, HttpContext httpContext) =>
{
    request.UsuarioCpf = (request.UsuarioCpf ?? string.Empty).Trim();
    request.Comentario = (request.Comentario ?? string.Empty).Trim();

    var accessResult = EnsureUserAccess(httpContext, request.UsuarioCpf);
    if (accessResult is not null)
        return accessResult;

    if (request.EventoId <= 0 || string.IsNullOrWhiteSpace(request.UsuarioCpf))
        return Results.BadRequest("Informe evento e usuário para avaliar.");

    if (request.Nota < 1 || request.Nota > 5)
        return Results.BadRequest("A nota deve estar entre 1 e 5.");

    if (request.Comentario.Length > 600)
        return Results.BadRequest("O comentario deve ter no maximo 600 caracteres.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @eventoId",
        new { eventoId = request.EventoId });
    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var reservaValida = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE EventoId = @eventoId
          AND UsuarioCpf = @usuarioCpf
          AND COALESCE(NULLIF(StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')",
        new { eventoId = request.EventoId, usuarioCpf = request.UsuarioCpf });
    if (reservaValida == 0)
        return Results.BadRequest("Usuário só pode avaliar evento reservado.");

    var avaliacaoDuplicada = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Avaliacoes
        WHERE EventoId = @eventoId
          AND UsuarioCpf = @usuarioCpf",
        new { eventoId = request.EventoId, usuarioCpf = request.UsuarioCpf });
    if (avaliacaoDuplicada > 0)
        return Results.BadRequest("Usuário já avaliou este evento.");

    try
    {
        var criadoEm = DateTime.UtcNow;
        connection.Execute(@"
            INSERT INTO Avaliacoes (EventoId, UsuarioCpf, Nota, Comentario, CriadoEm)
            VALUES (@eventoId, @usuarioCpf, @nota, @comentario, @criadoEm)",
            new
            {
                eventoId = request.EventoId,
                usuarioCpf = request.UsuarioCpf,
                nota = request.Nota,
                comentario = request.Comentario,
                criadoEm = criadoEm.ToString("s")
            });

        return Results.Ok(new
        {
            mensagem = "Avaliação registrada com sucesso."
        });
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        return Results.BadRequest("Usuário já avaliou este evento.");
    }
});

app.MapDelete("/api/admin/avaliacoes/{id:int}", (int id, HttpContext httpContext) =>
{
    var accessResult = EnsureAdminAccess(httpContext);
    if (accessResult is not null)
        return accessResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var affectedRows = connection.Execute(
        "DELETE FROM Avaliacoes WHERE Id = @id",
        new { id });

    return affectedRows == 0
        ? Results.NotFound("Avaliacao não encontrada.")
        : Results.Ok("Avaliação removida com sucesso.");
});

app.MapPost("/api/convidados", (Convidado convidado, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!IsValidGuest(convidado))
        return Results.BadRequest("Preencha os dados do convidado corretamente.");

    if (!IsRegularImagePayloadWithinLimit(convidado.FotoUrl))
        return Results.BadRequest("A foto do convidado é muito grande. Use uma imagem de até 500 KB ou um link externo.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existing = connection.QueryFirstOrDefault<Convidado>(@"
        SELECT Id, Nome, Tipo, Bio, FotoUrl
        FROM Convidados
        WHERE lower(Nome) = lower(@nome) AND lower(Tipo) = lower(@tipo)",
        new { nome = convidado.Nome.Trim(), tipo = convidado.Tipo.Trim() });

    if (existing is not null)
        return Results.BadRequest("Já existe um convidado com este nome e tipo.");

    connection.Execute(@"
        INSERT INTO Convidados (Nome, Tipo, Bio, FotoUrl)
        VALUES (@Nome, @Tipo, @Bio, @FotoUrl)", new
    {
        Nome = convidado.Nome.Trim(),
        Tipo = convidado.Tipo.Trim(),
        Bio = convidado.Bio?.Trim() ?? string.Empty,
        FotoUrl = convidado.FotoUrl?.Trim() ?? string.Empty
    });

    return Results.Ok("Convidado cadastrado com sucesso!");
});

app.MapPost("/api/eventos/{id:int}/convidados", (int id, EventoConvidadoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (request.ConvidadoId <= 0)
        return Results.BadRequest("Informe um convidado válido.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id },
        transaction);

    if (eventoExiste == 0)
        return Results.NotFound("Evento não encontrado.");

    var convidadoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Convidados WHERE Id = @convidadoId",
        new { convidadoId = request.ConvidadoId },
        transaction);

    if (convidadoExiste == 0)
        return Results.NotFound("Convidado não encontrado.");

    connection.Execute(@"
        INSERT OR IGNORE INTO EventoConvidados (EventoId, ConvidadoId)
        VALUES (@eventoId, @convidadoId)",
        new { eventoId = id, convidadoId = request.ConvidadoId },
        transaction);

    transaction.Commit();

    return Results.Ok("Convidado associado ao evento com sucesso!");
});

app.MapDelete("/api/eventos/{id:int}/convidados/{convidadoId:int}", (int id, int convidadoId, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var deleted = connection.Execute(@"
        DELETE FROM EventoConvidados
        WHERE EventoId = @id AND ConvidadoId = @convidadoId",
        new { id, convidadoId });

    return deleted == 0
        ? Results.NotFound("Associação de convidado não encontrada.")
        : Results.Ok("Convidado removido do evento com sucesso!");
});

app.MapDelete("/api/admin/convidados/{id:int}", (int id, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    connection.Execute(
        "DELETE FROM EventoConvidados WHERE ConvidadoId = @id",
        new { id },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM Convidados WHERE Id = @id",
        new { id },
        transaction);

    transaction.Commit();

    return deleted == 0
        ? Results.NotFound("Convidado não encontrado.")
        : Results.Ok("Convidado removido com sucesso!");
});

app.MapPost("/api/atividades", (Atividade atividade, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!IsValidActivity(atividade))
        return Results.BadRequest("Preencha os dados da atividade corretamente.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @eventoId",
        new { eventoId = atividade.EventoId });

    if (eventoExiste == 0)
        return Results.BadRequest("Evento da atividade não encontrado.");

    var atividadeExiste = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*) FROM Atividades
        WHERE EventoId = @eventoId AND lower(Nome) = lower(@nome)",
        new { eventoId = atividade.EventoId, nome = atividade.Nome.Trim() });

    if (atividadeExiste > 0)
        return Results.BadRequest("Já existe uma atividade com este nome para o evento.");

    connection.Execute(@"
        INSERT INTO Atividades (EventoId, Nome, Horario, HorarioFim, Tipo, Descricao, LimiteParticipantes)
        VALUES (@EventoId, @Nome, @Horario, @HorarioFim, @Tipo, @Descricao, @LimiteParticipantes)", new
    {
        atividade.EventoId,
        Nome = atividade.Nome.Trim(),
        Horario = atividade.Horario.ToString("s"),
        HorarioFim = atividade.HorarioFim.ToString("s"),
        Tipo = atividade.Tipo.Trim(),
        Descricao = (atividade.Descricao ?? string.Empty).Trim(),
        atividade.LimiteParticipantes
    });

    return Results.Ok("Atividade criada com sucesso!");
});

app.MapPost("/api/atividades/{id:int}/inscricao", (int id, AtividadeInscricaoRequest request, HttpContext httpContext) =>
{
    request.UsuarioCpf = (request.UsuarioCpf ?? string.Empty).Trim();
    request.Quantidade = Math.Clamp(request.Quantidade, 1, 2);
    var requestedSeats = NormalizeSeatList(request.Assentos);

    var accessResult = EnsureUserAccess(httpContext, request.UsuarioCpf);
    if (accessResult is not null)
        return accessResult;

    if (requestedSeats.Count != request.Quantidade)
        return Results.BadRequest("Selecione a quantidade exata de assentos para a atividade.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuarioExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE Cpf = @cpf",
        new { cpf = request.UsuarioCpf });

    if (usuarioExiste == 0)
        return Results.BadRequest("Usuário não encontrado para a inscrição.");

    using var transaction = connection.BeginTransaction();

    var atividade = connection.QueryFirstOrDefault<Atividade>(@"
        SELECT Id, EventoId, Nome, Horario, HorarioFim, Tipo, Descricao, LimiteParticipantes
        FROM Atividades
        WHERE Id = @id", new { id }, transaction);

    if (atividade is null)
        return Results.NotFound("Atividade não encontrada.");

    var quantidadeAtual = connection.ExecuteScalar<int>(@"
        SELECT COALESCE(Quantidade, 1) FROM InscricoesAtividades
        WHERE AtividadeId = @id AND UsuarioCpf = @cpf",
        new { id, cpf = request.UsuarioCpf }, transaction);

    var totalUsuarioOutrasAtividades = connection.ExecuteScalar<int>(@"
        SELECT COALESCE(SUM(COALESCE(i.Quantidade, 1)), 0)
        FROM InscricoesAtividades i
        INNER JOIN Atividades a ON a.Id = i.AtividadeId
        WHERE a.EventoId = @eventoId
          AND i.UsuarioCpf = @cpf
          AND i.AtividadeId <> @id",
        new { eventoId = atividade.EventoId, id, cpf = request.UsuarioCpf }, transaction);

    if (totalUsuarioOutrasAtividades + request.Quantidade > 2)
        return Results.BadRequest("Cada usuário pode reservar no máximo 2 vagas em atividades deste evento.");

    var inscritos = connection.ExecuteScalar<int>(
        "SELECT COALESCE(SUM(COALESCE(Quantidade, 1)), 0) FROM InscricoesAtividades WHERE AtividadeId = @id AND UsuarioCpf <> @cpf",
        new { id, cpf = request.UsuarioCpf }, transaction);

    if (inscritos + request.Quantidade > atividade.LimiteParticipantes)
        return Results.BadRequest("Atividade sem vagas disponiveis.");

    var occupiedSeats = connection.Query<string>(@"
        SELECT Assentos
        FROM InscricoesAtividades
        WHERE AtividadeId = @id AND UsuarioCpf <> @cpf AND TRIM(Assentos) <> ''",
        new { id, cpf = request.UsuarioCpf }, transaction)
        .SelectMany(NormalizeSeatList)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (requestedSeats.Any(occupiedSeats.Contains))
        return Results.BadRequest("Um ou mais assentos selecionados já foram reservados.");

    var normalizedSeats = string.Join(", ", requestedSeats);

    try
    {
        if (quantidadeAtual > 0)
        {
            connection.Execute(@"
                UPDATE InscricoesAtividades
                SET Quantidade = @quantidade,
                    Assentos = @assentos
                WHERE AtividadeId = @id AND UsuarioCpf = @cpf", new
            {
                id,
                cpf = request.UsuarioCpf,
                quantidade = request.Quantidade,
                assentos = normalizedSeats
            }, transaction);
        }
        else
        {
            connection.Execute(@"
                INSERT INTO InscricoesAtividades (AtividadeId, UsuarioCpf, Quantidade, Assentos, CriadoEm)
                VALUES (@id, @cpf, @quantidade, @assentos, @criadoEm)", new
            {
                id,
                cpf = request.UsuarioCpf,
                quantidade = request.Quantidade,
                assentos = normalizedSeats,
                criadoEm = DateTime.UtcNow.ToString("s")
            }, transaction);
        }
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        return Results.BadRequest("Não foi possível atualizar a inscrição nesta atividade.");
    }

    transaction.Commit();

    return Results.Ok(quantidadeAtual > 0
        ? "Inscrição atualizada com sucesso!"
        : "Inscrição realizada com sucesso!");
});

app.MapDelete("/api/atividades/{id:int}/inscricao/{usuarioCpf}", (int id, string usuarioCpf, HttpContext httpContext) =>
{
    var accessResult = EnsureUserAccess(httpContext, usuarioCpf);
    if (accessResult is not null)
        return accessResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var deleted = connection.Execute(@"
        DELETE FROM InscricoesAtividades
        WHERE AtividadeId = @id AND UsuarioCpf = @cpf",
        new { id, cpf = usuarioCpf });

    return deleted == 0
        ? Results.NotFound("Inscrição não encontrada.")
        : Results.Ok("Inscrição cancelada com sucesso.");
});

app.MapDelete("/api/admin/atividades/{id:int}", (int id, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    connection.Execute(
        "DELETE FROM InscricoesAtividades WHERE AtividadeId = @id",
        new { id },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM Atividades WHERE Id = @id",
        new { id },
        transaction);

    transaction.Commit();

    return deleted == 0
        ? Results.NotFound("Atividade não encontrada.")
        : Results.Ok("Atividade removida com sucesso!");
});

app.MapGet("/api/admin/cupons", (HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var cupons = connection.Query<Cupom>(
        "SELECT Codigo, PorcentagemDesconto, ValorMinimoRegra FROM Cupons ORDER BY Codigo")
        .ToList();

    return Results.Ok(cupons);
});

app.MapPost("/api/cupons", (Cupom cupom, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!AlphabitRules.IsValidCoupon(cupom))
        return Results.BadRequest("Preencha os dados do cupom corretamente.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    if (connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Cupons WHERE Codigo = @codigo", new { codigo = cupom.Codigo }) > 0)
        return Results.BadRequest("Cupom já existe.");

    connection.Execute(@"
        INSERT INTO Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra)
        VALUES (@codigo, @desconto, @valorMinimo)", new
    {
        codigo = cupom.Codigo,
        desconto = cupom.PorcentagemDesconto,
        valorMinimo = cupom.ValorMinimoRegra
    });

    return Results.Ok("Cupom criado com sucesso!");
});

app.MapPut("/api/admin/cupons/{codigo}", (string codigo, Cupom cupom, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!AlphabitRules.IsValidCoupon(cupom))
        return Results.BadRequest("Preencha os dados do cupom corretamente.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var updated = connection.Execute(@"
        UPDATE Cupons
        SET PorcentagemDesconto = @desconto,
            ValorMinimoRegra = @valorMinimo
        WHERE Codigo = @codigo", new
    {
        codigo,
        desconto = cupom.PorcentagemDesconto,
        valorMinimo = cupom.ValorMinimoRegra
    });

    if (updated == 0)
        return Results.NotFound("Cupom não encontrado.");

    return Results.Ok("Cupom atualizado com sucesso!");
});

app.MapDelete("/api/admin/cupons/{codigo}", (string codigo, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var cupomExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Cupons WHERE Codigo = @codigo",
        new { codigo },
        transaction);

    if (cupomExiste == 0)
        return Results.NotFound("Cupom não encontrado.");

    connection.Execute(
        "UPDATE Reservas SET CupomUtilizado = NULL WHERE CupomUtilizado = @codigo",
        new { codigo },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM Cupons WHERE Codigo = @codigo",
        new { codigo },
        transaction);

    transaction.Commit();

    if (deleted == 0)
        return Results.NotFound("Cupom não encontrado.");

    return Results.Ok("Cupom removido com sucesso!");
});

app.MapPost("/api/cupons/preview", (ReservaCheckoutRequest request) =>
{
    if (!AlphabitRules.IsValidCheckout(request))
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Informe o CPF do usuário e pelo menos um ingresso válido."
        });

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuarioExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE Cpf = @cpf",
        new { cpf = request.UsuarioCpf });

    if (usuarioExiste == 0)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Usuário não encontrado para calcular o desconto."
        });

    var eventoIds = request.Itens.Select(item => item.EventoId).Distinct().ToArray();
    var eventos = connection.Query<Evento>(
        "SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos WHERE Id IN @ids",
        new { ids = eventoIds }).ToList();

    if (eventos.Count != eventoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais eventos não foram encontrados."
        });

    var tipoIds = request.Itens
        .Where(item => item.TipoIngressoId.HasValue)
        .Select(item => item.TipoIngressoId!.Value)
        .Distinct()
        .ToArray();

    var tiposIngresso = tipoIds.Length == 0
        ? []
        : connection.Query<TipoIngresso>(@"
            SELECT Id, EventoId, Nome, Beneficios, Preco, QuantidadeDisponivel
            FROM TiposIngresso
            WHERE Id IN @ids",
            new { ids = tipoIds }).ToList();

    if (tiposIngresso.Count != tipoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais tipos de ingresso não foram encontrados."
        });

    Cupom? cupom = null;
    if (!string.IsNullOrWhiteSpace(request.CupomCodigo))
    {
        cupom = connection.QueryFirstOrDefault<Cupom>(
            "SELECT Codigo, PorcentagemDesconto, ValorMinimoRegra FROM Cupons WHERE lower(Codigo) = lower(@codigo)",
            new { codigo = request.CupomCodigo });

        if (cupom is null)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = "Cupom não encontrado."
            });
    }

    decimal totalOriginal = 0;
    decimal totalComDesconto = 0;

    foreach (var item in request.Itens)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var tipoIngresso = item.TipoIngressoId.HasValue
            ? tiposIngresso.First(current => current.Id == item.TipoIngressoId.Value)
            : null;

        if (tipoIngresso is not null && tipoIngresso.EventoId != evento.Id)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O tipo de ingresso {tipoIngresso.Nome} não pertence ao evento {evento.Nome}."
            });

        var precoUnitario = tipoIngresso?.Preco ?? evento.PrecoPadrao;
        var subtotal = precoUnitario * item.Quantidade;
        var subtotalComDesconto = subtotal;

        if (cupom is not null && precoUnitario >= cupom.ValorMinimoRegra)
            subtotalComDesconto = subtotal - (subtotal * (cupom.PorcentagemDesconto / 100m));

        totalOriginal += subtotal;
        totalComDesconto += subtotalComDesconto;
    }

    return Results.Ok(new ReservaCheckoutResponse
    {
        Sucesso = true,
        Mensagem = cupom is null
            ? "Resumo calculado sem cupom."
            : totalOriginal == totalComDesconto
                ? "Cupom válido, mas sem desconto para os itens atuais."
                : "Cupom aplicado com sucesso.",
        CupomAplicado = cupom?.Codigo ?? string.Empty,
        TotalOriginal = decimal.Round(totalOriginal, 2),
        DescontoAplicado = decimal.Round(totalOriginal - totalComDesconto, 2),
        TotalFinal = decimal.Round(totalComDesconto, 2)
    });
});

app.MapPost("/api/reservas", (ReservaCheckoutRequest request) =>
{
    try
    {
    if (!AlphabitRules.IsValidCheckout(request))
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Informe o CPF do usuário e pelo menos um ingresso válido."
        });

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var usuarioExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE Cpf = @cpf",
        new { cpf = request.UsuarioCpf });

    if (usuarioExiste == 0)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Usuário não encontrado para concluir a compra."
        });

    var eventoIds = request.Itens.Select(item => item.EventoId).Distinct().ToArray();
    var eventos = connection.Query<Evento>(
        "SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos WHERE Id IN @ids",
        new { ids = eventoIds }).ToList();

    if (eventos.Count != eventoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais eventos não foram encontrados."
        });

    var tipoIds = request.Itens
        .Where(item => item.TipoIngressoId.HasValue)
        .Select(item => item.TipoIngressoId!.Value)
        .Distinct()
        .ToArray();

    var tiposIngresso = tipoIds.Length == 0
        ? []
        : connection.Query<TipoIngresso>(@"
            SELECT Id, EventoId, Nome, Beneficios, Preco, QuantidadeDisponivel
            FROM TiposIngresso
            WHERE Id IN @ids",
            new { ids = tipoIds }).ToList();

    if (tiposIngresso.Count != tipoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais tipos de ingresso não foram encontrados."
        });

    var itensPorEvento = request.Itens
        .GroupBy(item => new { item.EventoId, item.TipoIngressoId })
        .Select(group => new
        {
            group.Key.EventoId,
            group.Key.TipoIngressoId,
            Quantidade = group.Sum(item => item.Quantidade),
            Assentos = group
                .SelectMany(item => item.Assentos)
                .Select(assento => assento.Trim())
                .ToList()
        })
        .ToList();

    foreach (var item in itensPorEvento)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var tipoIngresso = item.TipoIngressoId.HasValue
            ? tiposIngresso.First(current => current.Id == item.TipoIngressoId.Value)
            : null;

        if (tipoIngresso is not null && tipoIngresso.EventoId != evento.Id)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O tipo de ingresso {tipoIngresso.Nome} não pertence ao evento {evento.Nome}."
            });

        var assentosOcupados = connection.Query<string>(
            "SELECT Assentos FROM Reservas WHERE EventoId = @eventoId AND Assentos <> ''",
            new { eventoId = item.EventoId })
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assentosDuplicados = item.Assentos
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (assentosDuplicados)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"Selecione assentos unicos para o evento {evento.Nome}."
            });

        var assentoJaReservado = item.Assentos.FirstOrDefault(assento => assentosOcupados.Contains(assento));
        if (!string.IsNullOrWhiteSpace(assentoJaReservado))
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O assento {assentoJaReservado} do evento {evento.Nome} já foi reservado."
            });

        var reservasMesmoCpfNoEvento = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE UsuarioCpf = @cpf AND EventoId = @eventoId",
            new { cpf = request.UsuarioCpf, eventoId = item.EventoId });

        if (reservasMesmoCpfNoEvento + item.Quantidade > 2)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O CPF informado não pode ter mais de 2 reservas para o evento {evento.Nome}."
            });

        var ingressosReservados = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE EventoId = @eventoId",
            new { eventoId = item.EventoId });

        if (ingressosReservados + item.Quantidade > evento.CapacidadeTotal)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O evento {evento.Nome} não possui mais lugares suficientes."
            });

        if (tipoIngresso is not null)
        {
            var ingressosDoTipo = connection.ExecuteScalar<int>(
                "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE TipoIngressoId = @tipoIngressoId",
                new { tipoIngressoId = tipoIngresso.Id });

            if (ingressosDoTipo + item.Quantidade > tipoIngresso.QuantidadeDisponivel)
                return Results.BadRequest(new ReservaCheckoutResponse
                {
                    Sucesso = false,
                    Mensagem = $"O ingresso {tipoIngresso.Nome} do evento {evento.Nome} não possui mais vagas suficientes."
                });
        }
    }

    Cupom? cupom = null;
    if (!string.IsNullOrWhiteSpace(request.CupomCodigo))
    {
        cupom = connection.QueryFirstOrDefault<Cupom>(
            "SELECT Codigo, PorcentagemDesconto, ValorMinimoRegra FROM Cupons WHERE lower(Codigo) = lower(@codigo)",
            new { codigo = request.CupomCodigo });

        if (cupom is null)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = "Cupom não encontrado."
            });
    }

    decimal totalOriginal = 0;
    decimal totalComDesconto = 0;
    var reservasParaInserir = new List<Reserva>();
    var formaPagamento = NormalizePaymentMethod(request.FormaPagamento);
    var statusPagamento = DeterminePaymentStatus(formaPagamento);

    foreach (var item in itensPorEvento)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var tipoIngresso = item.TipoIngressoId.HasValue
            ? tiposIngresso.First(current => current.Id == item.TipoIngressoId.Value)
            : null;
        var precoUnitario = tipoIngresso?.Preco ?? evento.PrecoPadrao;
        var subtotal = precoUnitario * item.Quantidade;
        var subtotalComDesconto = subtotal;

        if (cupom is not null && precoUnitario >= cupom.ValorMinimoRegra)
        {
            subtotalComDesconto = subtotal - (subtotal * (cupom.PorcentagemDesconto / 100m));
        }

        totalOriginal += subtotal;
        totalComDesconto += subtotalComDesconto;

        reservasParaInserir.Add(new Reserva
        {
            UsuarioCpf = request.UsuarioCpf,
            EventoId = item.EventoId,
            TipoIngressoId = item.TipoIngressoId,
            CupomUtilizado = cupom?.Codigo,
            Assentos = string.Join(", ", item.Assentos.OrderBy(value => value)),
            Quantidade = item.Quantidade,
            PrecoUnitario = precoUnitario,
            ValorFinalPago = decimal.Round(subtotalComDesconto, 2),
            FormaPagamento = formaPagamento,
            StatusPagamento = statusPagamento,
            CodigoPedido = GenerateOrderCode(),
            DataReserva = DateTime.UtcNow
        });
    }

    using var transaction = connection.BeginTransaction();

    foreach (var item in itensPorEvento)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var tipoIngresso = item.TipoIngressoId.HasValue
            ? tiposIngresso.First(current => current.Id == item.TipoIngressoId.Value)
            : null;

        var assentosOcupados = connection.Query<string>(
            "SELECT Assentos FROM Reservas WHERE EventoId = @eventoId AND Assentos <> ''",
            new { eventoId = item.EventoId },
            transaction)
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assentoJaReservado = item.Assentos.FirstOrDefault(assento => assentosOcupados.Contains(assento));
        if (!string.IsNullOrWhiteSpace(assentoJaReservado))
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O assento {assentoJaReservado} do evento {evento.Nome} já foi reservado."
            });

        var reservasMesmoCpfNoEvento = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE UsuarioCpf = @cpf AND EventoId = @eventoId",
            new { cpf = request.UsuarioCpf, eventoId = item.EventoId },
            transaction);

        if (reservasMesmoCpfNoEvento + item.Quantidade > 2)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O CPF informado não pode ter mais de 2 reservas para o evento {evento.Nome}."
            });

        var ingressosReservados = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE EventoId = @eventoId",
            new { eventoId = item.EventoId },
            transaction);

        if (ingressosReservados + item.Quantidade > evento.CapacidadeTotal)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O evento {evento.Nome} não possui mais lugares suficientes."
            });

        if (tipoIngresso is not null)
        {
            var ingressosDoTipo = connection.ExecuteScalar<int>(
                "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE TipoIngressoId = @tipoIngressoId",
                new { tipoIngressoId = tipoIngresso.Id },
                transaction);

            if (ingressosDoTipo + item.Quantidade > tipoIngresso.QuantidadeDisponivel)
                return Results.BadRequest(new ReservaCheckoutResponse
                {
                    Sucesso = false,
                    Mensagem = $"O ingresso {tipoIngresso.Nome} do evento {evento.Nome} não possui mais vagas suficientes."
                });
        }
    }

    foreach (var reserva in reservasParaInserir)
    {
        var reservaId = connection.ExecuteScalar<long>(@"
            INSERT INTO Reservas (UsuarioCpf, EventoId, TipoIngressoId, CupomUtilizado, Assentos, Quantidade, PrecoUnitario, ValorFinalPago, FormaPagamento, StatusPagamento, CodigoPedido, DataReserva)
            VALUES (@UsuarioCpf, @EventoId, @TipoIngressoId, @CupomUtilizado, @Assentos, @Quantidade, @PrecoUnitario, @ValorFinalPago, @FormaPagamento, @StatusPagamento, @CodigoPedido, @DataReserva);
            SELECT last_insert_rowid();",
            new
            {
                reserva.UsuarioCpf,
                reserva.EventoId,
                reserva.TipoIngressoId,
                reserva.CupomUtilizado,
                reserva.Assentos,
                reserva.Quantidade,
                reserva.PrecoUnitario,
                reserva.ValorFinalPago,
                reserva.FormaPagamento,
                reserva.StatusPagamento,
                reserva.CodigoPedido,
                DataReserva = reserva.DataReserva.ToString("s")
            }, transaction);

        connection.Execute(@"
            INSERT INTO Checkins (ReservaId, QrCode, DataCheckin, Status)
            VALUES (@reservaId, @qrCode, NULL, 'Pendente')",
            new { reservaId, qrCode = GenerateQrCode() },
            transaction);
    }

    transaction.Commit();

    return Results.Ok(new ReservaCheckoutResponse
    {
        Sucesso = true,
        Mensagem = "Compra concluida com sucesso.",
        CupomAplicado = cupom?.Codigo ?? string.Empty,
        TotalOriginal = decimal.Round(totalOriginal, 2),
        DescontoAplicado = decimal.Round(totalOriginal - totalComDesconto, 2),
        TotalFinal = decimal.Round(totalComDesconto, 2)
    });
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Não foi possível concluir a compra com os dados atuais. Revise o cupom e os assentos selecionados."
        });
    }
    catch
    {
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Não foi possível concluir a compra agora. Tente novamente em instantes."
        });
    }
});

app.MapGet("/api/reservas/{cpf}", (string cpf, HttpContext httpContext) =>
{
    var accessResult = EnsureUserAccess(httpContext, cpf);
    if (accessResult is not null)
        return accessResult;

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var reservas = connection.Query<ReservaResumoResponse>(@"
        SELECT
            r.Id,
            r.UsuarioCpf,
            r.EventoId,
            e.Nome AS EventoNome,
            e.CidadeEvento AS EventoCidade,
            r.TipoIngressoId,
            COALESCE(t.Nome, 'Normal') AS TipoIngressoNome,
            COALESCE(r.CupomUtilizado, '') AS CupomUtilizado,
            r.Assentos,
            r.Quantidade,
            r.PrecoUnitario,
            r.ValorFinalPago,
            COALESCE(r.FormaPagamento, 'Pix') AS FormaPagamento,
            COALESCE(r.StatusPagamento, 'Pago') AS StatusPagamento,
            COALESCE(r.CodigoPedido, '') AS CodigoPedido,
            COALESCE(c.QrCode, '') AS QrCode,
            COALESCE(c.Status, 'Pendente') AS CheckinStatus,
            c.DataCheckin,
            r.DataReserva,
            CASE
                WHEN av.Id IS NULL THEN 0
                ELSE 1
            END AS JaAvaliado
        FROM Reservas r
        INNER JOIN Eventos e ON e.Id = r.EventoId
        LEFT JOIN TiposIngresso t ON t.Id = r.TipoIngressoId
        LEFT JOIN Checkins c ON c.ReservaId = r.Id
        LEFT JOIN Avaliacoes av ON av.EventoId = r.EventoId AND av.UsuarioCpf = r.UsuarioCpf
        WHERE r.UsuarioCpf = @cpf
        ORDER BY r.DataReserva DESC", new { cpf }).ToList();

    foreach (var reserva in reservas)
    {
        reserva.DataReserva = ConvertUtcLikeToBrasília(reserva.DataReserva);
        if (reserva.DataCheckin is not null)
            reserva.DataCheckin = ConvertUtcLikeToBrasília(reserva.DataCheckin.Value);
    }

    return Results.Ok(reservas);
});

app.MapPost("/api/checkin", (CheckinRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (string.IsNullOrWhiteSpace(request.QrCode))
        return Results.BadRequest(new CheckinResponse
        {
            Sucesso = false,
            Mensagem = "Informe o QR Code para validar o check-in."
        });

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var checkin = connection.QueryFirstOrDefault<(int Id, int ReservaId, string QrCode, string Status, DateTime? DataCheckin, string StatusPagamento, string EventoNome, string ClienteCpf)>(@"
        SELECT
            c.Id,
            c.ReservaId,
            c.QrCode,
            c.Status,
            c.DataCheckin,
            r.StatusPagamento,
            e.Nome AS EventoNome,
            r.UsuarioCpf AS ClienteCpf
        FROM Checkins c
        INNER JOIN Reservas r ON r.Id = c.ReservaId
        INNER JOIN Eventos e ON e.Id = r.EventoId
        WHERE c.QrCode = @qrCode",
        new { qrCode = request.QrCode.Trim() },
        transaction);

    if (checkin.Id == 0)
        return Results.NotFound(new CheckinResponse
        {
            Sucesso = false,
            Mensagem = "QR Code inválido."
        });

    if (string.Equals(checkin.StatusPagamento, "Cancelado", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(checkin.StatusPagamento, "Cancelada", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new CheckinResponse
        {
            Sucesso = false,
            Mensagem = "Reserva cancelada não pode fazer check-in.",
            ReservaId = checkin.ReservaId,
            EventoNome = checkin.EventoNome,
            ClienteCpf = checkin.ClienteCpf,
            Status = checkin.Status,
            DataCheckin = checkin.DataCheckin
        });

    if (string.Equals(checkin.Status, "Usado", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new CheckinResponse
        {
            Sucesso = false,
            Mensagem = "Check-in já realizado para esta reserva.",
            ReservaId = checkin.ReservaId,
            EventoNome = checkin.EventoNome,
            ClienteCpf = checkin.ClienteCpf,
            Status = checkin.Status,
            DataCheckin = checkin.DataCheckin
        });

    var checkedAt = DateTime.UtcNow;
    var updatedRows = connection.Execute(@"
        UPDATE Checkins
        SET Status = 'Usado',
            DataCheckin = @dataCheckin
        WHERE Id = @id
          AND Status <> 'Usado'",
        new { id = checkin.Id, dataCheckin = checkedAt.ToString("s") },
        transaction);

    if (updatedRows == 0)
        return Results.BadRequest(new CheckinResponse
        {
            Sucesso = false,
            Mensagem = "Check-in já realizado para esta reserva.",
            ReservaId = checkin.ReservaId,
            EventoNome = checkin.EventoNome,
            ClienteCpf = checkin.ClienteCpf,
            Status = "Usado",
            DataCheckin = checkin.DataCheckin
        });

    transaction.Commit();

    return Results.Ok(new CheckinResponse
    {
        Sucesso = true,
        Mensagem = "Check-in realizado com sucesso.",
        ReservaId = checkin.ReservaId,
        EventoNome = checkin.EventoNome,
        ClienteCpf = checkin.ClienteCpf,
        Status = "Usado",
        DataCheckin = ConvertUtcLikeToBrasília(checkedAt)
    });
});

app.MapGet("/api/admin/vendas/dashboard", (HttpContext httpContext, int? comprasLimit, int? comprasOffset) =>
{
    var accessResult = EnsureAdminAccess(httpContext);
    if (accessResult is not null)
        return accessResult;

    var stopwatch = Stopwatch.StartNew();
    var dashboard = BuildAdminSalesDashboard(connectionString, comprasLimit, comprasOffset);
    app.Logger.LogInformation("Dashboard admin gerado em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

    return Results.Ok(dashboard);
});

app.MapGet("/api/dashboard", (HttpContext httpContext, int? comprasLimit, int? comprasOffset) =>
{
    var accessResult = EnsureAdminAccess(httpContext);
    if (accessResult is not null)
        return accessResult;

    var stopwatch = Stopwatch.StartNew();
    var dashboard = BuildAdminSalesDashboard(connectionString, comprasLimit, comprasOffset);
    app.Logger.LogInformation("Dashboard legado gerado em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

    return Results.Ok(dashboard);
});

app.Run();

IResult? EnsureAdminAccess(HttpContext httpContext)
{
    if (!httpContext.Request.Headers.TryGetValue("X-Admin-Token", out var token) || token != adminToken)
        return Results.Unauthorized();

    return null;
}

IResult? EnsureUserAccess(HttpContext httpContext, string routeCpf)
{
    if (httpContext.Request.Headers.TryGetValue("X-Admin-Token", out var adminHeader) &&
        adminHeader == adminToken)
    {
        return null;
    }

    if (!httpContext.Request.Headers.TryGetValue("X-User-Cpf", out var userCpf) ||
        string.IsNullOrWhiteSpace(userCpf) ||
        !string.Equals(userCpf.ToString().Trim(), routeCpf.Trim(), StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    return null;
}

static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string sqlDefinition)
{
    if (!IsKnownSchemaIdentifier(tableName) || !IsKnownSchemaIdentifier(columnName))
        throw new InvalidOperationException("Identificador de schema inválido.");

    var pragmaCmd = connection.CreateCommand();
    pragmaCmd.CommandText = $"PRAGMA table_info({tableName})";

    using var reader = pragmaCmd.ExecuteReader();
    while (reader.Read())
    {
        if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
            return;
    }

    var alterCmd = connection.CreateCommand();
    alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlDefinition}";
    alterCmd.ExecuteNonQuery();
}

static void EnsureReservasSchema(SqliteConnection connection)
{
    EnsureColumnExists(connection, "Reservas", "Quantidade", "INTEGER NOT NULL DEFAULT 1");
    EnsureColumnExists(connection, "Reservas", "PrecoUnitario", "REAL NOT NULL DEFAULT 0");
    EnsureColumnExists(connection, "Reservas", "DataReserva", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Reservas", "Assentos", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Reservas", "FormaPagamento", "TEXT NOT NULL DEFAULT 'Pix'");
    EnsureColumnExists(connection, "Reservas", "StatusPagamento", "TEXT NOT NULL DEFAULT 'Pago'");
    EnsureColumnExists(connection, "Reservas", "CodigoPedido", "TEXT NOT NULL DEFAULT ''");

    var foreignKeys = connection.Query<(int Id, int Seq, string Table, string From, string To, string OnUpdate, string OnDelete, string Match)>(
        "PRAGMA foreign_key_list(Reservas)");

    var hasUsuarioFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "UsuarioCpf", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Usuarios", StringComparison.OrdinalIgnoreCase));

    var hasEventoFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "EventoId", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Eventos", StringComparison.OrdinalIgnoreCase));

    var hasTipoIngressoFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "TipoIngressoId", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "TiposIngresso", StringComparison.OrdinalIgnoreCase));

    var hasCupomFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "CupomUtilizado", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Cupons", StringComparison.OrdinalIgnoreCase));

    if (hasUsuarioFk && hasEventoFk && hasTipoIngressoFk && hasCupomFk)
        return;

    using var transaction = connection.BeginTransaction();

    connection.Execute("PRAGMA foreign_keys = OFF;", transaction: transaction);

    connection.Execute(@"
        CREATE TABLE Reservas_new (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf TEXT NOT NULL,
            EventoId INTEGER NOT NULL,
            TipoIngressoId INTEGER NULL,
            CupomUtilizado TEXT NULL,
            Assentos TEXT NOT NULL DEFAULT '',
            Quantidade INTEGER NOT NULL DEFAULT 1,
            PrecoUnitario REAL NOT NULL DEFAULT 0,
            ValorFinalPago REAL NOT NULL,
            FormaPagamento TEXT NOT NULL DEFAULT 'Pix',
            StatusPagamento TEXT NOT NULL DEFAULT 'Pago',
            CodigoPedido TEXT NOT NULL DEFAULT '',
            DataReserva TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
            FOREIGN KEY (TipoIngressoId) REFERENCES TiposIngresso(Id) ON DELETE SET NULL,
            FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo) ON DELETE SET NULL
        );", transaction: transaction);

    connection.Execute(@"
        INSERT INTO Reservas_new (Id, UsuarioCpf, EventoId, TipoIngressoId, CupomUtilizado, Assentos, Quantidade, PrecoUnitario, ValorFinalPago, FormaPagamento, StatusPagamento, CodigoPedido, DataReserva)
        SELECT
            Id,
            UsuarioCpf,
            EventoId,
            TipoIngressoId,
            NULLIF(CupomUtilizado, ''),
            COALESCE(Assentos, ''),
            COALESCE(Quantidade, 1),
            COALESCE(PrecoUnitario, 0),
            ValorFinalPago,
            COALESCE(NULLIF(FormaPagamento, ''), 'Pix'),
            COALESCE(NULLIF(StatusPagamento, ''), 'Pago'),
            COALESCE(CodigoPedido, ''),
            COALESCE(DataReserva, '')
        FROM Reservas;", transaction: transaction);

    connection.Execute("DROP TABLE Reservas;", transaction: transaction);
    connection.Execute("ALTER TABLE Reservas_new RENAME TO Reservas;", transaction: transaction);
    connection.Execute("PRAGMA foreign_keys = ON;", transaction: transaction);

    transaction.Commit();
}

static void EnsureMusicGenresCatalog(SqliteConnection connection)
{
    var defaultGenres = new[]
    {
        "Anime",
        "Games",
        "Cosplay",
        "K-pop",
        "Card Game",
        "RPG",
        "HQ",
        "Tecnologia",
        "Retro Gaming"
    };

    foreach (var genre in defaultGenres)
    {
        connection.Execute(
            "INSERT OR IGNORE INTO GenerosMusicais (Nome) VALUES (@nome)",
            new { nome = genre });
    }

    var existingGenres = connection.Query<string>(
        "SELECT DISTINCT trim(GeneroMusical) FROM Eventos WHERE trim(GeneroMusical) <> ''");

    foreach (var genre in existingGenres)
    {
        connection.Execute(
            "INSERT OR IGNORE INTO GenerosMusicais (Nome) VALUES (@nome)",
            new { nome = genre.Trim() });
    }
}

static void EnsureCitiesCatalog(SqliteConnection connection)
{
    var defaultCities = new[]
    {
        "Sao Paulo",
        "Rio de Janeiro",
        "Belo Horizonte",
        "Curitiba",
        "Porto Alegre",
        "Salvador"
    };

    foreach (var city in defaultCities)
    {
        connection.Execute(
            "INSERT OR IGNORE INTO CidadesEventos (Nome) VALUES (@nome)",
            new { nome = city });
    }

    var existingCities = connection.Query<string>(
        "SELECT DISTINCT trim(CidadeEvento) FROM Eventos WHERE trim(CidadeEvento) <> ''");

    foreach (var city in existingCities)
    {
        connection.Execute(
            "INSERT OR IGNORE INTO CidadesEventos (Nome) VALUES (@nome)",
            new { nome = city.Trim() });
    }
}

static void EnsureDemoEvents(SqliteConnection connection)
{
    var demoEvents = new[]
    {
        new Evento
        {
            Nome = "Anime Friends Experience",
            LocalEvento = "Distrito Anhembi",
            CidadeEvento = "Sao Paulo",
            Artista = "Cosplayers convidados e dubladores",
            GeneroMusical = "Anime",
            CapacidadeTotal = 18000,
            DataEvento = new DateTime(2026, 6, 20, 17, 0, 0),
            PrecoPadrao = 249.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "BGS Arena Weekend",
            LocalEvento = "Riocentro",
            CidadeEvento = "Rio de Janeiro",
            Artista = "Streamers e pro players",
            GeneroMusical = "Games",
            CapacidadeTotal = 9500,
            DataEvento = new DateTime(2026, 7, 12, 21, 0, 0),
            PrecoPadrao = 189.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Cosplay Summit Brasil",
            LocalEvento = "Espaco Unimed",
            CidadeEvento = "Sao Paulo",
            Artista = "Jurados internacionais",
            GeneroMusical = "Cosplay",
            CapacidadeTotal = 12000,
            DataEvento = new DateTime(2026, 8, 8, 20, 30, 0),
            PrecoPadrao = 219.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Pokemon TCG Championship",
            LocalEvento = "Vivo Rio",
            CidadeEvento = "Rio de Janeiro",
            Artista = "Treinadores e mesas oficiais",
            GeneroMusical = "Card Game",
            CapacidadeTotal = 6000,
            DataEvento = new DateTime(2026, 9, 5, 19, 30, 0),
            PrecoPadrao = 159.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1613771404721-1f92d799e49f?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "K-pop Fan Festival",
            LocalEvento = "Mineirao Hall",
            CidadeEvento = "Belo Horizonte",
            Artista = "Dance crews e lojas temáticas",
            GeneroMusical = "K-pop",
            CapacidadeTotal = 14000,
            DataEvento = new DateTime(2026, 10, 17, 22, 0, 0),
            PrecoPadrao = 279.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Retro Gaming Fair",
            LocalEvento = "Teatro Bradesco",
            CidadeEvento = "Sao Paulo",
            Artista = "Colecionadores e arcades classicos",
            GeneroMusical = "Retro Gaming",
            CapacidadeTotal = 4200,
            DataEvento = new DateTime(2026, 11, 7, 20, 0, 0),
            PrecoPadrao = 174.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=1200&q=80"
        }
    };

    var legacyDemoNames = new[]
    {
        "Festival Sunset Brasil",
        "Noite do Rock Arena",
        "Pop Experience Live",
        "Samba Prime Experience",
        "Electronic Lights",
        "Turne Acustica Brasil"
    };

    for (var index = 0; index < demoEvents.Length; index++)
    {
        var evento = demoEvents[index];
        var legacyName = legacyDemoNames[index];
        var exists = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Eventos WHERE Nome = @nome OR Nome = @legacyName",
            new { nome = evento.Nome, legacyName });

        if (exists > 0)
        {
            connection.Execute(@"
                UPDATE Eventos
                SET Nome = @nome,
                    LocalEvento = @local,
                    CidadeEvento = @cidade,
                    Artista = @artista,
                    GeneroMusical = @genero,
                    CapacidadeTotal = @capacidade,
                    DataEvento = @data,
                    PrecoPadrao = @preco,
                    ImagemUrl = @imagem
                WHERE Nome = @nome OR Nome = @legacyName", new
            {
                nome = evento.Nome,
                legacyName,
                local = evento.LocalEvento,
                cidade = evento.CidadeEvento,
                artista = evento.Artista,
                genero = evento.GeneroMusical,
                capacidade = evento.CapacidadeTotal,
                data = evento.DataEvento.ToString("s"),
                preco = evento.PrecoPadrao,
                imagem = evento.ImagemUrl
            });
            continue;
        }

        connection.Execute(@"
            INSERT INTO Eventos (Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl)
            VALUES (@nome, @local, @cidade, @artista, @genero, @capacidade, @data, @preco, @imagem)", new
        {
            nome = evento.Nome,
            local = evento.LocalEvento,
            cidade = evento.CidadeEvento,
            artista = evento.Artista,
            genero = evento.GeneroMusical,
            capacidade = evento.CapacidadeTotal,
            data = evento.DataEvento.ToString("s"),
            preco = evento.PrecoPadrao,
            imagem = evento.ImagemUrl
        });
    }
}

static void EnsureTicketTypesCatalog(SqliteConnection connection)
{
    var eventos = connection.Query<Evento>(
        "SELECT Id, Nome, PrecoPadrao, CapacidadeTotal FROM Eventos").ToList();

    foreach (var evento in eventos)
    {
        EnsureDefaultTicketTypesForEvent(connection, evento.Id, evento.PrecoPadrao, evento.CapacidadeTotal);
    }
}

static void EnsureDemoActivities(SqliteConnection connection)
{
    var eventos = connection.Query<(int Id, string Nome, string DataEvento)>(
        "SELECT Id, Nome, DataEvento FROM Eventos").ToList();

    foreach (var evento in eventos)
    {
        var eventDate = DateTime.TryParse(evento.DataEvento, out var parsedDate)
            ? parsedDate
            : DateTime.UtcNow.Date.AddHours(18);

        EnsureDefaultActivitiesForEvent(connection, evento.Id, evento.Nome, eventDate);
    }
}

static void EnsureDefaultActivitiesForEvent(
    SqliteConnection connection,
    int eventId,
    string eventName,
    DateTime eventDate)
{
    var atividades = GetDefaultActivitiesForEvent(eventName, eventId, eventDate);

    foreach (var atividade in atividades)
    {
        connection.Execute(@"
            INSERT INTO Atividades (EventoId, Nome, Horario, HorarioFim, Tipo, Descricao, LimiteParticipantes)
            SELECT @EventoId, @Nome, @Horario, @HorarioFim, @Tipo, @Descricao, @LimiteParticipantes
            WHERE NOT EXISTS (
                SELECT 1 FROM Atividades
                WHERE EventoId = @EventoId AND lower(Nome) = lower(@Nome)
            )", new
        {
            atividade.EventoId,
            atividade.Nome,
            Horario = atividade.Horario.ToString("s"),
            HorarioFim = atividade.HorarioFim.ToString("s"),
            atividade.Tipo,
            Descricao = atividade.Descricao,
            atividade.LimiteParticipantes
        });
    }
}

static Atividade[] GetDefaultActivitiesForEvent(string eventName, int eventId, DateTime eventDate)
{
    var category = eventName.ToLowerInvariant();

    if (category.Contains("pokemon") || category.Contains("tcg"))
    {
        return
        [
            BuildActivity(eventId, "Torneio relampago TCG", eventDate.AddHours(1), "torneio", 64),
            BuildActivity(eventId, "Workshop de deck building", eventDate.AddHours(2), "workshop", 40)
        ];
    }

    if (category.Contains("cosplay"))
    {
        return
        [
            BuildActivity(eventId, "Concurso de cosplay", eventDate.AddHours(1), "concurso de cosplay", 80),
            BuildActivity(eventId, "Meet and greet com jurados", eventDate.AddHours(3), "meet and greet", 30)
        ];
    }

    if (category.Contains("k-pop"))
    {
        return
        [
            BuildActivity(eventId, "Karaoke fandom stage", eventDate.AddHours(1), "karaoke", 50),
            BuildActivity(eventId, "Workshop de coreografia", eventDate.AddHours(2), "workshop", 45)
        ];
    }

    return
    [
        BuildActivity(eventId, "Painel de convidados", eventDate.AddHours(1), "workshop", 60),
        BuildActivity(eventId, "Encontro da comunidade", eventDate.AddHours(2), "meet and greet", 35)
    ];
}

static Atividade BuildActivity(int eventId, string name, DateTime schedule, string type, int participantLimit)
{
    return new Atividade
    {
        EventoId = eventId,
        Nome = name,
        Horario = schedule,
        HorarioFim = schedule.AddHours(1),
        Tipo = type,
        Descricao = string.Empty,
        LimiteParticipantes = participantLimit
    };
}

static bool IsValidActivity(Atividade atividade)
{
    return atividade.EventoId > 0 &&
           !string.IsNullOrWhiteSpace(atividade.Nome) &&
           !string.IsNullOrWhiteSpace(atividade.Tipo) &&
           atividade.Horario > DateTime.MinValue &&
           atividade.HorarioFim >= atividade.Horario &&
           atividade.LimiteParticipantes > 0;
}

static List<string> NormalizeSeatList(string? seats)
{
    return (seats ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(seat => seat.ToUpperInvariant())
        .Where(seat => seat.Length is > 0 and <= 8)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(seat => seat, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

static void EnsureDemoStands(SqliteConnection connection)
{
    var eventIds = connection.Query<int>("SELECT Id FROM Eventos").ToList();

    foreach (var eventId in eventIds)
    {
        EnsureDefaultStandSectorsForEvent(connection, eventId);
    }
}

static void EnsureDefaultStandSectorsForEvent(SqliteConnection connection, int eventId)
{
    var existingSectors = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM StandSetores WHERE EventoId = @eventId",
        new { eventId });

    if (existingSectors > 0)
        return;

    var sectors = new[] { "Linha Azul", "Linha Vermelha", "Linha Verde", "Linha Amarela" };

    foreach (var sector in sectors)
    {
        connection.Execute(@"
            INSERT OR IGNORE INTO StandSetores (EventoId, Nome)
            VALUES (@eventId, @sector)",
            new { eventId, sector });
    }
}

static List<StandEspaco> GetStandsForEvent(SqliteConnection connection, int eventId)
{
    return connection.Query<StandEspaco>(@"
        SELECT
            s.Id,
            s.EventoId,
            s.Setor,
            s.Codigo,
            s.PosicaoX,
            s.PosicaoY,
            s.Largura,
            s.Altura,
            s.TipoArea,
            s.AreaX,
            s.AreaY,
            s.AreaLargura,
            s.AreaAltura,
            s.AreaMetrosQuadrados,
            s.PrecoPorMetroQuadrado,
            s.PrecoFixo,
            CAST(s.Reservado AS INTEGER) AS Reservado,
            s.NomeOcupante,
            s.TipoOcupante,
            s.Descricao,
            e.MapaImagemUrl
        FROM StandsEspacos s
        INNER JOIN Eventos e ON e.Id = s.EventoId
        WHERE s.EventoId = @eventId
        ORDER BY PosicaoY, PosicaoX, Codigo",
        new { eventId }).ToList();
}

static void EnsureDemoGuests(SqliteConnection connection)
{
    var convidados = new[]
    {
        new Convidado
        {
            Nome = "Akira Cosplay",
            Tipo = "Cosplayer",
            Bio = "Cosplayer convidada para paineis, fotos e concurso de figurinos.",
            FotoUrl = "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=800&q=80"
        },
        new Convidado
        {
            Nome = "NerdPlayer BR",
            Tipo = "Streamer",
            Bio = "Streamer de games e cultura pop com participação em arenas competitivas.",
            FotoUrl = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=800&q=80"
        },
        new Convidado
        {
            Nome = "Vozes do Anime",
            Tipo = "Voice Actor",
            Bio = "Painel especial com dublagem, bastidores e encontro com fas.",
            FotoUrl = "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?auto=format&fit=crop&w=800&q=80"
        },
        new Convidado
        {
            Nome = "PixelWave",
            Tipo = "Banda",
            Bio = "Banda geek com repertorio de anime songs, games e nostalgia.",
            FotoUrl = "https://images.unsplash.com/photo-1516280440614-37939bbacd81?auto=format&fit=crop&w=800&q=80"
        }
    };

    foreach (var convidado in convidados)
    {
        connection.Execute(@"
            INSERT OR IGNORE INTO Convidados (Nome, Tipo, Bio, FotoUrl)
            VALUES (@Nome, @Tipo, @Bio, @FotoUrl)", convidado);
    }

    var eventos = connection.Query<(int Id, string GeneroMusical)>(
        "SELECT Id, GeneroMusical FROM Eventos").ToList();

    foreach (var evento in eventos)
    {
        var convidadosDoEvento = SelectDemoGuestNames(evento.GeneroMusical);
        foreach (var nome in convidadosDoEvento)
        {
            var convidadoId = connection.ExecuteScalar<int>(
                "SELECT Id FROM Convidados WHERE Nome = @nome",
                new { nome });

            if (convidadoId <= 0)
                continue;

            connection.Execute(@"
                INSERT OR IGNORE INTO EventoConvidados (EventoId, ConvidadoId)
                VALUES (@eventoId, @convidadoId)",
                new { eventoId = evento.Id, convidadoId });
        }
    }
}

static string[] SelectDemoGuestNames(string category)
{
    var normalized = category.ToLowerInvariant();

    if (normalized.Contains("games") || normalized.Contains("card") || normalized.Contains("retro"))
        return ["NerdPlayer BR", "PixelWave"];

    if (normalized.Contains("cosplay"))
        return ["Akira Cosplay", "Vozes do Anime"];

    if (normalized.Contains("k-pop"))
        return ["PixelWave", "Akira Cosplay"];

    return ["Akira Cosplay", "Vozes do Anime"];
}

static bool IsValidGuest(Convidado convidado)
{
    return !string.IsNullOrWhiteSpace(convidado.Nome) &&
           !string.IsNullOrWhiteSpace(convidado.Tipo);
}

static void EnsureDefaultTicketTypesForEvent(
    SqliteConnection connection,
    int eventId,
    decimal basePrice,
    int eventCapacity)
{
    var tipos = new[]
    {
        new TipoIngresso
        {
            EventoId = eventId,
            Nome = "Normal",
            Beneficios = "Acesso geral ao evento",
            Preco = basePrice,
            QuantidadeDisponivel = Math.Max(eventCapacity, 1)
        },
        new TipoIngresso
        {
            EventoId = eventId,
            Nome = "VIP",
            Beneficios = "Entrada preferencial e área próxima ao palco",
            Preco = decimal.Round(basePrice * 1.6m, 2),
            QuantidadeDisponivel = Math.Max(eventCapacity / 5, 20)
        },
        new TipoIngresso
        {
            EventoId = eventId,
            Nome = "Meet and Greet",
            Beneficios = "Encontro rapido com convidados selecionados",
            Preco = decimal.Round(basePrice * 2.4m, 2),
            QuantidadeDisponivel = Math.Max(eventCapacity / 20, 10)
        }
    };

    foreach (var tipo in tipos)
    {
        connection.Execute(@"
            INSERT OR IGNORE INTO TiposIngresso (EventoId, Nome, Beneficios, Preco, QuantidadeDisponivel)
            VALUES (@EventoId, @Nome, @Beneficios, @Preco, @QuantidadeDisponivel)", tipo);

        connection.Execute(@"
            UPDATE TiposIngresso
            SET Beneficios = @Beneficios,
                Preco = @Preco,
                QuantidadeDisponivel = @QuantidadeDisponivel
            WHERE EventoId = @EventoId AND lower(Nome) = lower(@Nome)", tipo);
    }
}

static string GetRequiredConfiguration(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"A configuração obrigatoria '{key}' não foi encontrada.");

    return value;
}

static bool CanSendPasswordResetEmail(
    string? emailApiBaseUrl,
    string? emailApiKey,
    string? emailApiSenderEmail,
    string? smtpHost,
    string? smtpUser,
    string? smtpPassword)
{
    return CanSendPasswordResetEmailApi(emailApiBaseUrl, emailApiKey, emailApiSenderEmail) ||
           CanSendPasswordResetEmailSmtp(smtpHost, smtpUser, smtpPassword);
}

static bool CanSendPasswordResetEmailApi(string? emailApiBaseUrl, string? emailApiKey, string? emailApiSenderEmail)
{
    return !string.IsNullOrWhiteSpace(emailApiBaseUrl) &&
           !string.IsNullOrWhiteSpace(emailApiKey) &&
           !string.IsNullOrWhiteSpace(emailApiSenderEmail);
}

static bool CanSendPasswordResetEmailSmtp(string? smtpHost, string? smtpUser, string? smtpPassword)
{
    return !string.IsNullOrWhiteSpace(smtpHost) &&
           !string.IsNullOrWhiteSpace(smtpUser) &&
           !string.IsNullOrWhiteSpace(smtpPassword);
}

static string GenerateResetCode()
{
    return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
}

static string NormalizePaymentMethod(string? paymentMethod)
{
    var normalized = paymentMethod?.Trim().ToLowerInvariant();

    return normalized switch
    {
        "cartão" or "cartão" => "Cartão",
        "boleto" => "Boleto",
        _ => "Pix"
    };
}

static string DeterminePaymentStatus(string paymentMethod)
{
    return string.Equals(paymentMethod, "Boleto", StringComparison.OrdinalIgnoreCase)
        ? "Pendente"
        : "Pago";
}

static string GenerateOrderCode()
{
    return $"ALP-{DateTime.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(100000, 1000000)}";
}

static string GenerateQrCode()
{
    Span<byte> bytes = stackalloc byte[16];
    RandomNumberGenerator.Fill(bytes);
    return $"ALP-QR-{Convert.ToHexString(bytes)}";
}

static AdminSalesDashboardResponse BuildAdminSalesDashboard(string connectionString, int? comprasLimit = null, int? comprasOffset = null)
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var limit = NormalizeDashboardLimit(comprasLimit);
    var offset = NormalizeDashboardOffset(comprasOffset);

    var compras = connection.Query<AdminSaleResponse>(@"
        SELECT
            r.Id,
            u.Nome AS Cliente,
            u.Email AS ClienteEmail,
            r.UsuarioCpf AS ClienteCpf,
            e.Nome AS Evento,
            r.DataReserva AS DataCompra,
            COALESCE(NULLIF(r.FormaPagamento, ''), 'Pix') AS FormaPagamento,
            COALESCE(NULLIF(r.StatusPagamento, ''), 'Pago') AS StatusPagamento,
            r.ValorFinalPago AS ValorPago,
            r.Quantidade AS QuantidadeIngressos,
            COALESCE(t.Nome, 'Normal') || ' - ' ||
                CASE
                    WHEN trim(COALESCE(r.Assentos, '')) <> '' THEN r.Assentos
                    ELSE 'Lote unico'
                END AS SetorCadeiraLote,
            COALESCE(r.CodigoPedido, '') AS CodigoPedido
        FROM Reservas r
        INNER JOIN Usuarios u ON u.Cpf = r.UsuarioCpf
        INNER JOIN Eventos e ON e.Id = r.EventoId
        LEFT JOIN TiposIngresso t ON t.Id = r.TipoIngressoId
        ORDER BY r.DataReserva DESC
        LIMIT @limit OFFSET @offset",
        new { limit, offset }).ToList();

    foreach (var compra in compras)
    {
        compra.DataCompra = ConvertUtcLikeToBrasília(compra.DataCompra);
    }

    var hoje = ConvertUtcLikeToBrasília(DateTime.UtcNow).Date;
    var totalVendidoHoje = connection.ExecuteScalar<decimal>(@"
        SELECT COALESCE(SUM(ValorFinalPago), 0)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') = 'Pago' COLLATE NOCASE
          AND date(DataReserva) = @hoje",
        new { hoje = hoje.ToString("yyyy-MM-dd") });

    var receitaTotal = connection.ExecuteScalar<decimal>(@"
        SELECT COALESCE(SUM(ValorFinalPago), 0)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') = 'Pago' COLLATE NOCASE");
    var valorPendente = connection.ExecuteScalar<decimal>(@"
        SELECT COALESCE(SUM(ValorFinalPago), 0)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') = 'Pendente' COLLATE NOCASE");
    var totalIngressosVendidos = connection.ExecuteScalar<int>(@"
        SELECT COALESCE(SUM(Quantidade), 0)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')");
    var totalEventos = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Eventos");
    var capacidadeTotal = connection.ExecuteScalar<int>("SELECT COALESCE(SUM(CapacidadeTotal), 0) FROM Eventos");
    var checkinsRealizados = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Checkins c
        INNER JOIN Reservas r ON r.Id = c.ReservaId
        WHERE c.Status = 'Usado'
          AND COALESCE(NULLIF(r.StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')");
    var cuponsUtilizados = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE CupomUtilizado IS NOT NULL
          AND trim(CupomUtilizado) <> ''
          AND COALESCE(NULLIF(StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')");
    var totalCompras = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Reservas");
    var reservasPagas = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') = 'Pago' COLLATE NOCASE");
    var reservasPendentesPagamento = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') = 'Pendente' COLLATE NOCASE");
    var reservasCanceladas = connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')");
    var totalAvaliacoes = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Avaliacoes");
    var mediaAvaliacoes = connection.ExecuteScalar<decimal>("SELECT COALESCE(AVG(Nota), 0) FROM Avaliacoes");
    var capacidadeRestante = Math.Max(0, capacidadeTotal - totalIngressosVendidos);
    var taxaOcupacao = capacidadeTotal > 0
        ? decimal.Round((decimal)totalIngressosVendidos / capacidadeTotal * 100, 2)
        : 0;
    var taxaCheckin = totalIngressosVendidos > 0
        ? decimal.Round((decimal)checkinsRealizados / totalIngressosVendidos * 100, 2)
        : 0;

    var pagamentosPix = CountPaymentMethod(connection, "Pix");
    var pagamentosCartao = CountPaymentMethod(connection, "Cartão");
    var pagamentosBoleto = CountPaymentMethod(connection, "Boleto");

    var eventosMaisVendidos = connection.Query<AdminTopEventResponse>(@"
        SELECT
            e.Nome AS Evento,
            COALESCE(SUM(r.Quantidade), 0) AS IngressosVendidos,
            COALESCE(SUM(r.ValorFinalPago), 0) AS ValorArrecadado
        FROM Reservas r
        INNER JOIN Eventos e ON e.Id = r.EventoId
        WHERE COALESCE(NULLIF(r.StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')
        GROUP BY e.Nome
        ORDER BY IngressosVendidos DESC, ValorArrecadado DESC
        LIMIT 5").ToList();

    var ultimasAvaliacoes = connection.Query<AdminReviewResponse>(@"
        SELECT
            a.Id,
            a.EventoId,
            e.Nome AS Evento,
            u.Nome AS UsuarioNome,
            a.UsuarioCpf,
            a.Nota,
            a.Comentario,
            a.CriadoEm
        FROM Avaliacoes a
        INNER JOIN Eventos e ON e.Id = a.EventoId
        INNER JOIN Usuarios u ON u.Cpf = a.UsuarioCpf
        ORDER BY a.CriadoEm DESC
        LIMIT 8").ToList();

    foreach (var avaliacao in ultimasAvaliacoes)
    {
        avaliacao.CriadoEm = ConvertUtcLikeToBrasília(avaliacao.CriadoEm);
    }

    return new AdminSalesDashboardResponse
    {
        TotalEventos = totalEventos,
        TotalReservas = totalCompras,
        ReceitaTotal = decimal.Round(receitaTotal, 2),
        ValorPendente = decimal.Round(valorPendente, 2),
        TotalVendidoHoje = decimal.Round(totalVendidoHoje, 2),
        TotalIngressosVendidos = totalIngressosVendidos,
        ReservasPagas = reservasPagas,
        ReservasPendentesPagamento = reservasPendentesPagamento,
        ReservasCanceladas = reservasCanceladas,
        CheckinsRealizados = checkinsRealizados,
        CapacidadeTotal = capacidadeTotal,
        CapacidadeRestante = capacidadeRestante,
        CuponsUtilizados = cuponsUtilizados,
        TotalAvaliacoes = totalAvaliacoes,
        MediaAvaliacoes = decimal.Round(mediaAvaliacoes, 2),
        TaxaOcupacaoPercentual = taxaOcupacao,
        TaxaCheckinPercentual = taxaCheckin,
        PagamentosPix = pagamentosPix,
        PagamentosCartão = pagamentosCartao,
        PagamentosBoleto = pagamentosBoleto,
        Compras = compras,
        UltimasCompras = compras.Take(5).ToList(),
        UltimasAvaliacoes = ultimasAvaliacoes,
        EventosMaisVendidos = eventosMaisVendidos
    };
}

static int NormalizeDashboardLimit(int? limit)
{
    if (!limit.HasValue || limit.Value <= 0)
        return 50;

    return Math.Clamp(limit.Value, 1, 200);
}

static int NormalizeDashboardOffset(int? offset)
{
    return Math.Max(0, offset ?? 0);
}

static int CountPaymentMethod(SqliteConnection connection, string paymentMethod)
{
    return connection.ExecuteScalar<int>(@"
        SELECT COUNT(*)
        FROM Reservas
        WHERE COALESCE(NULLIF(StatusPagamento, ''), 'Pago') NOT IN ('Cancelado', 'Cancelada', 'Reembolsado', 'Reembolsada')
          AND COALESCE(NULLIF(FormaPagamento, ''), 'Pix') = @paymentMethod COLLATE NOCASE",
        new { paymentMethod });
}

static bool IsRegularImagePayloadWithinLimit(string? value)
{
    return IsImagePayloadWithinLimit(value, 700_000);
}

static bool IsMapImagePayloadWithinLimit(string? value)
{
    return IsImagePayloadWithinLimit(value, 1_400_000);
}

static bool IsImagePayloadWithinLimit(string? value, int maxLength)
{
    var text = value?.Trim() ?? string.Empty;
    if (text.Length == 0)
        return true;

    if (!text.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        return true;

    return text.Length <= maxLength;
}

static void EnsureCheckinsForExistingReservations(SqliteConnection connection)
{
    var reservaIds = connection.Query<int>(@"
        SELECT r.Id
        FROM Reservas r
        LEFT JOIN Checkins c ON c.ReservaId = r.Id
        WHERE c.Id IS NULL").ToList();

    foreach (var reservaId in reservaIds)
    {
        connection.Execute(@"
            INSERT INTO Checkins (ReservaId, QrCode, DataCheckin, Status)
            VALUES (@reservaId, @qrCode, NULL, 'Pendente')",
            new { reservaId, qrCode = GenerateQrCode() });
    }
}

static bool IsKnownSchemaIdentifier(string identifier)
{
    return identifier.All(character =>
        char.IsAsciiLetterOrDigit(character) ||
        character == '_');
}

static DateTime ConvertUtcLikeToBrasília(DateTime value)
{
    var utcValue = value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    return TimeZoneInfo.ConvertTimeFromUtc(utcValue, GetBrasíliaTimeZone());
}

static TimeZoneInfo GetBrasíliaTimeZone()
{
    try
    {
        return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    }
    catch (TimeZoneNotFoundException)
    {
        return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
    }
}

static string MaskEmail(string email)
{
    var atIndex = email.IndexOf('@');
    if (atIndex <= 1)
        return email;

    var localPart = email[..atIndex];
    var domain = email[atIndex..];
    var visiblePrefix = localPart[..Math.Min(2, localPart.Length)];
    return $"{visiblePrefix}{new string('*', Math.Max(2, localPart.Length - visiblePrefix.Length))}{domain}";
}

static async Task SendPasswordResetEmailByApiAsync(
    HttpClient httpClient,
    string baseUrl,
    string apiKey,
    string senderEmail,
    string senderName,
    string recipientEmail,
    string subject,
    string body)
{
    var payload = new
    {
        from = $"{senderName} <{senderEmail}>",
        to = new[] { recipientEmail },
        subject,
        text = body
    };

    using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/emails")
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
    };
    request.Headers.Add("Authorization", $"Bearer {apiKey}");

    using var response = await httpClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"Falha ao enviar e-mail por API. Status: {(int)response.StatusCode}. Resposta: {errorContent}");
    }
}

static async Task SendPasswordResetEmailBySmtpAsync(
    string smtpHost,
    int smtpPort,
    string smtpUser,
    string smtpPassword,
    string senderName,
    string recipientEmail,
    string subject,
    string body)
{
    using var message = new MailMessage
    {
        From = new MailAddress(smtpUser, senderName),
        Subject = subject,
        Body = body,
        IsBodyHtml = false
    };

    message.To.Add(recipientEmail);

    using var client = new SmtpClient(smtpHost, smtpPort)
    {
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(smtpUser, smtpPassword)
    };

    await client.SendMailAsync(message);
}
