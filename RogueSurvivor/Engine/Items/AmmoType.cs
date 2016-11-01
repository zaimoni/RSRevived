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
    static internal string Singular(this AmmoType at)
    {
      switch (at) {
        case AmmoType.LIGHT_PISTOL: return "light pistol bullet";
        case AmmoType.HEAVY_PISTOL: return "heavy pistol bullet";
        case AmmoType.SHOTGUN: return "shotgun shell";
        case AmmoType.LIGHT_RIFLE: return "light rifle bullet";
        case AmmoType.HEAVY_RIFLE: return "heavy rifle bullet";
        case AmmoType.BOLT: return "bolt";
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled ammo type");
#else
        default: return "buggy bolt";
#endif
     }
   }
 
    static internal string Aggregate(this AmmoType at)  // morally PluralName
    {
      switch (at) {
        case AmmoType.LIGHT_PISTOL: return "light pistol clip";
        case AmmoType.HEAVY_PISTOL: return "heavy pistol clip";
        case AmmoType.SHOTGUN: return "shotgun cartridge";
        case AmmoType.LIGHT_RIFLE: return "light rifle clip";
        case AmmoType.HEAVY_RIFLE: return "heavy rifle clip";
        case AmmoType.BOLT: return "crossbow quiver";
#if DEBUG
        default: throw new ArgumentOutOfRangeException("unhandled ammo type");
#else
        default: return "buggy clip";
#endif
      }
    }
 
    static internal string Describe(this AmmoType at, bool plural=false)
    {
      if (AmmoType.SHOTGUN == at) return at.Aggregate();
      return at.Singular()+(plural ? "s" : ""); // regular plural in English
    }
  }
}
