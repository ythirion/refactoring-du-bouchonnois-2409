using Bouchonnois.Domain;
using Bouchonnois.Domain.Exceptions;

using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace Bouchonnois.Service;

public interface IUseCase<in T> where T : TCommand
{
    void Handle(T tirerCommand);
}

public abstract record TCommand;

public class TirerUseCase(IPartieDeChasseRepository repository, Func<DateTime> timeProvider) : IUseCase<TirerCommand>
{
    public void Handle(TirerCommand tirerCommand)
    {
        PartieDeChasse partieDeChasse = repository.GetById(tirerCommand.Id) ?? throw new LaPartieDeChasseNexistePas();

        partieDeChasse.Tirer(tirerCommand.Chasseur, timeProvider, () => repository.Save(partieDeChasse));

        repository.Save(partieDeChasse);
    }

    public Either<Error, Unit> HandleSansException(TirerCommand tirerCommand)
    {
        return Try(() =>
            {
                Handle(tirerCommand);
                return Unit.Default;
            })
            .ToEither()
            .MapLeft(ex => Error.New($"La partie de chasse {tirerCommand.Id} n'existe pas"));
    }
}