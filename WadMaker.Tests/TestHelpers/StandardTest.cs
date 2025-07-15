using WadMaker.Tests.Services;

namespace WadMaker.Tests.TestHelpers;

internal class StandardTest
{
    protected ServiceProvider ServiceProvider { get; }

    protected MapPainter MapPainter => ServiceProvider.GetRequiredService<MapPainter>();

    protected MapBuilder MapBuilder => ServiceProvider.GetRequiredService<MapBuilder>();


    public StandardTest()
    {
        ServiceProvider = TestServiceContainer.CreateWithTestAnnotator();
    }
}
