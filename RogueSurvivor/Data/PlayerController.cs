// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.PlayerController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class PlayerController : ActorController
  {
    private Gameplay.AI.Sensors.LOSSensor m_LOSSensor;
    private Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_itemMemory;

    public PlayerController() { 
      m_LOSSensor = new Gameplay.AI.Sensors.LOSSensor(Gameplay.AI.Sensors.LOSSensor.SensingFilter.ACTORS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.ITEMS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.CORPSES);
      m_itemMemory = new Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
    }

    public bool LastSeen(Location x, out int turn) {
      return m_itemMemory.HaveEverSeen(x,out turn);
    }

    public bool HaveSeen(Location x) {
      int discard;
      return LastSeen(x, out discard);
    }

    public void ForceSeen(Point x) {   // for world creation
      m_itemMemory.Set(new Location(m_Actor.Location.Map, x), null, m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    public List<Engine.AI.Percept> UpdateSensors(RogueGame game)
    {
      List<Engine.AI.Percept> tmp = m_LOSSensor.Sense(game, m_Actor);

      // update the enhanced item memory here
      Dictionary<Location,HashSet< Gameplay.GameItems.IDs >> seen_items = new Dictionary<Location, HashSet<Gameplay.GameItems.IDs>>();
      foreach(Engine.AI.Percept tmp2 in tmp) {
        Inventory tmp3 = tmp2.Percepted as Inventory;
        if (null == tmp3) continue;
        if (0 >= tmp3.CountItems) continue;
        seen_items[tmp2.Location] = new HashSet<Gameplay.GameItems.IDs>(tmp3.Items.Select(x => x.Model.ID));
      }
      foreach(Point tmp2 in FOV) {
        Location tmp3 = new Location(m_Actor.Location.Map,tmp2);
        if (seen_items.ContainsKey(tmp3)) { m_itemMemory.Set(tmp3, seen_items[tmp3], m_Actor.Location.Map.LocalTime.TurnCounter); }
        else { m_itemMemory.Set(tmp3, null, m_Actor.Location.Map.LocalTime.TurnCounter); }
      }

      return tmp;
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }

    public override ActorAction GetAction(RogueGame game)
    {
      throw new InvalidOperationException("do not call PlayerController.GetAction()");
    }
  }
}
