public static class ThingTypeExtensions
{
    public static int GetHp(this ThingType type)
    {
        var maybeMonsterHp = DoomConfig.MonsterHp.Try(type);
        return maybeMonsterHp?.Health ?? 0;
    }

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