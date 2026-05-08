namespace TicketPrime.Tests;

public class TicketPrimeRiskTests
{
    private static readonly string ProgramSource = File.ReadAllText(GetApiFilePath("Program.cs"));
    private static readonly string RulesSource = File.ReadAllText(GetApiFilePath("TicketPrimeRules.cs"));

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
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Login\")", ProgramSource);
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Password\")", ProgramSource);
        Assert.Contains("GetRequiredConfiguration(builder.Configuration, \"AdminAccess:Token\")", ProgramSource);
        Assert.DoesNotContain("?? \"admin\"", ProgramSource);
        Assert.DoesNotContain("?? \"ticketprime-admin-token\"", ProgramSource);
    }

    private static string GetApiFilePath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "TicketPrime.API", fileName);
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
