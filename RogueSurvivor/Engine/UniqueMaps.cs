// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueMaps
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueMaps
  {
    public UniqueMap CHARUndergroundFacility { get; set; }
    public UniqueMap PoliceStation_OfficesLevel { get; set; }
    public UniqueMap PoliceStation_JailsLevel { get; set; }
    public UniqueMap Hospital_Admissions { get; set; }
    public UniqueMap Hospital_Offices { get; set; }
    public UniqueMap Hospital_Patients { get; set; }
    public UniqueMap Hospital_Storage { get; set; }
    public UniqueMap Hospital_Power { get; set; }
  }
}
