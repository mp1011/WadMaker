namespace WadMaker.Models.LineSpecials;

public record DoorRaise(int Tag, Speed Speed, Delay Delay=Delay.StandardDoor, int LightTag=0) 
    : LineSpecial(LineSpecialType.DoorRaise, Tag, (int)Speed, (int)Delay, LightTag, null)
{
    public override bool AppliesToBackSector => Tag == 0;
    public override int? SectorTag => Tag;
    public override bool IsDoor => true;
}
