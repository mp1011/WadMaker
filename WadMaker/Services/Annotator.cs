namespace WadMaker.Services;

public interface IAnnotator
{
    string? GetComment(linedef linedef, int? index=null);
}

public static class IAnnotatorExtensions
{
    public static linedef AddComment(this linedef linedef, int? index, IAnnotator annotator) => 
        linedef with {  comment = annotator.GetComment(linedef, index) };

    public static IEnumerable<linedef> AnnotateAll(this IEnumerable<linedef> linedefs, IAnnotator annotator)
    {
        int index = 0;
        foreach (var linedef in linedefs)
        {
            yield return linedef.AddComment(index++, annotator);
        }
    }
}

public class EmpyAnnotator : IAnnotator
{
    public string? GetComment(linedef linedef, int? index = null) => null;
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
}