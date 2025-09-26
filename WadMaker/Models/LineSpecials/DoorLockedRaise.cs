namespace WadMaker.Models.LineSpecials;

public record DoorLockedRaise(int Tag, Speed Speed, Delay Delay = Delay.StandardDoor, KeyType Lock = KeyType.Red, int LightTag = 0)
    : LineSpecial(LineSpecialType.Door_LockedRaise, Tag, (int)Speed, (int)Delay, (int)Lock, LightTag)
{
    public override bool AppliesToBackSector => Tag == 0;
    public override int? SectorTag => Tag;
    public override bool IsDoor => true;
}
