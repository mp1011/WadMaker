namespace WadMaker.Models;

public enum Side 
{
    None,
    Left,
    Right,
    Top,
    Bottom
}

public enum Speed
{ 
    StandardDoor = 16,
    StandardLift = 32, // not positive about this
    BlazingDoor = 64,
}

public enum Delay
{
    StandardLift = 105,
    StandardDoor = 150,
}

public enum AmmoBalance
{
    UnableToCalculate,
    Insufficient,
    BarelyEnough,
    Adequate,
    Comfortable,
    Generous
}