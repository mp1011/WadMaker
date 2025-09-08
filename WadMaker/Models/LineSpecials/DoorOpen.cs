namespace WadMaker.Models.LineSpecials;

public record DoorOpen(int Tag, Speed Speed, int LightTag=0) 
    : LineSpecial(LineSpecialType.DoorOpen, Tag, (int)Speed, LightTag, null, null)
{
}
