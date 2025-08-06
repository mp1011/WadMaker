namespace WadMaker.Models;

public enum ThingType
{
  Unknown = 0,
  Player1Start = 1,
  Shotgun = 2001,
  Shells = 2008,
  Imp = 3001,
}

public record DoomThingInfo(int Decimal, string Hex, string Version, int Radius, int Height,
  string Sprite, string Sequence, string Class, string Description);