using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;

namespace Bouchonnois.Service;

public class TirerUseCase(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
{
    private readonly IPartieDeChasseRepository _repository = repository;
    private readonly Func<DateTime> _timeProvider = timeProvider;

    public void Tirer(Guid id, string chasseur)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id) ?? throw new LaPartieDeChasseNexistePas();

        if ( !partieDeChasse.Chasseurs.Exists(c => c.Nom == chasseur) )
        {
            throw new ChasseurInconnu(chasseur);
        }

        partieDeChasse.Tirer(chasseur, _timeProvider, () => _repository.Save(partieDeChasse));

        _repository.Save(partieDeChasse);
    }
}