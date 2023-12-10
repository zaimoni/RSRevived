// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.PowerGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  internal sealed class PowerGenerator : StateMapObject, Zaimoni.Serialization.ISerialize
    {
    private const int STATE_OFF = 0;
    private const int STATE_ON = 1;

    // XXX there is currently only one type of power generator: the CHAR power generator.
    // The CHAR power generator is a Nikolai Tesla-esque magnetosphere power tap.
    // Typically, it would have 12 terawatts of power available to "import" from the solar wind.

    // It is unclear whether a solar storm (or solar minimum) has anything to do with the z apocalypse.

    // Since the z apocalypse is "near now", CHAR power generators are unlikely to be available outside 
    // of the military or the CHAR company town.

    // As such, we do not specifically have to track power generator types at this time.  Other options
    // would be gasoline or diesel.  There is little point modeling either of these without modeling the
    // refineries needed to make gasoline and diesel.

    static private readonly string[] m_imageIDs = new string[2] { Gameplay.GameImages.OBJ_POWERGEN_OFF, Gameplay.GameImages.OBJ_POWERGEN_ON };

    public bool IsOn { get => State == STATE_ON; }

    // While there is only one kind of power generator currently, the graphics
    // should be isolated from the constructor "just in case".
    public PowerGenerator() : base(m_imageIDs[0]) {}
#region implement Zaimoni.Serialization.ISerialize
    protected PowerGenerator(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion

    override protected string StateToID(int x) => m_imageIDs[x];

    override public void SetState(int newState) {
#if DEBUG
        if (0 > newState || m_imageIDs.Length <= newState) throw new ArgumentOutOfRangeException("newState unhandled");
#endif
        _update(newState);
    }


    public void TogglePower() => SetState(State == STATE_OFF ? STATE_ON : STATE_OFF);

    public void TogglePower(Actor a)
    {
      SetState(State == STATE_OFF ? STATE_ON : STATE_OFF);
      RogueGame.Game.OnMapPowerGeneratorSwitch(Location.Map, a);
    }
    }
}
