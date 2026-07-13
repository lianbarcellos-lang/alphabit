namespace Alphabit.Tests;

public class DocumentationComplianceTests
{
    [Fact]
    public void Documentation_ShouldContainArchitectureAnalysis_WhenEvaluationRequiresArchitecturePatterns()
    {
        // Arrange
        var documentPath = GetDocsPath("analise_arquitetura.md");

        // Act
        var content = File.ReadAllText(documentPath);

        // Assert
        Assert.Contains("Padrões Arquiteturais", content);
        Assert.Contains("Arquitetura Tecnica", content);
        Assert.Contains("Arquitetura Conceitual", content);
    }

    [Fact]
    public void Documentation_ShouldContainAdrDirectory_WhenEvaluationRequiresAdrs()
    {
        // Arrange
        var adrDirectory = GetDocsPath("adrs");

        // Act
        var adrFiles = Directory.GetFiles(adrDirectory, "*.md");

        // Assert
        Assert.True(adrFiles.Length >= 3);
        Assert.Contains(adrFiles, file => File.ReadAllText(file).Contains("## Decisao"));
    }

    [Fact]
    public void Documentation_ShouldContainRiskStrategyColumn_WhenEvaluationRequiresRiskMatrix()
    {
        // Arrange
        var documentPath = GetDocsPath("operacao.md");

        // Act
        var content = File.ReadAllText(documentPath);

        // Assert
        Assert.Contains("Estratégia", content);
        Assert.Contains("Gatilhos de Risco", content);
        Assert.Contains("Check-in duplicado", content);
    }

    [Fact]
    public void Documentation_ShouldContainDoraAndQualityMetrics_WhenEvaluationRequiresMetricSheets()
    {
        // Arrange
        var doraPath = GetDocsPath("metricas_dora.md");
        var qualityPath = GetDocsPath("metricas_qualidade.md");

        // Act
        var doraContent = File.ReadAllText(doraPath);
        var qualityContent = File.ReadAllText(qualityPath);

        // Assert
        Assert.Contains("Deployment Frequency", doraContent);
        Assert.Contains("Lead Time for Changes", doraContent);
        Assert.Contains("Change Failure Rate", doraContent);
        Assert.Contains("Mean Time to Restore", doraContent);
        Assert.Contains("Metrica 1", qualityContent);
        Assert.Contains("Metrica 2", qualityContent);
    }

    [Fact]
    public void Documentation_ShouldContainSloErrorBudgetSsdfThreatModelAndDod_WhenEvaluationRequiresOperations()
    {
        // Arrange
        var requiredDocuments = new[]
        {
            "slo.md",
            "error_budget_policy.md",
            "ssdf.md",
            "threat_model_e_gates.md",
            "topologia_times.md",
            "definition_of_done.md"
        };

        // Act
        var contents = requiredDocuments
            .Select(fileName => File.ReadAllText(GetDocsPath(fileName)))
            .ToArray();

        // Assert
        Assert.Contains(contents, content => content.Contains("Ficha SLO Principal"));
        Assert.Contains(contents, content => content.Contains("Nivel 3 - Critico"));
        Assert.Contains(contents, content => content.Contains("Secure Software Development Framework"));
        Assert.Contains(contents, content => content.Contains("STRIDE"));
        Assert.Contains(contents, content => content.Contains("Topologia"));
        Assert.Contains(contents, content => content.Contains("DoD Geral"));
    }

    private static string GetDocsPath(params string[] parts)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs"));
        return Path.Combine(new[] { root }.Concat(parts).ToArray());
    }
}
