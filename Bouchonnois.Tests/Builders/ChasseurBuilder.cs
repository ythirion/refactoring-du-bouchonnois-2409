using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Builders;

public class ChasseurBuilder
{
    private int _ballesRestantes;
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
        return new ChasseurBuilder(Chasseurs.Bernard, 8);
    }

    public Chasseur Build()
    {
        return new Chasseur {BallesRestantes = _ballesRestantes, NbGalinettes = _nbGalinettes, Nom = _nom,};
    }

    // Object mothers
    public static ChasseurBuilder Dédé()
    {
        return new ChasseurBuilder(Chasseurs.Dédé, 20);
    }

    public static ChasseurBuilder Robert()
    {
        return new ChasseurBuilder(Chasseurs.Robert, 12);
    }

    public static ChasseurBuilder UnChasseur(string nom) => new(nom);

    public ChasseurBuilder AvecDesBalles(int ballesRestantes)
    {
        _ballesRestantes = ballesRestantes;
        return this;
    }

    public ChasseurBuilder SansBalles()
    {
        _ballesRestantes = 0;
        return this;
    }
}