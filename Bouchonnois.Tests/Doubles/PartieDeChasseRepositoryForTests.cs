using Bouchonnois.Domain;
using Bouchonnois.Service;

namespace Bouchonnois.Tests.Doubles;

public class PartieDeChasseRepositoryForTests : IPartieDeChasseRepository
{
    private readonly Dictionary<Guid, PartieDeChasse> _partiesDeChasse = new Dictionary<Guid, PartieDeChasse>();
    private PartieDeChasse? _savedPartieDeChasse;

    public void Save(PartieDeChasse partieDeChasse)
    {
        _savedPartieDeChasse = partieDeChasse;
        _partiesDeChasse[partieDeChasse.Id] = partieDeChasse;
    }

    public PartieDeChasse GetById(Guid partieDeChasseId)
    {
        return (_partiesDeChasse.TryGetValue(partieDeChasseId, out PartieDeChasse? value)
            ? value
            : null)!;
    }

    public void Add(PartieDeChasse partieDeChasse)
    {
        _partiesDeChasse[partieDeChasse.Id] = partieDeChasse;
    }

    public PartieDeChasse SavedPartieDeChasse()
    {
        return _savedPartieDeChasse!;
    }

    public bool HasNotSavedPartieDeChasse()
    {
        return _savedPartieDeChasse == null;
    }
}