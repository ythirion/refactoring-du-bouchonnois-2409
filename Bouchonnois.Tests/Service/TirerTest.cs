using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Service;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;

using FsCheck;
using FsCheck.Xunit;

using Microsoft.FSharp.Collections;
using FluentAssertions.LanguageExt;

using static Bouchonnois.Tests.Builders.PartieDeChasseBuilder;
using static Bouchonnois.Tests.Builders.ChasseurBuilder;
using static Bouchonnois.Tests.Assertions.PartieDeChasseExtensions;
using LanguageExt;

namespace Bouchonnois.Tests.Service;

public class TirerTest : PartieDeChasseServiceTests
{
    private readonly PartieDeChasseRepositoryForTests _repository;
    private readonly TirerUseCase _tirerUseCase;

    public TirerTest()
    {
        _repository = new PartieDeChasseRepositoryForTests();
        _tirerUseCase = new TirerUseCase(_repository, TimeProvider);
    }

    [Fact]
    public void AvecUnChasseurAyantDesBalles()
    {
        var nbGalinettes = 2;
        var unePartieDeChasseAvecDesChasseursAyantDesBalles = UnePartieDeChasse()
            .AvecDesChasseurs(Dédé(), Bernard(), Robert())
            .EtUnTerrainAvecDesGalinettes(nbGalinettes)
            .Build();
        _repository.Add(unePartieDeChasseAvecDesChasseursAyantDesBalles);

        _tirerUseCase.Handle(new TirerCommand(unePartieDeChasseAvecDesChasseursAyantDesBalles.Id, Chasseurs.Bernard));

        _repository
            .SavedPartieDeChasse()
            .Should()
            .HaveEmittedEvent(Now, $"{Chasseurs.Bernard} tire")
            .And
            .ChasseurATiré(Chasseurs.Bernard, expectedBallesRestantes: 7)
            .And
            .TerrainAEncoreDesGalinettes(nbGalinettes);
    }

    [Fact]
    public void EchoueAvecUnChasseurNayantPlusDeBalles()
    {
        var partieDeChasse = UnePartieDeChasse()
            .AvecDesChasseurs(Dédé(),
                Bernard()
                    .SansBalles()
            )
            .EtUnTerrainAvecDesGalinettes()
            .Build();
        _repository.Add(partieDeChasse);

        Action tirerSansBalle = () => _tirerUseCase.Handle(new TirerCommand(partieDeChasse.Id, Chasseurs.Bernard));

        tirerSansBalle.Should()
            .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

        AssertLastEvent(
            _repository.SavedPartieDeChasse(),
            "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"
        );
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
    {
        var partieDeChasse = UnePartieDeChasse()
            .AvecDesChasseurs(
                Dédé()
            )
            .EtUnTerrainAvecDesGalinettes()
            .Build();
        _repository.Add(partieDeChasse);

        var nomChasseurInconnu = "Chasseur inconnu";
        Action chasseurInconnuVeutTirer = () => _tirerUseCase.Handle(new TirerCommand(partieDeChasse.Id, nomChasseurInconnu));

        chasseurInconnuVeutTirer.Should()
            .Throw<ChasseurInconnu>()
            .WithMessage($"Chasseur inconnu {nomChasseurInconnu}");

        _repository.HasNotSavedPartieDeChasse()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        Action? tirerQuandPartieExistePas = () => _tirerUseCase.Handle(new TirerCommand(id, "Bernard"));

        tirerQuandPartieExistePas.Should()
            .Throw<LaPartieDeChasseNexistePas>();

        _repository.HasNotSavedPartieDeChasse()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstTerminée()
    {
        var partieDeChasse = UnePartieDeChasse()
            .AvecDesChasseurs(
                Dédé(),
                Bernard()
            )
            .EtUnTerrainAvecDesGalinettes()
            .Terminée()
            .Build();
        _repository.Add(partieDeChasse);

        Action tirerQuandTerminée = () => _tirerUseCase.Handle(new TirerCommand(partieDeChasse.Id, Chasseurs.Bernard));

        tirerQuandTerminée.Should()
            .Throw<OnTirePasQuandLaPartieEstTerminée>();

        AssertLastEvent(_repository.SavedPartieDeChasse(),
            "Bernard veut tirer -> On tire pas quand la partie est terminée");
    }

    [Fact]
    public void EchoueSiLesChasseursSontEnApero()
    {
        var partieDeChasse = UnePartieDeChasse()
            .AvecDesChasseurs(
                Dédé(),
                Bernard()
            )
            .EtUnTerrainAvecDesGalinettes()
            .ALapéro()
            .Build();
        _repository.Add(partieDeChasse);

        Action? tirerEnPleinApéro = () => _tirerUseCase.Handle(new TirerCommand(partieDeChasse.Id, Chasseurs.Bernard));

        tirerEnPleinApéro.Should()
            .Throw<OnTirePasPendantLapéroCestSacré>();

        AssertLastEvent(_repository.SavedPartieDeChasse(),
            "Bernard veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
    }

    [Fact]
    public void TirerUseCase_Should_Not_Return()
    {
        var partieDeChasseId = Guid.NewGuid();
        // Act
        var result = _tirerUseCase.HandleSansException(new TirerCommand(partieDeChasseId, "Bernard"));

        // Assert
        result.Should().BeLeft(); // Par convention Left contient le cas d'erreur
        result.LeftUnsafe().Message.Should().Be($"La partie de chasse {partieDeChasseId} n'existe pas");
        _repository.SavedPartieDeChasse().Should().BeNull();
    }
    
    [Fact]
    public void TirerUseCase_ShouldSucceed()
    {
        int nbGalinettes = 22;
        var unePartieDeChasseAvecDesChasseursAyantDesBalles = UnePartieDeChasse()
            .AvecDesChasseurs(Dédé(), Bernard(), Robert())
            .EtUnTerrainAvecDesGalinettes(nbGalinettes)
            .Build();
        _repository.Add(unePartieDeChasseAvecDesChasseursAyantDesBalles);
        
        // Act
        var result = _tirerUseCase.HandleSansException(
            new TirerCommand(unePartieDeChasseAvecDesChasseursAyantDesBalles.Id, "Bernard"));

        // Assert
        result.Should().BeRight();
        
        _repository
            .SavedPartieDeChasse()
            .Should()
            .HaveEmittedEvent(Now, $"{Chasseurs.Bernard} tire")
            .And
            .ChasseurATiré(Chasseurs.Bernard, expectedBallesRestantes: 7)
            .And
            .TerrainAEncoreDesGalinettes(nbGalinettes);
    }

    #region Properties

    [Property]
    public Property PourNimporteQuellePartieDeChasseQuandUnChasseurSansBalleTireCestUnEchec()
        => Prop.ForAll(TerrainRicheEnGalinettesGenerator(), ChasseursSansBallesGenerator(),
            (terrain, chasseurs) => EchoueAvec<TasPlusDeBallesMonVieuxChasseALaMain>(service =>
                {
                    var partieDeChasse = UnePartieDeChasse()
                        .AvecDesChasseurs(chasseurs.ToChasseurBuilders())
                        .EtUnTerrainAvecDesGalinettes(terrain.nbGalinettes)
                        .Build();

                    _repository.Add(partieDeChasse);
                    service.Handle(new TirerCommand(partieDeChasse.Id, chasseurs.Head.nom));
                }
            ));

    private bool EchoueAvec<T>(Action<TirerUseCase> action)
        where T : Exception
    {
        try
        {
            action(_tirerUseCase);
            return false;
        }
        catch ( T )
        {
            return true;
        }
    }

    private static Arbitrary<(string nom, int nbGalinettes)> TerrainGenerator(int minGalinettes, int maxGalinettes)
        => (from nom in Arb.Generate<string>()
            from nbGalinette in Gen.Choose(minGalinettes, maxGalinettes)
            select (nom, nbGalinette)).ToArbitrary();

    protected static Arbitrary<(string nom, int nbGalinettes)> TerrainRicheEnGalinettesGenerator()
        => TerrainGenerator(1, int.MaxValue);

    protected static Arbitrary<(string nom, int nbGalinettes)> TerrainSansGalinettesGenerator()
        => TerrainGenerator(-int.MaxValue, 0);

    private static Arbitrary<(string nom, int nbBalles)> ChasseurGenerator(int minBalles, int maxBalles)
        => (from nom in Arb.Generate<string>()
            from nbBalles in Gen.Choose(minBalles, maxBalles)
            select (nom, nbBalles)).ToArbitrary();

    private static Arbitrary<FSharpList<(string nom, int nbBalles)>> GroupeDeChasseursGenerator(int minBalles,
        int maxBalles)
        => (from nbChasseurs in Gen.Choose(1, 1_000)
            select ChasseurGenerator(minBalles, maxBalles)
                .Generator.Sample(1, nbChasseurs)).ToArbitrary();

    protected static Arbitrary<FSharpList<(string nom, int nbBalles)>> ChasseursAvecBallesGenerator()
        => GroupeDeChasseursGenerator(1, int.MaxValue);

    protected static Arbitrary<FSharpList<(string nom, int nbBalles)>> ChasseursSansBallesGenerator()
        => GroupeDeChasseursGenerator(0, 0);

    #endregion
}

public static class PartieDeChasseExtensions
{
    public static ChasseurBuilder[] ToChasseurBuilders(this FSharpList<(string nom, int nbBalles)> chasseurs)
        => chasseurs.Select(c =>
                UnChasseur(c.nom)
                    .AvecDesBalles(c.nbBalles)
            )
            .ToArray();
}

public static class LangExtExtensions
{
    public static TRight RightUnsafe<TLeft, TRight>(this Either<TLeft, TRight> either) => either.RightToSeq()[0];
    public static TLeft LeftUnsafe<TLeft, TRight>(this Either<TLeft, TRight> either) => either.LeftToSeq()[0];
}