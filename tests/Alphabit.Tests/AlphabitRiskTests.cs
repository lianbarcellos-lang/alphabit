namespace Alphabit.Tests;

public class AlphabitRiskTests
{
    private static readonly string ProgramSource = File.ReadAllText(GetApiFilePath("Program.cs"));
    private static readonly string RulesSource = File.ReadAllText(GetApiFilePath("AlphabitRules.cs"));

    [Fact]
    public void ReviewRisk_ProfileRoutes_ShouldRequireUserAccessCheck()
    {
        Assert.Contains("app.MapGet(\"/api/usuarios/{cpf}/perfil\"", ProgramSource);
        Assert.Contains("app.MapPut(\"/api/usuarios/{cpf}/perfil\"", ProgramSource);

        var getBlock = ExtractBlock(ProgramSource, "app.MapGet(\"/api/usuarios/{cpf}/perfil\"");
        var putBlock = ExtractBlock(ProgramSource, "app.MapPut(\"/api/usuarios/{cpf}/perfil\"");

        Assert.Contains("EnsureUserAccess", getBlock);
        Assert.Contains("EnsureUserAccess", putBlock);
    }

    [Fact]
    public void ReviewRisk_ReservationsRoute_ShouldRequireUserAccessCheck()
    {
        Assert.Contains("app.MapGet(\"/api/reservas/{cpf}\"", ProgramSource);

        var getBlock = ExtractBlock(ProgramSource, "app.MapGet(\"/api/reservas/{cpf}\"");

        Assert.Contains("EnsureUserAccess", getBlock);
    }

    [Fact]
    public void ReviewRisk_UsersRoute_ShouldRequireAdminAccess()
    {
        Assert.Contains("app.MapGet(\"/api/usuarios\"", ProgramSource);

        var getBlock = ExtractBlock(ProgramSource, "app.MapGet(\"/api/usuarios\"");

        Assert.Contains("EnsureAdminAccess", getBlock);
    }

    [Fact]
    public void ReviewRisk_DeleteEvent_ShouldDeleteDependenciesInsideTransaction()
    {
        var block = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/admin/eventos/{id:int}\"");

        Assert.Contains("BeginTransaction", block);
        Assert.Contains("DELETE FROM Reservas WHERE EventoId = @id", block);
        Assert.Contains("DELETE FROM Eventos WHERE Id = @id", block);
        Assert.Contains("transaction.Commit()", block);
    }

    [Fact]
    public void ReviewRisk_DeleteCoupon_ShouldClearReservationsInsideTransaction()
    {
        var block = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/admin/cupons/{codigo}\"");

        Assert.Contains("BeginTransaction", block);
        Assert.Contains("UPDATE Reservas SET CupomUtilizado = NULL WHERE CupomUtilizado = @codigo", block);
        Assert.Contains("DELETE FROM Cupons WHERE Codigo = @codigo", block);
        Assert.Contains("transaction.Commit()", block);
    }

    [Fact]
    public void ReviewRisk_Av1UserRoute_ShouldAvoidPredictablePasswordAndBlockDuplicateEmail()
    {
        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/usuarios\"");

        Assert.Contains("SenhaHash", block);
        Assert.Contains("SELECT COUNT(*) FROM Usuarios WHERE lower(Email) = lower(@email)", block);
        Assert.Contains("senhaHash = string.Empty", block);
        Assert.DoesNotContain("HashPassword(senhaInicial)", block);
        Assert.DoesNotContain("senha inicial", block, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReviewRisk_ReservationsTable_ShouldBeCreatedWithForeignKeys()
    {
        var createTableBlock = ExtractBetween(
            ProgramSource,
            "CREATE TABLE IF NOT EXISTS Reservas (",
            "        );");

        Assert.Contains("FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf) ON DELETE CASCADE", createTableBlock);
        Assert.Contains("FOREIGN KEY (EventoId) REFERENCES Eventos(Id) ON DELETE CASCADE", createTableBlock);
        Assert.Contains("FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo) ON DELETE SET NULL", createTableBlock);
        Assert.Contains("UsuarioCpf TEXT NOT NULL", createTableBlock);
        Assert.Contains("EventoId INTEGER NOT NULL", createTableBlock);
        Assert.Contains("CupomUtilizado TEXT NULL", createTableBlock);
        Assert.Contains("EnsureReservasSchema(connection);", ProgramSource);
    }

    [Fact]
    public void ReviewRisk_AdminCredential_ShouldComeFromConfiguration()
    {
        Assert.DoesNotContain("return login == \"admin\" && senha == \"admin\";", RulesSource);
        Assert.Contains("CryptographicOperations.FixedTimeEquals", RulesSource);
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Login\")", ProgramSource);
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Password\")", ProgramSource);
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Token\")", ProgramSource);
        Assert.DoesNotContain("?? \"admin\"", ProgramSource);
        Assert.DoesNotContain("?? \"alphabit-admin-token\"", ProgramSource);
    }

    [Fact]
    public void ReviewRisk_ResetAndOrderCodes_ShouldUseCryptographicRandomness()
    {
        Assert.Contains("RandomNumberGenerator.GetInt32(100000, 1000000)", ProgramSource);
        Assert.Contains("ALP-{DateTime.UtcNow:yyyyMMdd}", ProgramSource);
        Assert.DoesNotContain("TP-{DateTime.UtcNow:yyyyMMdd}", ProgramSource);
        Assert.DoesNotContain("Random.Shared.Next(100000, 999999)", ProgramSource);
    }

    [Fact]
    public void ReviewRisk_Checkout_ShouldRevalidateCapacityInsideTransaction()
    {
        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/reservas\"");
        var transactionStart = block.IndexOf("using var transaction = connection.BeginTransaction();", StringComparison.Ordinal);
        Assert.True(transactionStart >= 0, "Transacao de reserva nao encontrada.");

        var transactionalBlock = block[transactionStart..];

        Assert.Contains("SELECT Assentos FROM Reservas WHERE EventoId = @eventoId AND Assentos <> ''", transactionalBlock);
        Assert.Contains("SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE UsuarioCpf = @cpf AND EventoId = @eventoId", transactionalBlock);
        Assert.Contains("SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE EventoId = @eventoId", transactionalBlock);
        Assert.Contains("transaction.Commit()", transactionalBlock);
    }

    [Fact]
    public void ReviewRisk_TicketTypes_ShouldHaveTableAndCheckoutLimits()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS TiposIngresso", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_TiposIngresso_Evento_Nome", ProgramSource);
        Assert.Contains("TipoIngressoId INTEGER NULL", ProgramSource);
        Assert.Contains("FOREIGN KEY (TipoIngressoId) REFERENCES TiposIngresso(Id) ON DELETE SET NULL", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/tipos-ingresso\"", ProgramSource);
        Assert.Contains("EnsureDefaultTicketTypesForEvent(connection, eventId, evento.PrecoPadrao, evento.CapacidadeTotal)", ProgramSource);
        Assert.Contains("EnsureDefaultTicketTypesForEvent(connection, id, evento.PrecoPadrao, evento.CapacidadeTotal)", ProgramSource);

        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/reservas\"");
        var transactionStart = block.IndexOf("using var transaction = connection.BeginTransaction();", StringComparison.Ordinal);
        var transactionalBlock = block[transactionStart..];

        Assert.Contains("SELECT COALESCE(SUM(Quantidade), 0) FROM Reservas WHERE TipoIngressoId = @tipoIngressoId", transactionalBlock);
        Assert.Contains("tipoIngresso.QuantidadeDisponivel", transactionalBlock);
        Assert.Contains("INSERT INTO Reservas (UsuarioCpf, EventoId, TipoIngressoId", block);
    }

    [Fact]
    public void ReviewRisk_Activities_ShouldHaveTablesAndSignupLimits()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS Atividades", ProgramSource);
        Assert.Contains("CREATE TABLE IF NOT EXISTS InscricoesAtividades", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_InscricoesAtividades_Atividade_Usuario", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/atividades\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/atividades\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/atividades/{id:int}/inscricao\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/admin/atividades/{id:int}\"", ProgramSource);
        Assert.Contains("EnsureDefaultActivitiesForEvent(connection, eventId, evento.Nome, evento.DataEvento)", ProgramSource);

        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/atividades/{id:int}/inscricao\"");
        var createBlock = ExtractBlock(ProgramSource, "app.MapPost(\"/api/atividades\"");

        Assert.Contains("EnsureUserAccess(httpContext, request.UsuarioCpf)", block);
        Assert.Contains("using var transaction = connection.BeginTransaction();", block);
        Assert.Contains("Usuário já inscrito nesta atividade.", block);
        Assert.Contains("inscritos >= atividade.LimiteParticipantes", block);
        Assert.Contains("transaction.Commit()", block);
        Assert.Contains("Já existe uma atividade com este nome para o evento.", createBlock);

        var deleteBlock = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/admin/atividades/{id:int}\"");
        Assert.Contains("EnsureAdminAccess", deleteBlock);
        Assert.Contains("DELETE FROM InscricoesAtividades WHERE AtividadeId = @id", deleteBlock);
        Assert.Contains("DELETE FROM Atividades WHERE Id = @id", deleteBlock);
    }

    [Fact]
    public void ReviewRisk_Guests_ShouldHaveTablesAndAdminAssociationRoutes()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS Convidados", ProgramSource);
        Assert.Contains("CREATE TABLE IF NOT EXISTS EventoConvidados", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_Convidados_Nome_Tipo", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/convidados\"", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/convidados\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/convidados\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/eventos/{id:int}/convidados\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/eventos/{id:int}/convidados/{convidadoId:int}\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/admin/convidados/{id:int}\"", ProgramSource);

        var createBlock = ExtractBlock(ProgramSource, "app.MapPost(\"/api/convidados\"");
        Assert.Contains("EnsureAdminAccess", createBlock);
        Assert.Contains("Já existe um convidado com este nome e tipo.", createBlock);

        var linkBlock = ExtractBlock(ProgramSource, "app.MapPost(\"/api/eventos/{id:int}/convidados\"");
        Assert.Contains("EnsureAdminAccess", linkBlock);
        Assert.Contains("using var transaction = connection.BeginTransaction();", linkBlock);
        Assert.Contains("INSERT OR IGNORE INTO EventoConvidados", linkBlock);
        Assert.Contains("transaction.Commit()", linkBlock);

        var unlinkBlock = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/eventos/{id:int}/convidados/{convidadoId:int}\"");
        Assert.Contains("EnsureAdminAccess", unlinkBlock);
        Assert.Contains("DELETE FROM EventoConvidados", unlinkBlock);

        var deleteGuestBlock = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/admin/convidados/{id:int}\"");
        Assert.Contains("EnsureAdminAccess", deleteGuestBlock);
        Assert.Contains("DELETE FROM EventoConvidados WHERE ConvidadoId = @id", deleteGuestBlock);
        Assert.Contains("DELETE FROM Convidados WHERE Id = @id", deleteGuestBlock);
    }

    [Fact]
    public void ReviewRisk_Checkins_ShouldGenerateQrAndValidateOnce()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS Checkins", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_Checkins_Reserva", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_Checkins_QrCode", ProgramSource);
        Assert.Contains("EnsureCheckinsForExistingReservations(connection);", ProgramSource);
        Assert.Contains("GenerateQrCode()", ProgramSource);
        Assert.Contains("INSERT INTO Checkins (ReservaId, QrCode, DataCheckin, Status)", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/checkin\"", ProgramSource);

        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/checkin\"");

        Assert.Contains("EnsureAdminAccess", block);
        Assert.Contains("QR Code inválido.", block);
        Assert.Contains("Reserva cancelada não pode fazer check-in.", block);
        Assert.Contains("Check-in já realizado para esta reserva.", block);
        Assert.Contains("UPDATE Checkins", block);
        Assert.Contains("Status = 'Usado'", block);
        Assert.Contains("AND Status <> 'Usado'", block);
        Assert.Contains("updatedRows == 0", block);
    }

    [Fact]
    public void ReviewRisk_ApiProject_ShouldKeepOnlySQLiteProvider()
    {
        var projectFile = File.ReadAllText(GetApiFilePath("Alphabit.API.csproj"));

        Assert.Contains("Microsoft.Data.Sqlite", projectFile);
        Assert.DoesNotContain("System.Data.SqlClient", projectFile);
        Assert.DoesNotContain("System.Data.SQLite", projectFile);
    }

    [Fact]
    public void ReviewRisk_AdminDashboard_ShouldExposeOperationalMetrics()
    {
        Assert.Contains("app.MapGet(\"/api/admin/vendas/dashboard\"", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/dashboard\"", ProgramSource);
        Assert.Contains("BuildAdminSalesDashboard(connectionString)", ProgramSource);

        var dashboardBlock = ExtractBlock(ProgramSource, "static AdminSalesDashboardResponse BuildAdminSalesDashboard");

        Assert.Contains("TotalEventos", dashboardBlock);
        Assert.Contains("TotalReservas", dashboardBlock);
        Assert.Contains("ReceitaTotal", dashboardBlock);
        Assert.Contains("ValorPendente", dashboardBlock);
        Assert.Contains("ReservasPagas", dashboardBlock);
        Assert.Contains("ReservasCanceladas", dashboardBlock);
        Assert.Contains("CheckinsRealizados", dashboardBlock);
        Assert.Contains("CapacidadeRestante", dashboardBlock);
        Assert.Contains("CuponsUtilizados", dashboardBlock);
        Assert.Contains("TaxaOcupacaoPercentual", dashboardBlock);
        Assert.Contains("TaxaCheckinPercentual", dashboardBlock);
        Assert.Contains("INNER JOIN Reservas r ON r.Id = c.ReservaId", dashboardBlock);
        Assert.Contains("SELECT COALESCE(SUM(CapacidadeTotal), 0) FROM Eventos", dashboardBlock);
        Assert.Contains("IsPaidPaymentStatus", dashboardBlock);
        Assert.Contains("IsPendingPaymentStatus", dashboardBlock);
        Assert.Contains("IsCancelledOrRefundedPaymentStatus", dashboardBlock);
    }

    [Fact]
    public void ReviewRisk_AdminReviewModeration_ShouldExposeMetricsAndDeleteRoute()
    {
        Assert.Contains("app.MapDelete(\"/api/admin/avaliacoes/{id:int}\"", ProgramSource);

        var deleteBlock = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/admin/avaliacoes/{id:int}\"");
        var dashboardBlock = ExtractBlock(ProgramSource, "static AdminSalesDashboardResponse BuildAdminSalesDashboard");

        Assert.Contains("EnsureAdminAccess", deleteBlock);
        Assert.Contains("DELETE FROM Avaliacoes WHERE Id = @id", deleteBlock);
        Assert.Contains("TotalAvaliacoes", dashboardBlock);
        Assert.Contains("MediaAvaliacoes", dashboardBlock);
        Assert.Contains("UltimasAvaliacoes", dashboardBlock);
        Assert.Contains("FROM Avaliacoes a", dashboardBlock);
        Assert.Contains("ORDER BY a.CriadoEm DESC", dashboardBlock);
    }

    [Fact]
    public void ReviewRisk_Reviews_ShouldRequireReservationAndPreventDuplicates()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS Avaliacoes", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_Avaliacoes_Evento_Usuario", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/avaliacoes\"", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/avaliacoes\"", ProgramSource);

        var createBlock = ExtractBlock(ProgramSource, "app.MapPost(\"/api/avaliacoes\"");

        Assert.Contains("EnsureUserAccess(httpContext, request.UsuarioCpf)", createBlock);
        Assert.Contains("request.Nota < 1 || request.Nota > 5", createBlock);
        Assert.Contains("Usuário só pode avaliar evento reservado.", createBlock);
        Assert.Contains("Usuário já avaliou este evento.", createBlock);
        Assert.Contains("INSERT INTO Avaliacoes", createBlock);
        Assert.Contains("StatusPagamento", createBlock);
        Assert.Contains("request.UsuarioCpf = (request.UsuarioCpf ?? string.Empty).Trim();", createBlock);
        Assert.Contains("request.Comentario = (request.Comentario ?? string.Empty).Trim();", createBlock);
        Assert.Contains("eventoId = request.EventoId", createBlock);
        Assert.Contains("usuarioCpf = request.UsuarioCpf", createBlock);
    }

    private static string GetApiFilePath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "Alphabit.API", fileName);
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"Nao foi possivel localizar o arquivo '{fileName}' da API.");
    }

    private static string ExtractBlock(string source, string marker)
    {
        var start = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Bloco nao encontrado: {marker}");

        var nextMap = source.IndexOf("\napp.Map", start + marker.Length, StringComparison.Ordinal);
        return nextMap >= 0
            ? source[start..nextMap]
            : source[start..];
    }

    private static string ExtractBetween(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Trecho inicial nao encontrado: {startMarker}");

        start += startMarker.Length;

        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end >= 0, $"Trecho final nao encontrado: {endMarker}");

        return source[start..end];
    }
}
