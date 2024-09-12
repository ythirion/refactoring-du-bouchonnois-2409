namespace Bouchonnois.Domain
{
    public class Chasseur
    {
        public required string Nom { get; set; }
        public int BallesRestantes { get; set; }
        public int NbGalinettes { get; set; }
    }
}