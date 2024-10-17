using Bouchonnois.Domain.Exceptions;
using Bouchonnois.Service;
using Bouchonnois.Tests.Doubles;

namespace Bouchonnois.Tests.Service;

public class ScenarioTests
{
    [Fact]
    public async Task DeroulerUnePartie()
    {
        var time = new DateTime(2024, 4, 25, 9, 0, 0);
        var repository = new PartieDeChasseRepositoryForTests();
        var service = new PartieDeChasseService(repository, () => time);
        var tirerUseCase = new TirerUseCase(repository, () => time);
        var chasseurs = new List<(string, int)> {("Dédé", 20), ("Bernard", 8), ("Robert", 12),};
        var terrainDeChasse = ("Pitibon sur Sauldre", 4);

        Guid id = service.Demarrer(
            terrainDeChasse,
            chasseurs
        );

        time = time.Add(TimeSpan.FromMinutes(10));
        tirerUseCase.Tirer(id, "Dédé");

        time = time.Add(TimeSpan.FromMinutes(30));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(20));
        service.PrendreLapéro(id);

        time = time.Add(TimeSpan.FromHours(1));
        service.ReprendreLaPartie(id);

        time = time.Add(TimeSpan.FromMinutes(2));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromMinutes(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromMinutes(1));
        service.TirerSurUneGalinette(id, "Dédé");

        time = time.Add(TimeSpan.FromMinutes(26));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(10));
        service.PrendreLapéro(id);

        time = time.Add(TimeSpan.FromMinutes(170));
        service.ReprendreLaPartie(id);

        time = time.Add(TimeSpan.FromMinutes(11));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));
        tirerUseCase.Tirer(id, "Bernard");

        time = time.Add(TimeSpan.FromSeconds(1));

        try
        {
            tirerUseCase.Tirer(id, "Bernard");
        }
        catch ( TasPlusDeBallesMonVieuxChasseALaMain )
        {
        }

        time = time.Add(TimeSpan.FromMinutes(19));
        service.TirerSurUneGalinette(id, "Robert");

        time = time.Add(TimeSpan.FromMinutes(30));
        service.TerminerLaPartie(id);

        await Verify(service.ConsulterStatus(id));
    }
}