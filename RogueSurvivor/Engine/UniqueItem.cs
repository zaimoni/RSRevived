// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal readonly struct UniqueItem
  {
    public readonly Item TheItem;
    public readonly bool IsSpawned;

    public UniqueItem(Item src, bool spawn=true)
    {
      TheItem = src;
      IsSpawned = spawn;
    }
  }
}
