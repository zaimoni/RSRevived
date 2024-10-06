// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.Fortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  public sealed class Fortification : MapObject, Zaimoni.Serialization.ISerialize
    {
    public const int SMALL_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS/2;
    public const int LARGE_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS;

    public Fortification(string imageID) : base(imageID, Fire.BURNABLE) {}

#region implement Zaimoni.Serialization.ISerialize
    protected Fortification(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion

  }
}
