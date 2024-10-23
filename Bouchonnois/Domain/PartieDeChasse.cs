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
            EmetEvenementEtSauver(timeProvider, save, $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!");

            throw new OnTirePasPendantLapéroCestSacré();
        }

        if ( IsPartieDeChasseTerminée() )
        {
            EmetEvenementEtSauver(timeProvider, save,
                $"{chasseur} veut tirer -> On tire pas quand la partie est terminée");
            
            throw new OnTirePasQuandLaPartieEstTerminée();
        }

        if ( !Chasseurs.Exists(c => c.Nom == chasseur) )
        {
            throw new ChasseurInconnu(chasseur);
        }
        
        Chasseur chasseurQuiTire = Chasseurs.Find(c => c.Nom == chasseur)!;

        if (chasseurQuiTire.YaPlusDeBalles())
        {
            EmetEvenementEtSauver(timeProvider, save,
                $"{chasseur} tire -> T'as plus de balles mon vieux, chasse à la main");

            throw new TasPlusDeBallesMonVieuxChasseALaMain();
        }

        Events.Add(new Event(timeProvider(), $"{chasseur} tire"));
        
        chasseurQuiTire.ATire();
    }

    private void EmetEvenementEtSauver(Func<DateTime> timeProvider, Action save, string message)
    {
        Events.Add(new Event(timeProvider(), message));

        save();
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