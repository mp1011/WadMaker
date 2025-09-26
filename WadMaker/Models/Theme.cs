namespace WadMaker.Models;

public record Theme(ThemeRule[] Rules)
{
    public Theme(IEnumerable<ThemeRule> Rules) : this(Rules.ToArray()) { }
}

public record ThemeRule(TextureQuery? Query = null, TextureInfo? Texture = null, params ThemeCondition[] Conditions)
{
    public bool AppliesTo(LineDef lineDef) => Conditions.All(c => c.AppliesTo(lineDef));

    public TextureInfo GetTexture(LineDef lineDef)
    {
        if (Query == null)
            return Texture ?? new TextureInfo();
        
        var upper = Query.Execute(lineDef, TexturePart.Upper).FirstOrDefault();
        var middle = Query.Execute(lineDef, TexturePart.Middle).FirstOrDefault();
        var lower = Query.Execute(lineDef, TexturePart.Lower).FirstOrDefault();

        if (Texture == null)
            return new TextureInfo(Mid: middle, Upper: upper, Lower: lower);

        return Texture with
        {
            Upper = Texture.Upper ?? new TextureQuery(upper),
            Mid = Texture.Mid ?? new TextureQuery(middle),
            Lower = Texture.Lower ?? new TextureQuery(lower)
        };
    }
}

public abstract record ThemeCondition()
{
    public abstract bool AppliesTo(LineDef lineDef);

    public ThemeCondition AndNot(ThemeCondition other)
    {
        return new And(this, new Not(other));
    }

    public ThemeCondition And(ThemeCondition other)
    {
        return new And(this, other);
    }
}

public record LambdaThemeCondition(Func<LineDef, bool> Condition) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef) => Condition(lineDef);
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
        if (lineDef.LineSpecial?.IsDoor == true && lineDef.LineSpecial?.AppliesToBackSector == true)
            return true;

        var activators = lineDef.Sectors.SelectMany(p => p.Activators)
            .Where(p => p.LineSpecial?.IsDoor == true)
            .ToArray();

        return activators.Any();
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

        if (floorFront == floorBack)
            return false;

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

public record LineLengthGreaterOrEqualTo(int Amount) : LambdaThemeCondition(x=> x.Length >= Amount)
{}

public record LineLengthIs(int Amount) : LambdaThemeCondition(x => x.Length == Amount)
{}

public record LowerFloorTextureIs(FlatsQuery FloorQuery) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        var lowerSector = lineDef.Sectors.OrderBy(p => p.Room.Floor).First();
        return FloorQuery.Execute().Contains(lowerSector.Floor);
    }
}