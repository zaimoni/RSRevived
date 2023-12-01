// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueMaps
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using Map = djack.RogueSurvivor.Data.Map;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueMaps : Zaimoni.Serialization.ISerialize
    {
    public UniqueMap CHARUndergroundFacility { get; set; }
    public UniqueMap PoliceStation_OfficesLevel { get; set; }
    public UniqueMap PoliceStation_JailsLevel { get; set; }
    public UniqueMap Hospital_Admissions { get; set; }
    public UniqueMap Hospital_Offices { get; set; }
    public UniqueMap Hospital_Patients { get; set; }
    public UniqueMap Hospital_Storage { get; set; }
    public UniqueMap Hospital_Power { get; set; }

    public UniqueMaps() { }

#region implement Zaimoni.Serialization.ISerialize
    protected UniqueMaps(Zaimoni.Serialization.DecodeObjects decode)
    {
//        byte relay_b = 0;
//        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref relay_b);

        ulong code;
        Map stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) CHARUndergroundFacility = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(CHARUndergroundFacility));
        } else {
            CHARUndergroundFacility = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) PoliceStation_OfficesLevel = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(PoliceStation_OfficesLevel));
        } else {
            PoliceStation_OfficesLevel = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) PoliceStation_JailsLevel = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(PoliceStation_JailsLevel));
        } else {
            PoliceStation_JailsLevel = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) Hospital_Admissions = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(Hospital_Admissions));
        } else {
            Hospital_Admissions = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) Hospital_Offices = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(Hospital_Offices));
        } else {
            Hospital_Offices = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) Hospital_Patients = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(Hospital_Patients));
        } else {
            Hospital_Patients = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) Hospital_Storage = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(Hospital_Storage));
        } else {
            Hospital_Storage = new(stage);
        }

        stage = decode.Load<Map>(out code);
        if (null == stage) {
            if (0 < code) {
                decode.Schedule(code, (o) => {
                    if (o is Map w) Hospital_Power = new(w);
                    else throw new InvalidOperationException("Map object not loaded");
                });
            } else throw new ArgumentNullException(nameof(Hospital_Power));
        } else {
            Hospital_Power = new(stage);
        }
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
    {
//      Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)8); // fail-fast if things don't add up?

        var code = encode.Saving(CHARUndergroundFacility.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("CHARUndergroundFacility.TheMap");

        code = encode.Saving(PoliceStation_OfficesLevel.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("PoliceStation_OfficesLevel.TheMap");

        code = encode.Saving(PoliceStation_JailsLevel.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("PoliceStation_JailsLevel.TheMap");

        code = encode.Saving(Hospital_Admissions.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("Hospital_Admissions.TheMap");

        code = encode.Saving(Hospital_Offices.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("Hospital_Offices.TheMap");

        code = encode.Saving(Hospital_Patients.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("Hospital_Patients.TheMap");

        code = encode.Saving(Hospital_Storage.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("Hospital_Storage.TheMap");

        code = encode.Saving(Hospital_Power.TheMap);
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException("Hospital_Power.TheMap");
    }
#endregion

    /// <returns>Key is towards surface; Value is deeper</returns>
    public KeyValuePair<Map,Map>? NavigatePoliceStation(Map x)
    {
      if (PoliceStation_OfficesLevel.TheMap == x) return new KeyValuePair<Map,Map>(x.District.EntryMap, PoliceStation_JailsLevel.TheMap);
      if (PoliceStation_JailsLevel.TheMap == x) return new KeyValuePair<Map,Map>(PoliceStation_OfficesLevel.TheMap,null);
      return null;
    }

    /// <returns>Key is towards surface; Value is deeper</returns>
    public KeyValuePair<Map,Map>? NavigateHospital(Map x)
    {
      if (Hospital_Admissions.TheMap == x) return new KeyValuePair<Map,Map>(x.District.EntryMap, Hospital_Offices.TheMap);
      if (Hospital_Offices.TheMap == x) return new KeyValuePair<Map,Map>(Hospital_Admissions.TheMap, Hospital_Patients.TheMap);
      if (Hospital_Patients.TheMap == x) return new KeyValuePair<Map,Map>(Hospital_Offices.TheMap, Hospital_Storage.TheMap);
      if (Hospital_Storage.TheMap == x) return new KeyValuePair<Map,Map>(Hospital_Patients.TheMap, Hospital_Power.TheMap);
      if (Hospital_Power.TheMap == x) return new KeyValuePair<Map,Map>(Hospital_Storage.TheMap,null);
      return null;
    }

    // numerical representation
    public int PoliceStationDepth(Map x)
    {
      if (PoliceStation_OfficesLevel.TheMap == x) return 1;
      if (PoliceStation_JailsLevel.TheMap == x) return 2;
      return 0;
    }

    public Map? PoliceStationMap(int code)
    {
      switch(code)
      {
      case 1: return PoliceStation_OfficesLevel.TheMap;
      case 2: return PoliceStation_JailsLevel.TheMap;
      default: return PoliceStation_OfficesLevel.TheMap.District.EntryMap; // not really, but makes certain algorithms work
      }
    }

    public int HospitalDepth(Map x)
    {
      if (Hospital_Admissions.TheMap == x) return 1;
      if (Hospital_Offices.TheMap == x) return 2;
      if (Hospital_Patients.TheMap == x) return 3;
      if (Hospital_Storage.TheMap == x) return 4;
      if (Hospital_Power.TheMap == x) return 5;
      return 0;
    }

    public Map? HospitalMap(int code)
    {
      switch(code)
      {
      case 1: return Hospital_Admissions.TheMap;
      case 2: return Hospital_Offices.TheMap;
      case 3: return Hospital_Patients.TheMap;
      case 4: return Hospital_Storage.TheMap;
      case 5: return Hospital_Power.TheMap;
      default: return Hospital_Admissions.TheMap.District.EntryMap; // not really, but makes certain algorithms work
      }
    }

    public Data.ZoneLoc PoliceLanding()
    {
      var src = PoliceStation_OfficesLevel.TheMap;
      var m = src.District.EntryMap;
      var zones = m.GetZonesAt(src.FirstExitFor(m).Value.Value.Location.Position);
      return new Data.ZoneLoc(m, zones[0]);
    }

    public Data.ZoneLoc HospitalLanding()
    {
      var src = Hospital_Admissions.TheMap;
      var m = src.District.EntryMap;
      var zones = m.GetZonesAt(src.FirstExitFor(m).Value.Value.Location.Position);
      return new Data.ZoneLoc(m, zones[0]);
    }
  }
}
