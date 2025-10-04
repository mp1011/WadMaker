namespace WadMaker.Models.LineSpecials;

public record Plat_DownWaitUpStay(int Tag, Speed Speed, Delay Delay = Delay.StandardLift, Activation Activation = Activation.Default)
    : LineSpecial(LineSpecialType.Plat_DownWaitUpStay, Tag, (int)Speed, (int)Delay, null, null, Activation)
{
    public override bool AppliesToBackSector => Tag == 0;
    public override int? SectorTag => Tag;
}
