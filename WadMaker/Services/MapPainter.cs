namespace WadMaker.Services;
public class MapPainter
{
    private readonly IAnnotator _annotator;

    public MapPainter(IAnnotator annotator)
    {
        _annotator = annotator;
    }

    public string Paint(MapElements mapElements)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace = \"ZDoom\";");
       
        mapElements.Vertices.Paint(sb);
        mapElements.Sectors.ToDataArray().Paint(sb);
        mapElements.SideDefs.ToDataArray().Paint(sb);
        mapElements.LineDefs.ToDataArray().AnnotateAll(_annotator).Paint(sb);
        mapElements.Things.ToDataArray().Paint(sb);
       
        return sb.ToString();
    }
}

