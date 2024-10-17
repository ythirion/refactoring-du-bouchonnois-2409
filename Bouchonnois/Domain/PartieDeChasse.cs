namespace Bouchonnois.Domain;

public class PartieDeChasse
{
    public List<Chasseur> Chasseurs { get; set; } = [];
    public List<Event> Events { get; set; } = [];
    public Guid Id { get; set; }
    public PartieStatus Status { get; set; }
    public required Terrain Terrain { get; set; }
}