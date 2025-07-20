namespace WadMaker.Models.LineSpecials;

public record DoorRaise(int tag, Speed speed, int delay=150, int lighttag=0) 
    : LineSpecial(LineSpecialType.DoorRaise, tag, (int)speed, delay, lighttag, null)
{
}
