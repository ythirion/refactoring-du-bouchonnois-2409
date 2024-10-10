using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Builders;
using Bouchonnois.Tests.Doubles;

using static Bouchonnois.Tests.Builders.PartieDeChasseDataBuilder;
using static Bouchonnois.Tests.Builders.ChasseurBuilder;
using static Bouchonnois.Tests.Assertions.PartieDeChasseExtensions;

namespace Bouchonnois.Tests.Service;

public class TirerTest : PartieDeChasseServiceTests
{
    [Fact]
    public void AvecUnChasseurAyantDesBalles()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var nbGalinettes = 2;
        var unePartieDeChasseAvecDesChasseursAyantDesBalles = UnePartieDeChasse()
            .AvecDesChasseursAyantDesBalles(Dédé(), Bernard(), Robert())
            .EtUnTerrainAvecDesGalinettes(nbGalinettes)
            .Build();

        repository.Add(unePartieDeChasseAvecDesChasseursAyantDesBalles);
        var service = new PartieDeChasseService(repository, TimeProvider);

        service.Tirer(unePartieDeChasseAvecDesChasseursAyantDesBalles.Id, Chasseurs.Bernard);

        repository
            .HasSavedPartieDeChasse()
            .Should()
            .HasEmittedEvent(Now, $"{Chasseurs.Bernard} tire")
            .And
            .ChasseurATiré(Chasseurs.Bernard, expectedBallesRestantes: 7)
            .And
            .TerrainAEncoreDesGalinettes(nbGalinettes);
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
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 0,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.EnCours,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action tirerSansBalle = () => service.Tirer(id, "Bernard");

        tirerSansBalle.Should()
            .Throw<TasPlusDeBallesMonVieuxChasseALaMain>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Bernard tire -> T'as plus de balles mon vieux, chasse à la main");
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
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.EnCours,
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        var nomChasseurInconnu = "Chasseur inconnu";

        Action? chasseurInconnuVeutTirer = () => service.Tirer(id, nomChasseurInconnu);

        chasseurInconnuVeutTirer.Should()
            .Throw<ChasseurInconnu>()
            .WithMessage($"Chasseur inconnu {nomChasseurInconnu}");

        repository.HasSavedPartieDeChasse()
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

        repository.HasSavedPartieDeChasse()
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
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.Terminée,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerQuandTerminée = () => service.Tirer(id, "Chasseur inconnu");

        tirerQuandTerminée.Should()
            .Throw<OnTirePasQuandLaPartieEstTerminée>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Chasseur inconnu veut tirer -> On tire pas quand la partie est terminée");
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
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.Apéro,
            Events = [],
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? tirerEnPleinApéro = () => service.Tirer(id, "Chasseur inconnu");

        tirerEnPleinApéro.Should()
            .Throw<OnTirePasPendantLapéroCestSacré>();

        AssertLastEvent(repository.HasSavedPartieDeChasse(),
            "Chasseur inconnu veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");
    }
}