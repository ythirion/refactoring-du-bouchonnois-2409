using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

using static Bouchonnois.Tests.Service.PartieDeChasseDataBuilder;
using static Bouchonnois.Tests.Service.ChasseurBuilder;

namespace Bouchonnois.Tests.Service;

public class PartieDeChasseDataBuilder
{
    private readonly List<Chasseur> _chasseurs = [];
    private Terrain _terrain = new() { Nom = "Pitibon sur Sauldre", };

    public PartieDeChasseDataBuilder AvecDesChasseursAyantDesBalles(params ChasseurBuilder[] builders)
    {
        foreach ( var builder in builders )
        {
            _chasseurs.Add(builder.Build());
        }

        return this;
    }

    public PartieDeChasse Build()
    {
        return new PartieDeChasse
        {
            Id = Guid.NewGuid(),
            Chasseurs = _chasseurs,
            Terrain = _terrain,
            Status = PartieStatus.EnCours,
            Events = [],
        };
    }

    public PartieDeChasseDataBuilder EtUnTerrainAvecDesGalinettes()
    {
        _terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, };

        return this;
    }

    public static PartieDeChasseDataBuilder UnePartieDeChasse()
    {
        return new PartieDeChasseDataBuilder();
    }
}
public class TirerTest : PartieDeChasseServiceTests
{
    [Fact]
    public void AvecUnChasseurAyantDesBalles()
    {
        var repository = new PartieDeChasseRepositoryForTests();

        PartieDeChasse unePartieDeChasseAvecDesChasseursAyantDesBalles = UnePartieDeChasse()
                                                                         .AvecDesChasseursAyantDesBalles(Dédé(), Bernard(), Robert())
                                                                         .EtUnTerrainAvecDesGalinettes()
                                                                         .Build();

        Guid id = unePartieDeChasseAvecDesChasseursAyantDesBalles.Id;

        repository.Add(unePartieDeChasseAvecDesChasseursAyantDesBalles);

        var service = new PartieDeChasseService(repository, TimeProvider);

        service.Tirer(id, "Bernard");

        PartieDeChasse? savedPartieDeChasse = repository.SavedPartieDeChasse();

        savedPartieDeChasse.Id.Should()
                           .Be(id);

        savedPartieDeChasse.Status.Should()
                           .Be(PartieStatus.EnCours);

        savedPartieDeChasse.Terrain.Nom.Should()
                           .Be("Pitibon sur Sauldre");

        savedPartieDeChasse.Terrain.NbGalinettes.Should()
                           .Be(3);

        savedPartieDeChasse.Chasseurs.Should()
                           .HaveCount(3);

        savedPartieDeChasse.Chasseurs[0]
                           .Nom.Should()
                           .Be("Dédé");

        savedPartieDeChasse.Chasseurs[0]
                           .BallesRestantes.Should()
                           .Be(20);

        savedPartieDeChasse.Chasseurs[0]
                           .NbGalinettes.Should()
                           .Be(0);

        savedPartieDeChasse.Chasseurs[1]
                           .Nom.Should()
                           .Be("Bernard");

        savedPartieDeChasse.Chasseurs[1]
                           .BallesRestantes.Should()
                           .Be(7);

        savedPartieDeChasse.Chasseurs[1]
                           .NbGalinettes.Should()
                           .Be(0);

        savedPartieDeChasse.Chasseurs[2]
                           .Nom.Should()
                           .Be("Robert");

        savedPartieDeChasse.Chasseurs[2]
                           .BallesRestantes.Should()
                           .Be(12);

        savedPartieDeChasse.Chasseurs[2]
                           .NbGalinettes.Should()
                           .Be(0);
    }

    [Fact]
    public void EchoueAvecUnChasseurNayantPlusDeBalles()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
                           [
                               new Chasseur { Nom = "Dédé", BallesRestantes = 20, },
                               new Chasseur { Nom = "Bernard", BallesRestantes = 0, },
                               new Chasseur { Nom = "Robert", BallesRestantes = 12, },
                           ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.EnCours,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action tirerSansBalle = () => service.Tirer(id, "Bernard");

        tirerSansBalle.Should()
                      .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

        AssertLastEvent(repository.SavedPartieDeChasse(), "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
    }

    [Fact]
    public void EchoueCarLeChasseurNestPasDansLaPartie()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
                           [
                               new Chasseur { Nom = "Dédé", BallesRestantes = 20, },
                               new Chasseur { Nom = "Bernard", BallesRestantes = 8, },
                               new Chasseur { Nom = "Robert", BallesRestantes = 12, },
                           ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.EnCours,
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        var nomChasseurInconnu = "Chasseur inconnu";

        Action? chasseurInconnuVeutTirer = () => service.Tirer(id, nomChasseurInconnu);

        chasseurInconnuVeutTirer.Should()
                                .Throw<ChasseurInconnu>()
                                .WithMessage($"Chasseur inconnu {nomChasseurInconnu}");

        repository.SavedPartieDeChasse()
                  .Should()
                  .BeNull();
    }

    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerQuandPartieExistePas = () => service.Tirer(id, "Bernard");

        tirerQuandPartieExistePas.Should()
                                 .Throw<LaPartieDeChasseNexistePas>();

        repository.SavedPartieDeChasse()
                  .Should()
                  .BeNull();
    }

    [Fact]
    public void EchoueSiLaPartieDeChasseEstTerminée()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
                           [
                               new Chasseur { Nom = "Dédé", BallesRestantes = 20, },
                               new Chasseur { Nom = "Bernard", BallesRestantes = 8, },
                               new Chasseur { Nom = "Robert", BallesRestantes = 12, },
                           ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.Terminée,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

        tirerQuandTerminée.Should()
                          .Throw<OnTirePasQuandLaPartieEstTerminée>();

        AssertLastEvent(repository.SavedPartieDeChasse(), "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
    }

    [Fact]
    public void EchoueSiLesChasseursSontEnApero()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
                           [
                               new Chasseur { Nom = "Dédé", BallesRestantes = 20, },
                               new Chasseur { Nom = "Bernard", BallesRestantes = 8, },
                               new Chasseur { Nom = "Robert", BallesRestantes = 12, },
                           ],
            Terrain = new Terrain { Nom = "Pitibon sur Sauldre", NbGalinettes = 3, },
            Status = PartieStatus.Apéro,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

        tirerEnPleinApéro.Should()
                         .Throw<OnTirePasPendantLapéroCestSacré>();

        AssertLastEvent(repository.SavedPartieDeChasse(), "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
    }
}