namespace WadMaker.Models;

[Flags]
public enum Side 
{
    None,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8
}

public enum Speed
{ 
    StandardDoor = 16,
    StandardLift = 32, // not positive about this
    BlazingDoor = 64,
}

public enum KeyType
{
    None = 0,
    Red = 1,
    Blue = 2,
    Yellow = 3,
    RedSkull = 4,
    BlueSkull = 5,
    YellowSkull = 6
}

public enum Delay
{
    StandardLift = 105,
    StandardDoor = 150,
}

public enum EnemyDensity
{
    None,
    Single,
    Rare,
    Sparse,
    Common,
    Excessive
}

public enum ResourceBalance
{
    UnableToCalculate,
    Insufficient,
    BarelyEnough,
    Adequate,
    Comfortable,
    Generous
}

public enum TexturePart
{
    Middle,
    Upper,
    Lower
}

public enum AnimatedFlat
{
    None,
    BLOOD1,
    FWATER1,
    LAVA1,
    NUKAGE1,
    RROCK05,
    SLIME01,
    SLIME05
}

[Flags]
public enum ZDoomSectorSpecial
{
    Normal = 0,
    Light_Phased = 1,
    Light_PhasedSequenceStart = 2,
    Light_SequenceStep3 = 3,
    Light_SequenceStep4 = 4,
    Stairs_Normal = 26,
    Stairs_Synced = 27,
    Light_BlinkRandom = 65,
    Light_BlinkHalf = 66,
    Light_BlinkOne = 67,
    Combo_Damage20_BlinkHalf = 68,
    Damage_10Percent = 69,
    Damage_5Percent = 71,
    Light_Oscillate = 72,
    Door_Close30 = 74,
    End_Damage20 = 75,
    Light_BlinkHalfSync = 76,
    Light_BlinkOneSync = 77,
    Door_Open300 = 78,
    Damage_20Percent = 80,
    Light_FlickerRandom = 81,
    Damage_5Lava = 82,
    Damage_8Lava = 83,
    Combo_LavaScrollEastBlink = 84,
    Damage_4Sludge = 85,
    Fog_UseOutsideFog = 87,
    Damage_NukageCount_Plus2 = 105,
    Damage_InstantDeath = 115,
    Damage_NukageCount_Plus4 = 116,
    Scroller_WaterCurrent_Tagged = 118,
    AutomapHidden = 195,
    Healing = 196,
    LightningOutdoor = 197,
    Lightning_Flash_64 = 198,
    Lightning_Flash_32 = 199,
    Sky_UseSecondary = 200,
}


public enum DamagingSectorSpecial
{
    Combo_Damage20_BlinkHalf = 68,
    Damage_10Percent = 69,
    Damage_5Percent = 71,
    Damage_20Percent = 80,
    Damage_5Lava = 82,
    Damage_8Lava = 83,
    Damage_4Sludge = 85,
    Damage_NukageCount_Plus2 = 105,
    Damage_InstantDeath = 115,
    Damage_NukageCount_Plus4 = 116,
}
