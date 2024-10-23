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

        Chasseur chasseurQuiTire = partieDeChasse.Chasseurs.Find(c => c.Nom == chasseur)!;

        if ( chasseurQuiTire.BallesRestantes == 0 )
        {
            partieDeChasse.Events.Add(new Event(_timeProvider(),
                $"{chasseur} tire -> T'as plus de balles mon vieux, chasse à la main"));

            _repository.Save(partieDeChasse);

            throw new TasPlusDeBallesMonVieuxChasseALaMain();
        }

        partieDeChasse.Events.Add(new Event(_timeProvider(), $"{chasseur} tire"));
        chasseurQuiTire.BallesRestantes--;

        _repository.Save(partieDeChasse);
    }
}