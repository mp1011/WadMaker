
namespace WadMaker.Tests.TestHelpers;

class TestAnnotator : IAnnotator
{
    public string? GetComment(linedef linedef, int? index)
    {
        if (linedef.twoSided is not null)
            return "double sided line";
        else
            return null;
    }

    public string? GetComment(sidedef linedef, int? index = null) => null;
}
