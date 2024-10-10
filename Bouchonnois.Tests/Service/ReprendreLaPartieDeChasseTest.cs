using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service;

public class ReprendreLaPartieDeChasseTest : PartieDeChasseServiceTests
{
    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? reprendrePartieQuandPartieExistePas = () => service.ReprendreLaPartie(id);

        reprendrePartieQuandPartieExistePas.Should()
            .Throw<LaPartieDeChasseNexistePas>();

        repository.SavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void EchoueSiLaChasseEstEnCours()
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
        Action? reprendreLaPartieQuandChasseEnCours = () => service.ReprendreLaPartie(id);

        reprendreLaPartieQuandChasseEnCours.Should()
            .Throw<LaChasseEstDéjàEnCours>();

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
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.Terminée,
        });

        var service = new PartieDeChasseService(repository, TimeProvider);
        Action? prendreLapéroQuandTerminée = () => service.ReprendreLaPartie(id);

        prendreLapéroQuandTerminée.Should()
            .Throw<QuandCestFiniCestFini>();

        repository.SavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void QuandLapéroEstEnCours()
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
        service.ReprendreLaPartie(id);

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
            .Be(8);

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
}