using Bouchonnois.Domain;

namespace Bouchonnois.Service;

public interface IPartieDeChasseRepository
{
    PartieDeChasse GetById(Guid partieDeChasseId);

    void Save(PartieDeChasse partieDeChasse);
}