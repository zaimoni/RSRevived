// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.AmmoType
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine.Items
{
  internal enum AmmoType
  {
    LIGHT_PISTOL = 0,
    HEAVY_PISTOL = 1,
    SHOTGUN = 2,
    LIGHT_RIFLE = 3,
    HEAVY_RIFLE = 4,
    BOLT = 5,
//  _COUNT = 6,
  }

  static internal class ExtensionAmmoType
  {
    static internal string Describe(this AmmoType at)  // morally PluralName
    {
      switch (at) {
        case AmmoType.LIGHT_PISTOL: return "light pistol bullets";
        case AmmoType.HEAVY_PISTOL: return "heavy pistol bullets";
        case AmmoType.SHOTGUN: return "shotgun cartridge";
        case AmmoType.LIGHT_RIFLE: return "light rifle bullets";
        case AmmoType.HEAVY_RIFLE: return "heavy rifle bullets";
        case AmmoType.BOLT: return "bolts";
        default: throw new ArgumentOutOfRangeException("unhandled ammo type");
      }
   }
  }
}
