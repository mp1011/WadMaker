namespace WadMaker.Models.LineSpecials;

public record Plat_DownWaitUpStay(int Tag, Speed Speed, Delay Delay = Delay.StandardLift)
    : LineSpecial(LineSpecialType.Plat_DownWaitUpStay, Tag, (int)Speed, (int)Delay, null, null)
{
    public override bool AppliesToBackSector => Tag == 0;
    public override int? SectorTag => Tag;
}
