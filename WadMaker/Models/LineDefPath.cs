using System.Collections;
using System.Text;

namespace WadMaker.Models;

/// <summary>
/// A connected sequence of lines
/// </summary>
public class LineDefPath : IEnumerable<LineDef>
{
    private List<LineDef> _lines;
    private MapElements _mapElements;

    public LineDef Head => _lines.Last();
    public LineDef First => _lines.First();

    public LineDefPath(MapElements mapElements, LineDef initialLine)
    {
        _lines = new List<LineDef>();
        _lines.Add(initialLine);
        _mapElements = mapElements;
    }

    public LineDefPath(MapElements mapElements, IEnumerable<LineDef> initialLines)
    {
        _lines = new List<LineDef>();
        _lines.AddRange(initialLines);
        _mapElements = mapElements;
    }

    public IEnumerable<LineDefPath> Build()
    {
        while (true)
        {
            var nextLines = _mapElements.LineDefs.Where(p => p.V1 == Head.V2
                                                        && !_lines.Contains(p))
                                                 .ToArray();

            if (!nextLines.Any())
                return new[] { this };
            else if (nextLines.Length == 1)            
                _lines.Add(nextLines[0]);
            else
            {
                return nextLines.SelectMany(p =>
                {
                    var copyPath = _lines.ToList();
                    copyPath.Add(p);
                    return new LineDefPath(_mapElements, copyPath).Build();
                });
            }            
        }
    }

    public IEnumerable<LineDefPath> SplitBy(Func<LineDef,LineDef,bool> condition)
    {
        var prev = _lines.First();
        List<LineDefPath> runs = new List<LineDefPath>();
        List<LineDef> currentRun = new List<LineDef>();
        currentRun.Add(prev);

        foreach(var line in _lines.Skip(1))
        {
            if(condition(prev,line))
            {
                currentRun.Add(line);
            }
            else
            {
                runs.Add(new LineDefPath(_mapElements, currentRun));
                currentRun.Clear();
                currentRun.Add(line);
            }

            prev = line;
        }

        if (runs.Any() && condition(_lines.First(), currentRun.Last()))
        {
            currentRun.AddRange(runs.First());
            runs[0] = new LineDefPath(_mapElements, currentRun);
        }
        else
        {
            runs.Add(new LineDefPath(_mapElements, currentRun));
        }

        return runs;
    }

    public IEnumerator<LineDef> GetEnumerator()
    {
        return ((IEnumerable<LineDef>)_lines).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_lines).GetEnumerator();
    }
}
