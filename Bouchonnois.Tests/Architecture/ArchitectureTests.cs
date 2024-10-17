using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types;

namespace Bouchonnois.Tests.Architecture;

[Trait("Architecture", "Clean Architecture")]
public class ArchitectureTests
{
    private static GivenTypesConjunction TypesIn(string @namespace) =>
        ArchRuleDefinition.Types()
            .That()
            .ResideInNamespace(@namespace, true);

    /// <summary>
    /// This is a summary with an image:
    /// ![Dependency Rule](https://github.com/ythirion/refactoring-du-bouchonnois/raw/main/facilitation/steps/img/07.architecture-tests/onion.webp)
    /// </summary>
    [Fact(DisplayName = "Lower layers can not depend on outer layers")]
    public void CheckInwardDependencies()
        => Domain()
            .Should()
            .OnlyDependOn(Domain())
            .And(
                ApplicationServices()
                    .Should()
                    .NotDependOnAny(Infrastructure())
            )
            .Check();

    private static GivenTypesConjunctionWithDescription ApplicationServices() =>
        TypesIn("Service")
            .As("Application Business Rules");

    private static GivenTypesConjunctionWithDescription Infrastructure() =>
        TypesIn("Repository")
            .As("Framework & Drivers");

    private static GivenTypesConjunctionWithDescription Domain() =>
        TypesIn("Domain")
            .As("Enterprise Business Rules");
}