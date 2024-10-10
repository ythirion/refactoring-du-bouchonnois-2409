using Bouchonnois.Domain;
using Bouchonnois.Service;
using Bouchonnois.Service.Exceptions;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service;

public class DemarrerUnePartieDeChasseTest : PartieDeChasseServiceTests
{
    [Fact]
    public void AvecPlusieursChasseurs()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        var chasseurs = new List<(string, int)> {("Dédé", 20), ("Bernard", 8), ("Robert", 12),};
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        Guid id = service.Demarrer(
            terrainDeChasse,
            chasseurs
        );

        PartieDeChasse? savedPartieDeChasse = repository.HasSavedPartieDeChasse();

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

    [Fact]
    public void EchoueAvecUnTerrainSansGalinettes()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        var chasseurs = new List<(string, int)>();
        var terrainDeChasse = ("Pitibon sur Sauldre", 0);

        Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

        demarrerPartieSansChasseurs.Should()
            .Throw<ImpossibleDeDémarrerUnePartieSansGalinettes>();
    }

    [Fact]
    public void EchoueSansChasseurs()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        var chasseurs = new List<(string, int)>();
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        Action demarrerPartieSansChasseurs = () => service.Demarrer(terrainDeChasse, chasseurs);

        demarrerPartieSansChasseurs.Should()
            .Throw<ImpossibleDeDémarrerUnePartieSansChasseur>();

        repository.HasSavedPartieDeChasse()
            .Should()
            .BeNull();
    }

    [Fact]
    public void EchoueSiChasseurSansBalle()
    {
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, TimeProvider);
        var chasseurs = new List<(string, int)> {("Dédé", 20), ("Bernard", 0),};
        var terrainDeChasse = ("Pitibon sur Sauldre", 3);

        Action demarrerPartieAvecChasseurSansBalle = () => service.Demarrer(terrainDeChasse, chasseurs);

        demarrerPartieAvecChasseurSansBalle.Should()
            .Throw<ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle>();

        repository.HasSavedPartieDeChasse()
            .Should()
            .BeNull();
    }
}