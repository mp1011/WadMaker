namespace WadMaker.Models.Theming;

public abstract record ThemeCondition()
{
    public abstract bool AppliesTo(LineDef lineDef);

    public abstract bool AppliesTo(Sector sector);

    public ThemeCondition AndNot(ThemeCondition other)
    {
        return new And(this, new Not(other));
    }

    public ThemeCondition And(ThemeCondition other)
    {
        return new And(this, other);
    }
}

public abstract record LineOnlyThemeCondition() : ThemeCondition
{
    public override bool AppliesTo(Sector sectord) => false;
}

public record LambdaThemeCondition(Func<LineDef, bool> Condition) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef) => Condition(lineDef);
    public override bool AppliesTo(Sector sector) => false; // maybe?
}

public record TrueCondition() : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef) => true;
    public override bool AppliesTo(Sector sector) => true;
}

public record And(params ThemeCondition[] Conditions) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return Conditions.All(c => c.AppliesTo(lineDef));
    }

    public override bool AppliesTo(Sector sector)
    {
        return Conditions.All(c => c.AppliesTo(sector));
    }
}

public record Not(ThemeCondition Condition) : ThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return !Condition.AppliesTo(lineDef);
    }

    public override bool AppliesTo(Sector sector)
    {
        return !Condition.AppliesTo(sector);
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

    public override bool AppliesTo(Sector sector)
    {
        if(sector.Tag.HasValue)
            sector.Activators.Any(p=>p.LineSpecial?.IsDoor == true);

        return sector.Lines.Any(p=> p.Back != null && p.LineSpecial?.IsDoor == true && p.LineSpecial?.AppliesToBackSector == true);
    }
}

public record IsDoorSide() : LineOnlyThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        if (lineDef.Back != null)
            return false;

        return lineDef.Front.Sector.Lines.Any(p => p.LineSpecial?.IsDoor == true);
    }
}

public record HasLineSpecial() : LineOnlyThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        return lineDef.LineSpecial != null;
    }
}

public record FloorDifferenceLessOrEqualTo(int Amount) : LineOnlyThemeCondition
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
    public override bool AppliesTo(LineDef lineDef) =>
        lineDef.Sectors.Any(AppliesTo);
    
    public override bool AppliesTo(Sector sector) => 
        sector.Height >= Amount;
}

public record LineLengthGreaterOrEqualTo(int Amount) : LambdaThemeCondition(x => x.Length >= Amount)
{ }

public record LineLengthIs(int Amount) : LambdaThemeCondition(x => x.Length == Amount)
{ }

public record LowerFloorTextureIs(FlatsQuery FloorQuery) : LineOnlyThemeCondition
{
    public override bool AppliesTo(LineDef lineDef)
    {
        var lowerSector = lineDef.Sectors.OrderBy(p => p.Room.Floor).First();
        return FloorQuery.Execute().Contains(lowerSector.Floor);
    }
}

public record FrontRoomBuildingBlockTypeIs<T> : ThemeCondition
    where T : class
{
    public override bool AppliesTo(LineDef lineDef) => AppliesTo(lineDef.Front.Sector);

    public override bool AppliesTo(Sector sector)
    {
        return sector.Room.BuildingBlock != null && sector.Room.BuildingBlock is T;
    }
}