using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Service;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service;

public class ConsulterStatusTest : PartieDeChasseServiceTests
{
    [Fact]
    public void EchoueCarPartieNexistePas()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        Func<string>? reprendrePartieQuandPartieExistePas = () => service.ConsulterStatus(id);

        reprendrePartieQuandPartieExistePas.Should()
            .Throw<LaPartieDeChasseNexistePas>();

        repository.SavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void QuandLaPartieEstTerminée()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.EnCours,
            Events =
            [
                new Event(new DateTime(2024, 4, 25, 9, 0, 12),
                    "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),

                new Event(new DateTime(2024, 4, 25, 9, 10, 0), "Dédé tire"),
                new Event(new DateTime(2024, 4, 25, 9, 40, 0), "Robert tire sur une galinette"),
                new Event(new DateTime(2024, 4, 25, 10, 0, 0), "Petit apéro"),
                new Event(new DateTime(2024, 4, 25, 11, 0, 0), "Reprise de la chasse"),
                new Event(new DateTime(2024, 4, 25, 11, 2, 0), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 11, 3, 0), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 11, 4, 0), "Dédé tire sur une galinette"),
                new Event(new DateTime(2024, 4, 25, 11, 30, 0), "Robert tire sur une galinette"),
                new Event(new DateTime(2024, 4, 25, 11, 40, 0), "Petit apéro"),
                new Event(new DateTime(2024, 4, 25, 14, 30, 0), "Reprise de la chasse"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 0), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 1), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 2), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 3), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 4), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 5), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 6), "Bernard tire"),
                new Event(new DateTime(2024, 4, 25, 14, 41, 7),
                    "Bernard tire -> T'as plus de balles mon vieux, chasse à la main"),

                new Event(new DateTime(2024, 4, 25, 15, 0, 0), "Robert tire sur une galinette"),
                new Event(new DateTime(2024, 4, 25, 15, 30, 0),
                    "La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes"),
            ],
        });

        var status = service.ConsulterStatus(id);

        status.Should()
            .BeEquivalentTo(@"15:30 - La partie de chasse est terminée, vainqueur :  Robert - 3 galinettes
15:00 - Robert tire sur une galinette
14:41 - Bernard tire -> T'as plus de balles mon vieux, chasse à la main
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:41 - Bernard tire
14:30 - Reprise de la chasse
11:40 - Petit apéro
11:30 - Robert tire sur une galinette
11:04 - Dédé tire sur une galinette
11:03 - Bernard tire
11:02 - Bernard tire
11:00 - Reprise de la chasse
10:00 - Petit apéro
09:40 - Robert tire sur une galinette
09:10 - Dédé tire
09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
    }

    [Fact]
    public void QuandLaPartieVientDeDémarrer()
    {
        var id = Guid.NewGuid();
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);

        repository.Add(new PartieDeChasse
        {
            Id = id,
            Chasseurs =
            [
                new Chasseur {Nom = "Dédé", BallesRestantes = 20,},
                new Chasseur {Nom = "Bernard", BallesRestantes = 8,},
                new Chasseur {Nom = "Robert", BallesRestantes = 12, NbGalinettes = 2,},
            ],
            Terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,},
            Status = PartieStatus.EnCours,
            Events =
            [
                new Event(new DateTime(2024, 4, 25, 9, 0, 12),
                    "La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)"),
            ],
        });

        var status = service.ConsulterStatus(id);

        status.Should()
            .Be(
                "09:00 - La partie de chasse commence à Pitibon sur Sauldre avec Dédé (20 balles), Bernard (8 balles), Robert (12 balles)");
    }
}