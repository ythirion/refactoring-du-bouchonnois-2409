using Bogus;

using Bouchonnois.Domain;
using Bouchonnois.Service;

namespace Bouchonnois.Tests.Builders;

public class PartieDeChasseDataBuilder
{
    private readonly List<Chasseur> _chasseurs = [];
    private Terrain _terrain = new() {Nom = _faker.Address.City()};
    private static readonly Faker _faker = new("fr");

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
        _terrain = new Terrain {Nom = "Pitibon sur Sauldre", NbGalinettes = 3,};

        return this;
    }

    public static PartieDeChasseDataBuilder UnePartieDeChasse()
    {
        return new PartieDeChasseDataBuilder();
    }
}