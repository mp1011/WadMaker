public record MonsterPlacement(ThingType Monster, double BeginAt, double EndAt, EnemyDensity Density, Angle Angle,
    ThingPattern Pattern = ThingPattern.Row, ThingFlags Flags = ThingFlags.AllSkillsAndModes, bool Absolute = false)
{

    public MonsterPlacement(ThingType Monster, int BeginAt, int EndAt, EnemyDensity Density, Angle Angle,
        ThingPattern Pattern = ThingPattern.Row, ThingFlags Flags = ThingFlags.AllSkillsAndModes) : this(Monster, (double)BeginAt, (double)EndAt, Density, Angle, Pattern, Flags, Absolute: true)
    { }

    public IEnumerable<PlayerPathNode> NodeRange(PlayerPath path)
    {
        if (Absolute)
        {
            return path.Nodes.Where((p, index) => index >= BeginAt &&
                                          index <= EndAt);
        }
        else
        {
            return path.Nodes.Where((p, index) => index >= ((path.Nodes.Length - 1) * BeginAt) &&
                                                      index <= (path.Nodes.Length - 1) * EndAt);
        }
    }
}






