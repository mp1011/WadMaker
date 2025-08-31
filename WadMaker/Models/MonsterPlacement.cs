public record MonsterPlacement(ThingType Monster, double BeginAt, double EndAt, EnemyDensity Density)
{
    public IEnumerable<PlayerPathNode> NodeRange(PlayerPath path)
    {
        return path.Nodes.SkipWhile((p, index) => index < (path.Nodes.Length - 1) * BeginAt)
                         .TakeWhile((p, index) => index <= (path.Nodes.Length - 1) * EndAt);
    }
}



