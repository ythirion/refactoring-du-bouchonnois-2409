using Bogus;

using Bouchonnois.Domain;
using Bouchonnois.Service;

using static Bouchonnois.Domain.PartieStatus;

namespace Bouchonnois.Tests.Builders;

public class PartieDeChasseBuilder
{
    private readonly List<Chasseur> _chasseurs = [];
    private readonly Terrain _terrain = new() {Nom = _faker.Address.City()};
    private PartieStatus _status;
    private static readonly Faker _faker = new("fr");

    public PartieDeChasseBuilder AvecDesChasseurs(params ChasseurBuilder[] builders)
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
            Status = _status,
            Events = [],
        };
    }

    public PartieDeChasseBuilder Terminée()
    {
        _status = PartieStatus.Terminée;
        return this;
    }

    public PartieDeChasseBuilder ALapéro()
    {
        _status = Apéro;
        return this;
    }

    public PartieDeChasseBuilder EtUnTerrainAvecDesGalinettes(int nbGalinettes = 3)
    {
        _terrain.NbGalinettes = nbGalinettes;
        return this;
    }

    public static PartieDeChasseBuilder UnePartieDeChasse()
    {
        return new PartieDeChasseBuilder();
    }
}