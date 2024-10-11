using ArchUnitNET.Fluent.Syntax.Elements.Members.MethodMembers;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouchonnois.Tests.Architecture
{
    public class LinguisticAntiPatterns
    {
        private static GivenMethodMembersThat Methods()
            => MethodMembers()
                .That()
                .AreNoConstructors()
                .And();

        [Fact]
        public void NoGetMethodShouldReturnVoid()
            => Methods()
                .HaveName("Get[A-Z].*", useRegularExpressions: true)
                .Should()
                .NotHaveReturnType(typeof(void))
                .Check();

        [Fact]
        public void IserAndHaserShouldReturnBooleans()
            => Methods()
                .HaveName("Is[A-Z].*|Has[A-Z].*", useRegularExpressions: true)
                .Should()
                .HaveReturnType(typeof(bool))
                .WithoutRequiringPositiveResults()
                .Check();
    }
}