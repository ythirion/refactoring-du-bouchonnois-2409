using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;

namespace Bouchonnois.Service;

public class TirerUseCase(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
{
    public void Tirer(Guid id, string chasseur)
    {
        PartieDeChasse partieDeChasse = repository.GetById(id) ?? throw new LaPartieDeChasseNexistePas();

        partieDeChasse.Tirer(chasseur, timeProvider, () => repository.Save(partieDeChasse));

        repository.Save(partieDeChasse);
    }
}