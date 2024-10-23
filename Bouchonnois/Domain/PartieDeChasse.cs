using Bouchonnois.Domain.Exceptions;

namespace Bouchonnois.Domain;

public class PartieDeChasse
{
    public List<Chasseur> Chasseurs { get; set; } = [];
    public List<Event> Events { get; set; } = [];
    public Guid Id { get; set; }
    public PartieStatus Status { get; set; }
    public required Terrain Terrain { get; set; }

    public void Tirer(string chasseur, Func<DateTime> timeProvider, Action save)
    {
        if ( IsDuringApéro() )
        {
            Events.Add(new Event(timeProvider(),
                                        $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));

            save();

            throw new OnTirePasPendantLapéroCestSacré();
        }

        if ( IsPartieDeChasseTerminée() )
        {
            Events.Add(new Event(timeProvider(),
                $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));

            save();

            throw new OnTirePasQuandLaPartieEstTerminée();
        }

        if ( !Chasseurs.Exists(c => c.Nom == chasseur) )
        {
            throw new ChasseurInconnu(chasseur);
        }
    }

    private bool IsDuringApéro()
    {
        return Status == PartieStatus.Apéro;
    }

    private bool IsPartieDeChasseTerminée()
    {
        return Status == PartieStatus.Terminée;
    }
}