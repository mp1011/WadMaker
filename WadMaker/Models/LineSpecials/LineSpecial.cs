namespace WadMaker.Models.LineSpecials;

public enum LineSpecialType
{
    None=0,
    DoorOpen=11,
    DoorRaise=12,
    Plat_DownWaitUpStay=62,
    Exit_Normal=243
}

public record class LineSpecial(LineSpecialType Type, int? arg0, int? arg1, int? arg2, int? arg3, int? arg4)
{
    public linedef ApplyTo(linedef linedef) => linedef with
    {
        special = (int)Type,
        arg0 = arg0,
        arg1 = arg1,
        arg2 = arg2,
        arg3 = arg3,
        arg4 = arg4,
        playeruse = true,
        repeatspecial = true,
    };

    public virtual bool AppliesToBackSector => false;
}
