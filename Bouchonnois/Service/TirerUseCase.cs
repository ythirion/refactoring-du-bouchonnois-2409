using Bouchonnois.Domain;
using Bouchonnois.Service.Exceptions;

namespace Bouchonnois.Service;

public class TirerUseCase(IPartieDeChasseRepository repository, Func<DateTime> timeProvider)
{
    private readonly IPartieDeChasseRepository _repository = repository;
    private readonly Func<DateTime> _timeProvider = timeProvider;

    public void Tirer(Guid id, string chasseur)
    {
        PartieDeChasse partieDeChasse = _repository.GetById(id) ?? throw new LaPartieDeChasseNexistePas();

        if ( partieDeChasse.Status == PartieStatus.Apéro )
        {
            partieDeChasse.Events.Add(new Event(_timeProvider(),
                $"{chasseur} veut tirer -> On tire pas pendant l'apéro, c'est sacré !!!"));

            _repository.Save(partieDeChasse);

            throw new OnTirePasPendantLapéroCestSacré();
        }

        if ( partieDeChasse.Status == PartieStatus.Terminée )
        {
            partieDeChasse.Events.Add(new Event(_timeProvider(),
                $"{chasseur} veut tirer -> On tire pas quand la partie est terminée"));

            _repository.Save(partieDeChasse);

            throw new OnTirePasQuandLaPartieEstTerminée();
        }

        if ( !partieDeChasse.Chasseurs.Exists(c => c.Nom == chasseur) )
        {
            throw new ChasseurInconnu(chasseur);
        }
        
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