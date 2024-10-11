using Bouchonnois.Domain;

using FluentAssertions.Primitives;

namespace Bouchonnois.Tests.Assertions;

public static class PartieDeChasseExtensions
{
    public static PartieDeChasseAssertions Should(this PartieDeChasse partieDeChasse) => new(partieDeChasse);
}

public class PartieDeChasseAssertions : ReferenceTypeAssertions<PartieDeChasse, PartieDeChasseAssertions>
{
    protected override string Identifier => "PartieDeChasse";

    public PartieDeChasseAssertions(PartieDeChasse partieDeChasse)
        : base(partieDeChasse)
    {
    }

    public AndConstraint<PartieDeChasseAssertions> HasEmittedEvent(DateTime expectedDate, string expectedMessage)
    {
        Subject
            .Events
            .Should()
            .ContainEquivalentOf(new Event(expectedDate, expectedMessage));

        return new AndConstraint<PartieDeChasseAssertions>(this);
    }

    public AndConstraint<PartieDeChasseAssertions> ChasseurATiré(string chasseur,
        int expectedBallesRestantes)
    {
        Subject.Chasseurs
            .First(c => c.Nom == chasseur)
            .BallesRestantes.Should()
            .Be(expectedBallesRestantes);

        return new AndConstraint<PartieDeChasseAssertions>(this);
    }

    public AndConstraint<PartieDeChasseAssertions> TerrainAEncoreDesGalinettes(int expectedGalinettes)
    {
        Subject.Terrain
            .NbGalinettes
            .Should()
            .Be(expectedGalinettes);

        return new AndConstraint<PartieDeChasseAssertions>(this);
    }
}