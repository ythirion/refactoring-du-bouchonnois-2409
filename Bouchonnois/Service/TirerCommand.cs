namespace Bouchonnois.Service;

public record TirerCommand(Guid Id, string Chasseur) : TCommand;