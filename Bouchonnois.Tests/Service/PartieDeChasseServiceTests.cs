using Bouchonnois.Domain;

namespace Bouchonnois.Tests.Service;

public partial class PartieDeChasseServiceTests
{
    protected static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
    protected static readonly Func<DateTime> TimeProvider = () => Now;

    protected void AssertLastEvent(PartieDeChasse partieDeChasse,
        string expectedMessage)
    {
        partieDeChasse.Events.Should()
            .HaveCount(1)
            .And
            .EndWith(new Event(Now, expectedMessage));
    }
}