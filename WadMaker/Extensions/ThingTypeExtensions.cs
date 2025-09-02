public static class ThingTypeExtensions
{
    public static int GetHp(this ThingType type)
    {
        var maybeMonsterHp = DoomConfig.MonsterHp.Try(type);
        return maybeMonsterHp?.Health ?? 0;
    }

    public static ThingCategory Category(this ThingType type) =>
        type switch
        {
            ThingType.Arachnotron => ThingCategory.Monster,
            ThingType.Baron_of_Hell => ThingCategory.Monster,
            ThingType.Cacodemon => ThingCategory.Monster,
            ThingType.Demon => ThingCategory.Monster,
            ThingType.Imp => ThingCategory.Monster,
            ThingType.Lost_soul => ThingCategory.Monster,
            ThingType.Spectre => ThingCategory.Monster,
            ThingType.Zombieman => ThingCategory.Monster,
            ThingType.Hell_knight => ThingCategory.Monster,
            ThingType.Mancubus => ThingCategory.Monster,
            ThingType.Pain_elemental => ThingCategory.Monster,
            ThingType.Commander_Keen => ThingCategory.Monster,
            ThingType.Shotgun => ThingCategory.Weapon,
            ThingType.Chaingun => ThingCategory.Weapon,
            ThingType.Rocket_launcher => ThingCategory.Weapon,
            ThingType.Plasma_gun => ThingCategory.Weapon,
            ThingType.BFG9000 => ThingCategory.Weapon,
            ThingType.Four_shotgun_shells => ThingCategory.Ammo,
            ThingType.Box_of_shotgun_shells => ThingCategory.Ammo,
            ThingType.Clip => ThingCategory.Ammo,
            ThingType.Box_of_bullets => ThingCategory.Ammo,
            ThingType.Rocket => ThingCategory.Ammo,
            ThingType.Box_of_rockets => ThingCategory.Ammo,
            ThingType.Energy_cell => ThingCategory.Ammo,
            ThingType.Energy_cell_pack => ThingCategory.Ammo,
            ThingType.Player1_start => ThingCategory.PlayerStart,
            _ => throw new Exception($"Unknown thing category for {type}")  
        };

    public static ThingType SmallAmmoType(this ThingType type) =>
        type switch
        {
            ThingType.Shotgun => ThingType.Four_shotgun_shells,
            ThingType.Chaingun => ThingType.Clip,
            ThingType.Rocket_launcher => ThingType.Rocket,
            ThingType.Plasma_gun => ThingType.Energy_cell,
            ThingType.BFG9000 => ThingType.Energy_cell,
            _ => ThingType.Unknown
        };

    public static int GetMeanWeaponDamage(this ThingType type)
    {
        var maybeWeaponDamage = DoomConfig.WeaponDamage.Try(type);
        return maybeWeaponDamage?.MeanDamage ?? 0;
    }   

    public static (ThingType, int)[] AmmoWeaponAmounts(this ThingType type) =>
      type switch
      {
          ThingType.Four_shotgun_shells => [(ThingType.Shotgun, 4)],
          ThingType.Box_of_shotgun_shells => [(ThingType.Shotgun, 20)],
          ThingType.Shotgun => [(ThingType.Shotgun, 8)],

          ThingType.Clip => [(ThingType.Chaingun, 10)],
          ThingType.Box_of_bullets => [(ThingType.Chaingun, 50)],
          ThingType.Chaingun => [(ThingType.Chaingun, 20)],

          ThingType.Rocket => [(ThingType.Rocket_launcher, 1)],
          ThingType.Box_of_rockets => [(ThingType.Rocket_launcher, 5)],
          ThingType.Rocket_launcher => [(ThingType.Rocket_launcher, 2)],

          ThingType.Energy_cell => [(ThingType.Plasma_gun, 20)],
          ThingType.Energy_cell_pack => [(ThingType.Plasma_gun, 100)],
          ThingType.Plasma_gun => [(ThingType.Plasma_gun, 40)],
          ThingType.BFG9000 => [(ThingType.Plasma_gun, 40)],

          ThingType.Backpack => [
          (ThingType.Chaingun, 10),
        (ThingType.Shotgun, 4),
        (ThingType.Rocket_launcher, 1),
        (ThingType.Plasma_gun, 20)
        ],
          _ => []
      };
}