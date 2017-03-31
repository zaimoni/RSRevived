// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.AI.Sensor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Engine.AI
{
  internal interface Sensor
  {
    List<Percept> Sense(Actor actor);
  }
}
