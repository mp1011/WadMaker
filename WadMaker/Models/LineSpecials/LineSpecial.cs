namespace WadMaker.Models.LineSpecials;

public enum LineSpecialType
{
    None=0,
    DoorOpen=11,
    DoorRaise=12,
    Door_LockedRaise = 13,
    Plat_DownWaitUpStay=62,
    Exit_Normal=243
}

public enum Activation
{
    PlayerUse = 1,
    MonsterUse = 2,
    PlayerCross = 4,
    MonsterCross = 8,
    Shoot = 16,
    Repeating = 32,
    Default = PlayerUse | Activation.Repeating,
}

public record class LineSpecial(LineSpecialType Type, int? arg0, int? arg1, int? arg2, int? arg3, int? arg4, Activation Activation = Activation.Default)
{
    public linedef ApplyTo(linedef linedef) => linedef with
    {
        special = (int)Type,
        arg0 = arg0,
        arg1 = arg1,
        arg2 = arg2,
        arg3 = arg3,
        arg4 = arg4,
        playeruse = Activation.HasFlag(Activation.PlayerUse) ? true : null,
        playercross = Activation.HasFlag(Activation.PlayerCross) ? true : null,
        monsteruse = Activation.HasFlag(Activation.MonsterUse) ? true : null,
        monstercross = Activation.HasFlag(Activation.MonsterCross) ? true : null,
        impact = Activation.HasFlag(Activation.Shoot) ? true : null,
        repeatspecial = Activation.HasFlag(Activation.Repeating) ? true : null,
    };

    public virtual int? SectorTag => null;
    public virtual bool AppliesToBackSector => false;
    public virtual bool IsDoor => false;
}
