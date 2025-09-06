public record MonsterPlacement(ThingType Monster, double BeginAt, double EndAt, EnemyDensity Density, Angle Angle,
    ThingPattern Pattern = ThingPattern.Row)
{
    public IEnumerable<PlayerPathNode> NodeRange(PlayerPath path)
    {
        return path.Nodes.Where((p, index) => index >= ((path.Nodes.Length - 1) * BeginAt) &&
                                                  index <= (path.Nodes.Length - 1) * EndAt);
    }
}



