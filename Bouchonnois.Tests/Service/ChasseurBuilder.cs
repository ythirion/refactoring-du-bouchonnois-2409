using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Service;

public class ChasseurBuilder
{
    private readonly int _ballesRestantes;
    private readonly string? _nom;
    private int _nbGalinettes;

    public ChasseurBuilder(string nom) => _nom = nom;

    private ChasseurBuilder(string nom, int ballesRestantes)
    {
        _nom = nom;
        _ballesRestantes = ballesRestantes;
    }

    public static ChasseurBuilder Bernard()
    {
        return new ChasseurBuilder("Bernard", 8);
    }

    public Chasseur Build()
    {
        return new Chasseur { BallesRestantes = _ballesRestantes, NbGalinettes = _nbGalinettes, Nom = _nom, };
    }

    // Object mothers
    public static ChasseurBuilder Dédé()
    {
        return new ChasseurBuilder("Dédé", 20);
    }

    public static ChasseurBuilder Robert()
    {
        return new ChasseurBuilder("Robert", 12);
    }
}