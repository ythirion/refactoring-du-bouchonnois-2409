namespace Bouchonnois.Domain;

public class Chasseur
{
    public int BallesRestantes { get; set; }
    public int NbGalinettes { get; set; }
    public required string Nom { get; set; }

    public bool YaPlusDeBalles()
    {
        return BallesRestantes == 0;
    }

    public void ATire()
    {
        BallesRestantes--;
    }
}