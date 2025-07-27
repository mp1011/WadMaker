namespace WadMaker.Models.LineSpecials;

public record DoorRaise(int Tag, Speed Speed, Delay Delay=Delay.StandardDoor, int LightTag=0) 
    : LineSpecial(LineSpecialType.DoorRaise, Tag, (int)Speed, (int)Delay, LightTag, null)
{
}
