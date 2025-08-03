namespace WadMaker.Models;

public record Theme(ThemeRule[] Rules)
{
    public Theme(IEnumerable<ThemeRule> Rules) : this(Rules.ToArray()) { }
}

public record ThemeRule(TextureInfo Texture, params ThemeCondition[] conditions)
{
    public bool AppliesTo(LineDef lineDef) => conditions.All(c => c.AppliesTo(lineDef));

    public ThemeRule(string[] themeNames, string colorName, params ThemeCondition[] conditions)
        : this(new TextureInfo(GetTexturesMatchingThemes.Execute(themeNames, colorName).First()), conditions)
    { }
}

public abstract record ThemeCondition()
{
    public abstract bool AppliesTo(LineDef lineDef);

    public ThemeCondition AndNot(ThemeCondition other)
    {
        return new And(this, new Not(other));
    }
}

public record TrueCondition() : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef) => true;
}

public record And(params ThemeCondition[] Conditions) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return Conditions.All(c=> c.AppliesTo(lineDef));
    }
}

public record Not(ThemeCondition Condition) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return !Condition.AppliesTo(lineDef);
    }
}

public record IsDoor() : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return lineDef.LineSpecial?.Type == LineSpecialType.DoorRaise;
    }
}

public record IsDoorSide() : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        if (lineDef.Back != null)
            return false;

        return lineDef.Front.Sector.Lines.Any(p=>p.LineSpecial?.Type == LineSpecialType.DoorRaise);
    }
}

public record HasLineSpecial() : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return lineDef.LineSpecial != null;
    }
}

public record FloorDifferenceLessOrEqualTo(int Amount) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        if (lineDef.Back == null)
            return false;

        int floorFront = lineDef.Front.Sector.Data.heightfloor;
        int floorBack = lineDef.Back.Sector.Data.heightfloor;

        return Math.Abs(floorFront - floorBack) <= Amount;
    }
}

public record SectorHeightGreaterOrEqualTo(int Amount) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        if (lineDef.Front.Sector.Height >= Amount)
            return true;

        if (lineDef.Back != null && lineDef.Back.Sector.Height >= Amount)
            return true;

        return false;
    }
}
