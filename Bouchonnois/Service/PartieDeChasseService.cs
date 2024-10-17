using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Service;

public class PartieDeChasseService
{
    private readonly IPartieDeChasseRepository _repository;
    private readonly Func<DateTime> _timeProvider;

    public PartieDeChasseService(
        IPartieDeChasseRepository repository,
        Func<DateTime> timeProvider)
    {
        _timeProvider = timeProvider;
        _repository = repository;
    }

    public string ConsulterStatus(Guid id)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id);

        if ( partieDeChasse == null )
        {
            throw new LaPartieDeChasseNexistePas();
        }

        return string.Join(
            Environment.NewLine,
            partieDeChasse.Events
                .OrderByDescending(@event => @event.Date)
                .Select(@event => @event.ToString())
        );
    }

    public Guid Demarrer((string nom, int nbGalinettes) terrainDeChasse, List<(string nom, int nbBalles)> chasseurs)
    {
        if ( terrainDeChasse.nbGalinettes <= 0 )
        {
            throw new ImpossibleDeDémarrerUnePartieSansGalinettes();
        }

        var partieDeChasse = new PartieDeChasse
        {
            Id = Guid.NewGuid(),
            Status = PartieStatus.EnCours,
            Chasseurs = [],
            Terrain = new Terrain {Nom = terrainDeChasse.nom, NbGalinettes = terrainDeChasse.nbGalinettes,},
            Events = [],
        };

        foreach ( (string nom, int nbBalles) chasseur in chasseurs )
        {
            if ( chasseur.nbBalles == 0 )
            {
                throw new ImpossibleDeDémarrerUnePartieAvecUnChasseurSansBalle();
            }

            partieDeChasse.Chasseurs.Add(new Chasseur {Nom = chasseur.nom, BallesRestantes = chasseur.nbBalles,});
        }

        if ( partieDeChasse.Chasseurs.Count == 0 )
        {
            throw new ImpossibleDeDémarrerUnePartieSansChasseur();
        }

        var chasseursToString = string.Join(
            ", ",
            partieDeChasse.Chasseurs.Select(c => c.Nom + $" ({c.BallesRestantes} balles)")
        );

        partieDeChasse.Events.Add(new Event(_timeProvider(),
            $"La partie de chasse commence à {partieDeChasse.Terrain.Nom} avec {chasseursToString}")
        );

        _repository.Save(partieDeChasse);

        return partieDeChasse.Id;
    }

    public void PrendreLapéro(Guid id)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id);

        if ( partieDeChasse == null )
        {
            throw new LaPartieDeChasseNexistePas();
        }

        if ( partieDeChasse.Status == PartieStatus.Apéro )
        {
            throw new OnEstDéjàEnTrainDePrendreLapéro();
        }

        if ( partieDeChasse.Status == PartieStatus.Terminée )
        {
            throw new OnPrendPasLapéroQuandLaPartieEstTerminée();
        }

        partieDeChasse.Status = PartieStatus.Apéro;
        partieDeChasse.Events.Add(new Event(_timeProvider(), "Petit apéro"));
        _repository.Save(partieDeChasse);
    }

    public void ReprendreLaPartie(Guid id)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id);

        if ( partieDeChasse == null )
        {
            throw new LaPartieDeChasseNexistePas();
        }

        if ( partieDeChasse.Status == PartieStatus.EnCours )
        {
            throw new LaChasseEstDéjàEnCours();
        }

        if ( partieDeChasse.Status == PartieStatus.Terminée )
        {
            throw new QuandCestFiniCestFini();
        }

        partieDeChasse.Status = PartieStatus.EnCours;
        partieDeChasse.Events.Add(new Event(_timeProvider(), "Reprise de la chasse"));
        _repository.Save(partieDeChasse);
    }

    public string TerminerLaPartie(Guid id)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id);

        IOrderedEnumerable<IGrouping<int, Chasseur>> classement = partieDeChasse
            .Chasseurs
            .GroupBy(c => c.NbGalinettes)
            .OrderByDescending(g => g.Key);

        if ( partieDeChasse.Status == PartieStatus.Terminée )
        {
            throw new QuandCestFiniCestFini();
        }

        partieDeChasse.Status = PartieStatus.Terminée;

        string result;

        if ( classement.All(group => group.Key == 0) )
        {
            result = "Brocouille";

            partieDeChasse.Events.Add(
                new Event(_timeProvider(), "La partie de chasse est terminée, vainqueur : Brocouille")
            );
        }
        else
        {
            IGrouping<int, Chasseur> firstChasseur = classement.ElementAt(0);
            result = string.Join(", ", firstChasseur.Select(c => c.Nom));

            partieDeChasse.Events.Add(
                new Event(_timeProvider(),
                    $"La partie de chasse est terminée, vainqueur : {
                        string.Join(", ", firstChasseur.Select(c => $"{c.Nom} - {c.NbGalinettes} galinettes"))}"
                )
            );
        }

        _repository.Save(partieDeChasse);

        return result;
    }

    public void TirerSurUneGalinette(Guid id, string chasseur)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id);

        if ( partieDeChasse == null )
        {
            throw new LaPartieDeChasseNexistePas();
        }

        if ( partieDeChasse.Terrain.NbGalinettes != 0 )
        {
            if ( partieDeChasse.Status != PartieStatus.Apéro )
            {
                if ( partieDeChasse.Status != PartieStatus.Terminée )
                {
                    if ( partieDeChasse.Chasseurs.Exists(c => c.Nom == chasseur) )
                    {
                        Chasseur chasseurQuiTire = partieDeChasse.Chasseurs.Find(c => c.Nom == chasseur)!;

                        if ( chasseurQuiTire.BallesRestantes == 0 )
                        {
                            partieDeChasse.Events.Add(new Event(_timeProvider(),
                                $"{chasseur} veut tirer sur une galinette -> T'as plus de balles mon vieux, chasse à la main"));

                            _repository.Save(partieDeChasse);

                            throw new TasPlusDeBallesMonVieuxChasseALaMain();
                        }

                        chasseurQuiTire.BallesRestantes--;
                        chasseurQuiTire.NbGalinettes++;
                        partieDeChasse.Terrain.NbGalinettes--;
                        partieDeChasse.Events.Add(new Event(_timeProvider(), $"{chasseur} tire sur une galinette"));
                    }
                    else
                    {
                        throw new ChasseurInconnu(chasseur);
                    }
                }
                else
                {
                    partieDeChasse.Events.Add(new Event(_timeProvider(),
                        $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));

                    _repository.Save(partieDeChasse);

                    throw new OnTirePasQuandLaPartieEstTerminée();
                }
            }
            else
            {
                partieDeChasse.Events.Add(new Event(_timeProvider(),
                    $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));

                _repository.Save(partieDeChasse);

                throw new OnTirePasPendantLapéroCestSacré();
            }
        }
        else
        {
            throw new TasTropPicoléMonVieuxTasRienTouché();
        }

        _repository.Save(partieDeChasse);
    }
}