using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AgroSolutions.ArchitectureTests;

/// <summary>
/// Testes de arquitetura para garantir aderência aos princípios SOLID e boas práticas
/// FOCO: Violações CRÍTICAS que impactam diretamente a qualidade do código
/// </summary>
public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Propriedades.Program).Assembly,
            typeof(IngestaoDados.Program).Assembly,
            typeof(Analise.Program).Assembly,
            typeof(Notificacoes.Program).Assembly,
            typeof(ProcessamentoDados.Program).Assembly,
            typeof(Identidade.Program).Assembly
        )
        .Build();

    #region CRÍTICO - Dependency Inversion Principle

    [Fact(DisplayName = "CRÍTICO: Services NÃO devem depender de DbContext diretamente")]
    public void Services_ShouldNot_DependOnDbContextDirectly()
    {
        var rule = Classes()
            .That().HaveNameEndingWith("Service")
            .And().DoNotHaveNameEndingWith("ConsumerService")
            .And().DoNotHaveNameEndingWith("BackgroundService")
            .Should().NotDependOnAny(typeof(Microsoft.EntityFrameworkCore.DbContext));

        Assert.True(rule.HasNoViolations(Architecture), 
            "VIOLAÇÃO CRÍTICA: Services devem usar Repositories, não DbContext diretamente (DIP)");
    }

    [Fact(DisplayName = "CRÍTICO: Controllers NÃO devem depender de DbContext (exceto testes Pact)")]
    public void Controllers_ShouldNot_DependOnDbContext()
    {
        var rule = Classes()
            .That().HaveNameEndingWith("Controller")
            .And().DoNotHaveName("ProviderStatesController") // Exceção: Controller de testes Pact
            .Should().NotDependOnAny(typeof(Microsoft.EntityFrameworkCore.DbContext));

        Assert.True(rule.HasNoViolations(Architecture),
            "VIOLAÇÃO CRÍTICA: Controllers não devem acessar DbContext diretamente (use Services)");
    }

    #endregion

    #region RECOMENDADO - Naming Conventions

    [Fact(DisplayName = "Interfaces devem começar com 'I'")]
    public void Interfaces_Should_StartWithI()
    {
        var rule = Interfaces()
            .Should().HaveNameStartingWith("I");

        Assert.True(rule.HasNoViolations(Architecture),
            "Interfaces devem começar com 'I' por convenção C#");
    }

    [Fact(DisplayName = "Controllers devem estar em namespace Controllers")]
    public void Controllers_Should_BeInCorrectNamespace()
    {
        var rule = Classes()
            .That().HaveNameEndingWith("Controller")
            .Should().ResideInNamespaceMatching(".*Controllers.*");

        Assert.True(rule.HasNoViolations(Architecture),
            "Controllers devem estar organizados no namespace Controllers");
    }

    #endregion
}