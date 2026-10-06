// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.Fortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  public sealed class Fortification : MapObject, IJsonStaged<Fortification>
    {
    public const int SMALL_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS/2;
    public const int LARGE_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS;

    public Fortification(bool isLarge) : base(isLarge ? Gameplay.GameImages.OBJ_LARGE_WOODEN_FORTIFICATION : Gameplay.GameImages.OBJ_SMALL_WOODEN_FORTIFICATION, Fire.BURNABLE) {}

    private Fortification(JsonElement src, JsonSerializerOptions options) : base(src, options) {}
    static Fortification IJsonStaged<Fortification>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

  }
}
