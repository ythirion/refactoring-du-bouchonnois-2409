using Bouchonnois.Domain;

namespace Bouchonnois.Repository;

public interface IPartieDeChasseRepository
{
    PartieDeChasse GetById(Guid partieDeChasseId);

    void Save(PartieDeChasse partieDeChasse);
}