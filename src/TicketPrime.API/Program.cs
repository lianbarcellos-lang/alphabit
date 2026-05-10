using Dapper;
using Microsoft.Data.Sqlite;
using System.Net;
using System.Net.Mail;
using TicketPrime.API;
using TicketPrime.API.modelos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var adminToken = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Token");
var adminLogin = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Login");
var adminPassword = GetRequiredConfiguration(builder.Configuration, "AdminAccess:Password");
var smtpHost = builder.Configuration["EmailSettings:SmtpHost"];
var smtpPort = int.TryParse(builder.Configuration["EmailSettings:SmtpPort"], out var parsedSmtpPort) ? parsedSmtpPort : 587;
var smtpUser = builder.Configuration["EmailSettings:SenderEmail"];
var smtpPassword = builder.Configuration["EmailSettings:AppPassword"];
var smtpDisplayName = builder.Configuration["EmailSettings:SenderName"] ?? "TicketPrime";

var legacyDbPath = Path.Combine(builder.Environment.ContentRootPath, "db", "TicketPrime.db");
var configuredDbDirectory = builder.Configuration["DatabaseSettings:Directory"];
var railwayVolumeMountPath = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH");
var runtimeDbDirectory = !string.IsNullOrWhiteSpace(configuredDbDirectory)
    ? configuredDbDirectory
    : !string.IsNullOrWhiteSpace(railwayVolumeMountPath)
        ? Path.Combine(railwayVolumeMountPath, "ticketprime-data")
        : Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TicketPrime");
Directory.CreateDirectory(runtimeDbDirectory);

var dbPath = Path.Combine(runtimeDbDirectory, "TicketPrime.db");
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
            ImagemUrl TEXT
        );

        CREATE TABLE IF NOT EXISTS Cupons (
            Codigo TEXT PRIMARY KEY,
            PorcentagemDesconto REAL NOT NULL,
            ValorMinimoRegra REAL NOT NULL
        );

        CREATE TABLE IF NOT EXISTS GenerosMusicais (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL COLLATE NOCASE UNIQUE
        );

        CREATE TABLE IF NOT EXISTS CidadesEventos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL COLLATE NOCASE UNIQUE
        );

        CREATE TABLE IF NOT EXISTS Reservas (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf TEXT NOT NULL,
            EventoId INTEGER NOT NULL,
            CupomUtilizado TEXT NULL,
            Assentos TEXT NOT NULL DEFAULT '',
            Quantidade INTEGER NOT NULL DEFAULT 1,
            PrecoUnitario REAL NOT NULL DEFAULT 0,
            ValorFinalPago REAL NOT NULL,
            DataReserva TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
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

    EnsureColumnExists(connection, "Usuarios", "SenhaHash", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "Sobrenome", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "PaisResidencia", "TEXT NOT NULL DEFAULT 'Brasil'");
    EnsureColumnExists(connection, "Usuarios", "TipoDocumento", "TEXT NOT NULL DEFAULT 'CPF'");
    EnsureColumnExists(connection, "Usuarios", "CodigoPais", "TEXT NOT NULL DEFAULT '+55'");
    EnsureColumnExists(connection, "Usuarios", "Telefone", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Usuarios", "DataNascimento", "TEXT NULL");
    EnsureColumnExists(connection, "Usuarios", "Sexo", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "ImagemUrl", "TEXT");
    EnsureColumnExists(connection, "Eventos", "LocalEvento", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "CidadeEvento", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "Artista", "TEXT NOT NULL DEFAULT ''");
    EnsureColumnExists(connection, "Eventos", "GeneroMusical", "TEXT NOT NULL DEFAULT ''");
    EnsureReservasSchema(connection);
    EnsureMusicGenresCatalog(connection);
    EnsureCitiesCatalog(connection);

    EnsureDemoEvents(connection);
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
        return Results.BadRequest("Ja existe um usuario com este CPF.");

    var emailExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Usuarios WHERE lower(Email) = lower(@email)",
        new { email = usuario.Email });

    if (emailExiste > 0)
        return Results.BadRequest("Ja existe um usuario com este email.");

    connection.Execute(@"
        INSERT INTO Usuarios (Cpf, Nome, Email, SenhaHash)
        VALUES (@cpf, @nome, @email, @senhaHash)", new
    {
        cpf = usuario.Cpf,
        nome = usuario.Nome,
        email = usuario.Email,
        senhaHash = string.Empty
    });

    return Results.Ok("Usuario criado com sucesso. Para acessar a conta, finalize o cadastro pelo primeiro acesso.");
});

app.MapPost("/api/auth/usuarios/cadastro", (UsuarioCadastroRequest request) =>
{
    if (!TicketPrimeRules.IsValidUserRegistration(request))
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
        return Results.BadRequest("Ja existe uma conta com este email.");
    }

    if (!string.IsNullOrWhiteSpace(userByCpf.Cpf))
    {
        var senhaHashAtual = userByCpf.SenhaHash;
        if (!string.IsNullOrWhiteSpace(senhaHashAtual))
            return Results.BadRequest("Ja existe uma conta com este CPF.");

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
            senhaHash = TicketPrimeRules.HashPassword(request.Senha)
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
        senhaHash = TicketPrimeRules.HashPassword(request.Senha)
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

    if (TicketPrimeRules.IsAdminCredential(request.Login, request.Senha, adminLogin, adminPassword))
    {
        return Results.Ok(new AuthResponse
        {
            Sucesso = true,
            Perfil = "admin",
            Token = adminToken,
            Nome = "Administrador TicketPrime",
            Email = "admin@ticketprime.com",
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
        return Results.BadRequest("Cadastro nao encontrado.");

    if (usuario.SenhaHash != TicketPrimeRules.HashPassword(request.Senha))
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

app.MapPost("/api/auth/usuarios/recuperar-senha", async (UsuarioRecuperacaoSenhaRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Login))
        return Results.BadRequest(new RecuperacaoSenhaResponse
        {
            Sucesso = false,
            Mensagem = "Informe o e-mail ou CPF da conta."
        });

    if (!CanSendPasswordResetEmail(smtpHost, smtpUser, smtpPassword))
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
        codigoHash = TicketPrimeRules.HashPassword(codigo),
        emailDestino = usuario.Email,
        expiraEm = expiraEmUtc.ToString("s"),
        criadoEm = agoraUtc.ToString("s")
    });

    var corpo = $"""
Ola, {usuario.Nome}.

Recebemos um pedido para redefinir a senha da sua conta TicketPrime.

Código de verificação: {codigo}

Esse código expira em 15 minutos.
Se você não solicitou a redefinição, ignore esta mensagem.
""";

    try
    {
        await SendPasswordResetEmailAsync(
            smtpHost!,
            smtpPort,
            smtpUser!,
            smtpPassword!,
            smtpDisplayName,
            usuario.Email,
            "TicketPrime - Código para redefinir senha",
            corpo);
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
            Mensagem = "Informe o login, o codigo e a nova senha."
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

    var codigoHash = TicketPrimeRules.HashPassword(request.Codigo.Trim());
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
        senhaHash = TicketPrimeRules.HashPassword(request.NovaSenha)
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
    if (!TicketPrimeRules.IsAdminCredential(request.Login, request.Senha, adminLogin, adminPassword))
        return Results.BadRequest("Login de administrador invalido.");

    return Results.Ok(new AuthResponse
    {
        Sucesso = true,
        Perfil = "admin",
        Token = adminToken,
        Nome = "Administrador TicketPrime",
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
        ? Results.NotFound("Usuario nao encontrado.")
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
        return Results.NotFound("Usuario nao encontrado.");

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

    if (!TicketPrimeRules.IsValidEvent(evento))
        return Results.BadRequest("Preencha os dados do evento corretamente.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

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
        imagem = string.IsNullOrWhiteSpace(evento.ImagemUrl) ? null : evento.ImagemUrl
    });

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

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventos = connection.Query<Evento>("SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos ORDER BY DataEvento").ToList();

    return Results.Ok(eventos);
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
        return Results.BadRequest("Informe o nome do genero musical.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome });

    if (!string.IsNullOrWhiteSpace(existente))
        return Results.Ok(new { mensagem = "Genero musical ja cadastrado.", nome = existente });

    connection.Execute(
        "INSERT INTO GenerosMusicais (Nome) VALUES (@nome)",
        new { nome });

    return Results.Ok(new { mensagem = "Genero musical cadastrado com sucesso.", nome });
});

app.MapPut("/api/admin/generos/{nomeAtual}", (string nomeAtual, GeneroMusicalCatalogoRequest request, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    var novoNome = (request.Nome ?? string.Empty).Trim();

    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe o genero musical atual.");

    if (string.IsNullOrWhiteSpace(novoNome))
        return Results.BadRequest("Informe o novo nome do genero musical.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var existente = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal });

    if (string.IsNullOrWhiteSpace(existente))
        return Results.NotFound("Genero musical nao encontrado.");

    var conflito = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) = lower(@nome) AND lower(Nome) <> lower(@original)",
        new { nome = novoNome, original = nomeOriginal });

    if (!string.IsNullOrWhiteSpace(conflito))
        return Results.BadRequest("Ja existe um genero musical com esse nome.");

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

    return Results.Ok(new { mensagem = "Genero musical atualizado com sucesso.", nome = novoNome });
});

app.MapDelete("/api/admin/generos/{nomeAtual}", (string nomeAtual, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    var nomeOriginal = Uri.UnescapeDataString(nomeAtual).Trim();
    if (string.IsNullOrWhiteSpace(nomeOriginal))
        return Results.BadRequest("Informe o genero musical que sera excluido.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var transaction = connection.BeginTransaction();

    var proximoGenero = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM GenerosMusicais WHERE lower(Nome) <> lower(@nome) ORDER BY Nome LIMIT 1",
        new { nome = nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET GeneroMusical = @novoGenero WHERE lower(GeneroMusical) = lower(@nome)",
        new { nome = nomeOriginal, novoGenero = proximoGenero ?? string.Empty },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM GenerosMusicais WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal },
        transaction);

    if (deleted == 0)
        return Results.NotFound("Genero musical nao encontrado.");

    transaction.Commit();

    var mensagem = string.IsNullOrWhiteSpace(proximoGenero)
        ? "Genero musical excluido com sucesso."
        : $"Genero musical excluido com sucesso. Eventos vinculados foram atualizados para {proximoGenero}.";

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
        return Results.Ok(new { mensagem = "Cidade ja cadastrada.", nome = existente });

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
        return Results.NotFound("Cidade nao encontrada.");

    var conflito = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) = lower(@nome) AND lower(Nome) <> lower(@original)",
        new { nome = novoNome, original = nomeOriginal });

    if (!string.IsNullOrWhiteSpace(conflito))
        return Results.BadRequest("Ja existe uma cidade com esse nome.");

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

    var proximaCidade = connection.QueryFirstOrDefault<string>(
        "SELECT Nome FROM CidadesEventos WHERE lower(Nome) <> lower(@nome) ORDER BY Nome LIMIT 1",
        new { nome = nomeOriginal },
        transaction);

    connection.Execute(
        "UPDATE Eventos SET CidadeEvento = @novaCidade WHERE lower(CidadeEvento) = lower(@nome)",
        new { nome = nomeOriginal, novaCidade = proximaCidade ?? string.Empty },
        transaction);

    var deleted = connection.Execute(
        "DELETE FROM CidadesEventos WHERE lower(Nome) = lower(@nome)",
        new { nome = nomeOriginal },
        transaction);

    if (deleted == 0)
        return Results.NotFound("Cidade nao encontrada.");

    transaction.Commit();

    var mensagem = string.IsNullOrWhiteSpace(proximaCidade)
        ? "Cidade excluida com sucesso."
        : $"Cidade excluida com sucesso. Eventos vinculados foram atualizados para {proximaCidade}.";

    return Results.Ok(new { mensagem });
});

app.MapPut("/api/admin/eventos/{id:int}", (int id, EventoAdminRequest evento, HttpContext httpContext) =>
{
    var adminResult = EnsureAdminAccess(httpContext);
    if (adminResult is not null)
        return adminResult;

    if (!TicketPrimeRules.IsValidEvent(new Evento
    {
        Nome = evento.Nome,
        LocalEvento = evento.LocalEvento,
        CidadeEvento = evento.CidadeEvento,
        Artista = evento.Artista,
        GeneroMusical = evento.GeneroMusical,
        CapacidadeTotal = evento.CapacidadeTotal,
        DataEvento = evento.DataEvento,
        PrecoPadrao = evento.PrecoPadrao,
        ImagemUrl = evento.ImagemUrl
    }))
        return Results.BadRequest("Preencha os dados do evento corretamente.");

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
            ImagemUrl = $imagem
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
        imagem = string.IsNullOrWhiteSpace(evento.ImagemUrl) ? null : evento.ImagemUrl
    });
    if (updated == 0)
        return Results.NotFound("Evento nao encontrado.");

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
        return Results.NotFound("Evento nao encontrado.");

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
        return Results.NotFound("Evento nao encontrado.");

    return Results.Ok("Evento removido com sucesso!");
});

app.MapGet("/api/eventos", () =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventos = connection.Query<Evento>("SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos").ToList();

    return Results.Ok(eventos);
});

app.MapGet("/api/eventos/{id:int}", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var evento = connection.QueryFirstOrDefault<Evento>(
        "SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos WHERE Id = @id",
        new { id });

    return evento is null ? Results.NotFound("Evento nao encontrado.") : Results.Ok(evento);
});

app.MapGet("/api/eventos/{id:int}/assentos-ocupados", (int id) =>
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var eventoExiste = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Eventos WHERE Id = @id",
        new { id });

    if (eventoExiste == 0)
        return Results.NotFound("Evento nao encontrado.");

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

    if (!TicketPrimeRules.IsValidCoupon(cupom))
        return Results.BadRequest("Preencha os dados do cupom corretamente.");

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    if (connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Cupons WHERE Codigo = @codigo", new { codigo = cupom.Codigo }) > 0)
        return Results.BadRequest("Cupom ja existe.");

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

    if (!TicketPrimeRules.IsValidCoupon(cupom))
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
        return Results.NotFound("Cupom nao encontrado.");

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
        return Results.NotFound("Cupom nao encontrado.");

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
        return Results.NotFound("Cupom nao encontrado.");

    return Results.Ok("Cupom removido com sucesso!");
});

app.MapPost("/api/cupons/preview", (ReservaCheckoutRequest request) =>
{
    if (!TicketPrimeRules.IsValidCheckout(request))
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Informe o CPF do usuario e pelo menos um ingresso valido."
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
            Mensagem = "Usuario nao encontrado para calcular o desconto."
        });

    var eventoIds = request.Itens.Select(item => item.EventoId).Distinct().ToArray();
    var eventos = connection.Query<Evento>(
        "SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos WHERE Id IN @ids",
        new { ids = eventoIds }).ToList();

    if (eventos.Count != eventoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais eventos nao foram encontrados."
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
                Mensagem = "Cupom nao encontrado."
            });
    }

    decimal totalOriginal = 0;
    decimal totalComDesconto = 0;

    foreach (var item in request.Itens)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var subtotal = evento.PrecoPadrao * item.Quantidade;
        var subtotalComDesconto = subtotal;

        if (cupom is not null && evento.PrecoPadrao >= cupom.ValorMinimoRegra)
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
                ? "Cupom valido, mas sem desconto para os itens atuais."
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
    if (!TicketPrimeRules.IsValidCheckout(request))
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Informe o CPF do usuario e pelo menos um ingresso valido."
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
            Mensagem = "Usuario nao encontrado para concluir a compra."
        });

    var eventoIds = request.Itens.Select(item => item.EventoId).Distinct().ToArray();
    var eventos = connection.Query<Evento>(
        "SELECT Id, Nome, LocalEvento, CidadeEvento, Artista, GeneroMusical, CapacidadeTotal, DataEvento, PrecoPadrao, ImagemUrl FROM Eventos WHERE Id IN @ids",
        new { ids = eventoIds }).ToList();

    if (eventos.Count != eventoIds.Length)
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Um ou mais eventos nao foram encontrados."
        });

    foreach (var item in request.Itens)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
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
                Mensagem = $"O assento {assentoJaReservado} do evento {evento.Nome} ja foi reservado."
            });

        var reservasMesmoCpfNoEvento = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE UsuarioCpf = @cpf AND EventoId = @eventoId",
            new { cpf = request.UsuarioCpf, eventoId = item.EventoId });

        if (reservasMesmoCpfNoEvento + item.Quantidade > 2)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O CPF informado nao pode ter mais de 2 reservas para o evento {evento.Nome}."
            });

        var ingressosReservados = connection.ExecuteScalar<int>(
            "SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE EventoId = @eventoId",
            new { eventoId = item.EventoId });

        if (ingressosReservados + item.Quantidade > evento.CapacidadeTotal)
            return Results.BadRequest(new ReservaCheckoutResponse
            {
                Sucesso = false,
                Mensagem = $"O evento {evento.Nome} nao possui mais lugares suficientes."
            });
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
                Mensagem = "Cupom nao encontrado."
            });
    }

    decimal totalOriginal = 0;
    decimal totalComDesconto = 0;
    var reservasParaInserir = new List<Reserva>();

    foreach (var item in request.Itens)
    {
        var evento = eventos.First(current => current.Id == item.EventoId);
        var subtotal = evento.PrecoPadrao * item.Quantidade;
        var subtotalComDesconto = subtotal;

        if (cupom is not null && evento.PrecoPadrao >= cupom.ValorMinimoRegra)
        {
            subtotalComDesconto = subtotal - (subtotal * (cupom.PorcentagemDesconto / 100m));
        }

        totalOriginal += subtotal;
        totalComDesconto += subtotalComDesconto;

        reservasParaInserir.Add(new Reserva
        {
            UsuarioCpf = request.UsuarioCpf,
            EventoId = item.EventoId,
            CupomUtilizado = cupom?.Codigo,
            Assentos = string.Join(", ", item.Assentos.OrderBy(value => value)),
            Quantidade = item.Quantidade,
            PrecoUnitario = evento.PrecoPadrao,
            ValorFinalPago = decimal.Round(subtotalComDesconto, 2),
            DataReserva = DateTime.UtcNow
        });
    }

    using var transaction = connection.BeginTransaction();

    foreach (var reserva in reservasParaInserir)
    {
        connection.Execute(@"
            INSERT INTO Reservas (UsuarioCpf, EventoId, CupomUtilizado, Assentos, Quantidade, PrecoUnitario, ValorFinalPago, DataReserva)
            VALUES (@UsuarioCpf, @EventoId, @CupomUtilizado, @Assentos, @Quantidade, @PrecoUnitario, @ValorFinalPago, @DataReserva)",
            new
            {
                reserva.UsuarioCpf,
                reserva.EventoId,
                reserva.CupomUtilizado,
                reserva.Assentos,
                reserva.Quantidade,
                reserva.PrecoUnitario,
                reserva.ValorFinalPago,
                DataReserva = reserva.DataReserva.ToString("s")
            }, transaction);
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
            Mensagem = "Nao foi possivel concluir a compra com os dados atuais. Revise o cupom e os assentos selecionados."
        });
    }
    catch
    {
        return Results.BadRequest(new ReservaCheckoutResponse
        {
            Sucesso = false,
            Mensagem = "Nao foi possivel concluir a compra agora. Tente novamente em instantes."
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
            COALESCE(r.CupomUtilizado, '') AS CupomUtilizado,
            r.Assentos,
            r.Quantidade,
            r.PrecoUnitario,
            r.ValorFinalPago,
            r.DataReserva
        FROM Reservas r
        INNER JOIN Eventos e ON e.Id = r.EventoId
        WHERE r.UsuarioCpf = @cpf
        ORDER BY r.DataReserva DESC", new { cpf }).ToList();

    return Results.Ok(reservas);
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
        !string.Equals(userCpf.ToString(), routeCpf, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    return null;
}

static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string sqlDefinition)
{
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

    var foreignKeys = connection.Query<(int Id, int Seq, string Table, string From, string To, string OnUpdate, string OnDelete, string Match)>(
        "PRAGMA foreign_key_list(Reservas)");

    var hasUsuarioFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "UsuarioCpf", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Usuarios", StringComparison.OrdinalIgnoreCase));

    var hasEventoFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "EventoId", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Eventos", StringComparison.OrdinalIgnoreCase));

    var hasCupomFk = foreignKeys.Any(fk =>
        string.Equals(fk.From, "CupomUtilizado", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(fk.Table, "Cupons", StringComparison.OrdinalIgnoreCase));

    if (hasUsuarioFk && hasEventoFk && hasCupomFk)
        return;

    using var transaction = connection.BeginTransaction();

    connection.Execute("PRAGMA foreign_keys = OFF;", transaction: transaction);

    connection.Execute(@"
        CREATE TABLE Reservas_new (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UsuarioCpf TEXT NOT NULL,
            EventoId INTEGER NOT NULL,
            CupomUtilizado TEXT NULL,
            Assentos TEXT NOT NULL DEFAULT '',
            Quantidade INTEGER NOT NULL DEFAULT 1,
            PrecoUnitario REAL NOT NULL DEFAULT 0,
            ValorFinalPago REAL NOT NULL,
            DataReserva TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE,
            FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE,
            FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo) ON DELETE SET NULL
        );", transaction: transaction);

    connection.Execute(@"
        INSERT INTO Reservas_new (Id, UsuarioCpf, EventoId, CupomUtilizado, Assentos, Quantidade, PrecoUnitario, ValorFinalPago, DataReserva)
        SELECT
            Id,
            UsuarioCpf,
            EventoId,
            NULLIF(CupomUtilizado, ''),
            COALESCE(Assentos, ''),
            COALESCE(Quantidade, 1),
            COALESCE(PrecoUnitario, 0),
            ValorFinalPago,
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
        "Festival",
        "Rock",
        "Pop",
        "Samba",
        "Eletronica",
        "MPB"
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
            Nome = "Festival Sunset Brasil",
            LocalEvento = "Allianz Parque",
            CidadeEvento = "Sao Paulo",
            Artista = "Anitta e convidados",
            GeneroMusical = "Festival",
            CapacidadeTotal = 18000,
            DataEvento = new DateTime(2026, 6, 20, 17, 0, 0),
            PrecoPadrao = 249.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Noite do Rock Arena",
            LocalEvento = "Jeunesse Arena",
            CidadeEvento = "Rio de Janeiro",
            Artista = "Capital Inicial",
            GeneroMusical = "Rock",
            CapacidadeTotal = 9500,
            DataEvento = new DateTime(2026, 7, 12, 21, 0, 0),
            PrecoPadrao = 189.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Pop Experience Live",
            LocalEvento = "Espaco Unimed",
            CidadeEvento = "Sao Paulo",
            Artista = "Jao",
            GeneroMusical = "Pop",
            CapacidadeTotal = 12000,
            DataEvento = new DateTime(2026, 8, 8, 20, 30, 0),
            PrecoPadrao = 219.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Samba Prime Experience",
            LocalEvento = "Vivo Rio",
            CidadeEvento = "Rio de Janeiro",
            Artista = "Thiaguinho",
            GeneroMusical = "Samba",
            CapacidadeTotal = 6000,
            DataEvento = new DateTime(2026, 9, 5, 19, 30, 0),
            PrecoPadrao = 159.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Electronic Lights",
            LocalEvento = "Mineirao Hall",
            CidadeEvento = "Belo Horizonte",
            Artista = "Alok",
            GeneroMusical = "Eletronica",
            CapacidadeTotal = 14000,
            DataEvento = new DateTime(2026, 10, 17, 22, 0, 0),
            PrecoPadrao = 279.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?auto=format&fit=crop&w=1200&q=80"
        },
        new Evento
        {
            Nome = "Turne Acustica Brasil",
            LocalEvento = "Teatro Bradesco",
            CidadeEvento = "Sao Paulo",
            Artista = "Lulu Santos",
            GeneroMusical = "MPB",
            CapacidadeTotal = 4200,
            DataEvento = new DateTime(2026, 11, 7, 20, 0, 0),
            PrecoPadrao = 174.90m,
            ImagemUrl = "https://images.unsplash.com/photo-1516450360452-9312f5e86fc7?auto=format&fit=crop&w=1200&q=80"
        }
    };

    foreach (var evento in demoEvents)
    {
        var exists = connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Eventos WHERE Nome = @nome",
            new { nome = evento.Nome });

        if (exists > 0)
            continue;

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

static string GetRequiredConfiguration(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"A configuracao obrigatoria '{key}' nao foi encontrada.");

    return value;
}

static bool CanSendPasswordResetEmail(string? smtpHost, string? smtpUser, string? smtpPassword)
{
    return !string.IsNullOrWhiteSpace(smtpHost) &&
           !string.IsNullOrWhiteSpace(smtpUser) &&
           !string.IsNullOrWhiteSpace(smtpPassword);
}

static string GenerateResetCode()
{
    return Random.Shared.Next(100000, 999999).ToString();
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

static async Task SendPasswordResetEmailAsync(
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
