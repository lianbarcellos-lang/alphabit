namespace Alphabit.Tests;

public class AlphabitRiskTests
{
    private static readonly string ProgramSource = File.ReadAllText(GetApiFilePath("Program.cs"));
    private static readonly string RulesSource = File.ReadAllText(GetApiFilePath("AlphabitRules.cs"));
    private static readonly string ReservationsPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Reservas.razor"));
    private static readonly string ReservationsCssSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Reservas.razor.css"));
    private static readonly string AdminPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Admin.razor"));
    private static readonly string AdminCssSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Admin.razor.css"));
    private static readonly string EventDetailPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "DetalheEvento.razor"));
    private static readonly string EventDetailCssSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "DetalheEvento.razor.css"));
    private static readonly string SeatsPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Assentos.razor"));
    private static readonly string EventsPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Eventos.razor"));
    private static readonly string CartPageSource = File.ReadAllText(GetAppFilePath("Components", "Pages", "Carrinho.razor"));

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
    public void ReviewRisk_EventDetail_ShouldContainDirectPurchaseFlowAndKeepSeatsRedirect()
    {
        Assert.Contains("@inject CartState Cart", EventDetailPageSource);
        Assert.Contains("id=\"compra\"", EventDetailPageSource);
        Assert.Contains("Escolha seu ingresso", EventDetailPageSource);
        Assert.Contains("Entrada sem assento marcado", EventDetailPageSource);
        Assert.Contains("GetTicketTypesAsync(Id)", EventDetailPageSource);
        Assert.Contains("Cart.AddEvent(eventItem, quantity, SelectedTicketType)", EventDetailPageSource);
        Assert.Contains("Navigation.NavigateTo(\"/carrinho\")", EventDetailPageSource);
        Assert.Contains("AuthOverlay.OpenLogin()", EventDetailPageSource);
        Assert.Contains("event-purchase-grid", EventDetailCssSource);
        Assert.Contains("ticket-type-card--active", EventDetailCssSource);

        Assert.Contains("@page \"/eventos/{Id:int}/assentos\"", SeatsPageSource);
        Assert.Contains("Navigation.NavigateTo($\"/eventos/{Id}#compra\", replace: true)", SeatsPageSource);
        Assert.DoesNotContain("Cart.AddEvent", SeatsPageSource);
        Assert.DoesNotContain("GetTicketTypesAsync", SeatsPageSource);

        Assert.Contains("@page \"/\"", EventsPageSource);
        Assert.Contains("@page \"/eventos\"", EventsPageSource);
        Assert.Contains("href=\"@GetEventHref(item.Id)\"", EventsPageSource);
        Assert.Contains("private static string GetEventHref(int eventId) => $\"/eventos/{eventId}\"", EventsPageSource);
        Assert.Contains("Navigation.NavigateTo($\"/eventos/{item.EventoId}#compra\")", CartPageSource);
        Assert.DoesNotContain("/assentos", EventsPageSource);
        Assert.DoesNotContain("/assentos", CartPageSource);
    }

    [Fact]
    public void ReviewRisk_Activities_ShouldHaveTablesAndSignupLimits()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS Atividades", ProgramSource);
        Assert.Contains("CREATE TABLE IF NOT EXISTS InscricoesAtividades", ProgramSource);
        Assert.Contains("HorarioFim TEXT NOT NULL DEFAULT ''", ProgramSource);
        Assert.Contains("Descricao TEXT NOT NULL DEFAULT ''", ProgramSource);
        Assert.Contains("Quantidade INTEGER NOT NULL DEFAULT 1", ProgramSource);
        Assert.Contains("Assentos TEXT NOT NULL DEFAULT ''", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_InscricoesAtividades_Atividade_Usuario", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/atividades\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/atividades\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/atividades/{id:int}/inscricao\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/atividades/{id:int}/inscricao/{usuarioCpf}\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/admin/atividades/{id:int}\"", ProgramSource);
        Assert.Contains("EnsureDefaultActivitiesForEvent(connection, eventId, evento.Nome, evento.DataEvento)", ProgramSource);

        var block = ExtractBlock(ProgramSource, "app.MapPost(\"/api/atividades/{id:int}/inscricao\"");
        var createBlock = ExtractBlock(ProgramSource, "app.MapPost(\"/api/atividades\"");
        var cancelBlock = ExtractBlock(ProgramSource, "app.MapDelete(\"/api/atividades/{id:int}/inscricao/{usuarioCpf}\"");

        Assert.Contains("EnsureUserAccess(httpContext, request.UsuarioCpf)", block);
        Assert.Contains("using var transaction = connection.BeginTransaction();", block);
        Assert.Contains("totalUsuarioOutrasAtividades + request.Quantidade > 2", block);
        Assert.Contains("inscritos + request.Quantidade > atividade.LimiteParticipantes", block);
        Assert.Contains("requestedSeats.Count != request.Quantidade", block);
        Assert.Contains("Um ou mais assentos selecionados já foram reservados.", block);
        Assert.Contains("UPDATE InscricoesAtividades", block);
        Assert.Contains("INSERT INTO InscricoesAtividades (AtividadeId, UsuarioCpf, Quantidade, Assentos, CriadoEm)", block);
        Assert.Contains("transaction.Commit()", block);
        Assert.Contains("Já existe uma atividade com este nome para o evento.", createBlock);
        Assert.Contains("INSERT INTO Atividades (EventoId, Nome, Horario, HorarioFim, Tipo, Descricao, LimiteParticipantes)", createBlock);
        Assert.Contains("EnsureUserAccess(httpContext, usuarioCpf)", cancelBlock);
        Assert.Contains("DELETE FROM InscricoesAtividades", cancelBlock);

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
        Assert.Contains("BuildAdminSalesDashboard(connectionString", ProgramSource);

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
        Assert.Contains("LIMIT @limit OFFSET @offset", dashboardBlock);
        Assert.Contains("StatusPagamento", dashboardBlock);
        Assert.Contains("ValorFinalPago", dashboardBlock);
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
    public void ReviewRisk_EventStands_ShouldExposePublicMapAndAdminAllocation()
    {
        Assert.Contains("CREATE TABLE IF NOT EXISTS StandsEspacos", ProgramSource);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS IX_StandsEspacos_Evento_Codigo", ProgramSource);
        Assert.Contains("EnsureDemoStands(connection);", ProgramSource);
        Assert.Contains("EnsureDefaultStandSectorsForEvent(connection, eventId);", ProgramSource);
        Assert.DoesNotContain("EnsureDefaultStandsForEvent", ProgramSource);
        Assert.Contains("app.MapGet(\"/api/eventos/{id:int}/stands\"", ProgramSource);
        Assert.Contains("app.MapPut(\"/api/admin/eventos/{id:int}/mapa-imagem\"", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/admin/eventos/{id:int}/stands\"", ProgramSource);
        Assert.Contains("app.MapPut(\"/api/admin/eventos/{id:int}/stands/{standId:int}\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/admin/eventos/{id:int}/stands/{standId:int}\"", ProgramSource);
        Assert.Contains("CREATE TABLE IF NOT EXISTS StandSetores", ProgramSource);
        Assert.Contains("app.MapPost(\"/api/admin/eventos/{id:int}/stand-setores\"", ProgramSource);
        Assert.Contains("app.MapPut(\"/api/admin/eventos/{id:int}/stand-setores/{nomeAtual}\"", ProgramSource);
        Assert.Contains("app.MapDelete(\"/api/admin/eventos/{id:int}/stand-setores/{nome}\"", ProgramSource);
        Assert.Contains("MapaImagemUrl", ProgramSource);
        Assert.Contains("AreaX", ProgramSource);
        Assert.Contains("AreaLargura", ProgramSource);
        Assert.Contains("PrecoPorMetroQuadrado", ProgramSource);
        Assert.Contains("PrecoFixo", ProgramSource);
        Assert.DoesNotContain("app.MapPost(\"/api/admin/eventos/{id:int}/stands/importar-mapa\"", ProgramSource);

        var updateBlock = ExtractBlock(ProgramSource, "app.MapPut(\"/api/admin/eventos/{id:int}/stands/{standId:int}\"");

        Assert.Contains("EnsureAdminAccess", updateBlock);
        Assert.Contains("NomeOcupante", updateBlock);
        Assert.Contains("Já existe um stand cadastrado com esse nome.", ProgramSource);
        Assert.Contains("NomeOcupante = @nomeOcupante COLLATE NOCASE", ProgramSource);
        Assert.Contains("Reservado", updateBlock);
        Assert.Contains("TipoArea", updateBlock);
        Assert.Contains("AreaX", updateBlock);
        Assert.Contains("UPDATE StandsEspacos", updateBlock);
        Assert.Contains("Stand não encontrado para este evento.", updateBlock);

        Assert.Contains("Salvar nome", AdminPageSource);
        Assert.Contains("HandleStandMapImageSelected", AdminPageSource);
        Assert.Contains("DropStandOnMap", AdminPageSource);
        Assert.Contains("HasDuplicateStandName", AdminPageSource);
        Assert.Contains("GetNextStandMapPoint", AdminPageSource);
        Assert.Contains("Organização automática", AdminPageSource);
        Assert.Contains("ApplyStandLayoutGrid", AdminPageSource);
        Assert.Contains("BuildStandLayoutGrid", AdminPageSource);
        Assert.DoesNotContain("2x2", AdminPageSource);
        Assert.Contains("3x3", AdminPageSource);
        Assert.Contains("4x4", AdminPageSource);
        Assert.DoesNotContain("5x5", AdminPageSource);
        Assert.DoesNotContain("8x8", AdminPageSource);
        Assert.DoesNotContain("eventStands.Count > option.Capacity", AdminPageSource);
        Assert.DoesNotContain("A organização automática suporta até 16 stands", AdminPageSource);
        Assert.Contains("StandLayoutOption", AdminPageSource);
        Assert.Contains("admin-stand-layout-tools", AdminCssSource);
        Assert.Contains("admin-stand-grid-options", AdminCssSource);
        Assert.Contains("CreateStand", AdminPageSource);
        Assert.Contains("DeleteSelectedStand", AdminPageSource);
        Assert.Contains("CreateStandSector", AdminPageSource);
        Assert.Contains("DeleteStandSector", AdminPageSource);
        Assert.Contains("DeleteStand(sectorStand)", AdminPageSource);
        Assert.Contains("MoveSelectedStandToSector", AdminPageSource);
        Assert.Contains("standEditCode", AdminPageSource);
        Assert.Contains("admin-stand-map", AdminPageSource);
        Assert.Contains("admin-stand-image-map", AdminPageSource);
        Assert.Contains("geekTopStandMap.getDropPoint", AdminPageSource);
        Assert.DoesNotContain("Detectar mapa colorido", AdminPageSource);
        Assert.DoesNotContain("Criar área faltante", AdminPageSource);
        Assert.DoesNotContain("BeginManualAreaDraw", AdminPageSource);
        Assert.DoesNotContain("DeleteSelectedStandArea", AdminPageSource);
        Assert.DoesNotContain("geekTopMapDesigner.getPoint", AdminPageSource);
        Assert.DoesNotContain("geekTopMapDetector.detect", AdminPageSource);
        Assert.DoesNotContain("BuildStandAreaStyle", AdminPageSource);
        Assert.DoesNotContain("Sugerir organização", AdminPageSource);
        Assert.DoesNotContain("SuggestStandLayout", AdminPageSource);
        Assert.DoesNotContain("BuildSuggestedStandLayout", AdminPageSource);
        Assert.DoesNotContain("GetSuggestedStandColumns", AdminPageSource);
    }

    [Fact]
    public void ReviewRisk_ReservationMapModal_ShouldUseAdminMapImageAndSupportPdfPrint()
    {
        var reservationsBlock = ExtractBlock(ProgramSource, "app.MapGet(\"/api/reservas/{cpf}\"");

        Assert.Contains("e.CidadeEvento AS EventoCidade", reservationsBlock);
        Assert.Contains("Ver mapa do evento", ReservationsPageSource);
        Assert.Contains("reservation-map-modal", ReservationsPageSource);
        Assert.Contains("SelectedMapImageUrl", ReservationsPageSource);
        Assert.Contains("HasSelectedMapImage", ReservationsPageSource);
        Assert.Contains("BuildReservationStandPinStyle", ReservationsPageSource);
        Assert.Contains("reservation-stand-image-map", ReservationsPageSource);
        Assert.Contains("reservation-stand-pin", ReservationsPageSource);
        Assert.Contains("Salvar mapa em PDF", ReservationsPageSource);
        Assert.Contains("BuildMapDownloadHref()", ReservationsPageSource);
        Assert.Contains("window.print", ReservationsPageSource);
        Assert.DoesNotContain("Baixar imagem", ReservationsPageSource);
        Assert.DoesNotContain("BuildMapImageHref()", ReservationsPageSource);
        Assert.DoesNotContain("reservation-uploaded-map", ReservationsPageSource);
        Assert.DoesNotContain("BuildMapTemplateClass", ReservationsPageSource);
        Assert.DoesNotContain("BuildMapArenaLabel", ReservationsPageSource);
        Assert.DoesNotContain("AppendSvgImageAreas", ReservationsPageSource);
        Assert.Contains("@media print", ReservationsCssSource);
        Assert.Contains("reservation-stand-image-map", ReservationsCssSource);
        Assert.Contains("reservation-stand-pin", ReservationsCssSource);
        Assert.DoesNotContain("reservation-map-visual--rio", ReservationsCssSource);
        Assert.DoesNotContain("reservation-map-visual--sao-paulo", ReservationsCssSource);
        Assert.DoesNotContain("reservation-map-visual--belo-horizonte", ReservationsCssSource);
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

    private static string GetAppFilePath(params string[] parts)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var segments = new[] { current.FullName, "src", "Alphabit.App" }
                .Concat(parts)
                .ToArray();
            var candidate = Path.Combine(segments);
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException($"Nao foi possivel localizar o arquivo '{Path.Combine(parts)}' do App.");
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
