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
    }

    private bool IsDuringApéro()
    {
        return Status == PartieStatus.Apéro;
    }
}