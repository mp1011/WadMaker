namespace WadMaker.Services;

public interface IAnnotator
{
    string? GetComment(linedef linedef, int? index=null);
    string? GetComment(sidedef linedef, int? index = null);
}

public static class IAnnotatorExtensions
{
    public static linedef AddComment(this linedef linedef, int? index, IAnnotator annotator) => 
        linedef with {  comment = annotator.GetComment(linedef, index) };

    public static sidedef AddComment(this sidedef sidedef, int? index, IAnnotator annotator) =>
        sidedef with { comment = annotator.GetComment(sidedef, index) };

    public static IEnumerable<linedef> AnnotateAll(this IEnumerable<linedef> linedefs, IAnnotator annotator)
    {
        int index = 0;
        foreach (var linedef in linedefs)
        {
            yield return linedef.AddComment(index++, annotator);
        }
    }

    public static IEnumerable<sidedef> AnnotateAll(this IEnumerable<sidedef> sidedefs, IAnnotator annotator)
    {
        int index = 0;
        foreach (var sidedef in sidedefs)
        {
            yield return sidedef.AddComment(index++, annotator);
        }
    }
}

public class EmpyAnnotator : IAnnotator
{
    public string? GetComment(linedef linedef, int? index = null) => null;
    public string? GetComment(sidedef sidedef, int? index = null) => null;

}

public class VerboseAnnotator : IAnnotator
{
    public string? GetComment(linedef linedef, int? index = null)
    {
        if (index.HasValue)
            return $"Line #{index} : {linedef}";
        else 
            return linedef.ToString();
    }

    public string? GetComment(sidedef linedef, int? index = null)
    {
        if (index.HasValue)
            return $"Side #{index} : {linedef}";
        else
            return linedef.ToString();
    }
}