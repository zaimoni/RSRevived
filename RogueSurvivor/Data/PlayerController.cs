// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.PlayerController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

//#define ALPHA_SAY

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

#if ALPHA_SAY
    public virtual void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      Actor.Says += HandleSay;
    }

    public virtual void LeaveControl()
    {
      base.LeaveControl();
      Actor.Says -= HandleSay;
    }
#endif

    public bool LastSeen(Location x, out int turn) { return m_itemMemory.HaveEverSeen(x,out turn); }

    public bool IsKnown(Location x) {
      int discard;
      return LastSeen(x, out discard);
    }

    public void ForceKnown(Point x) {   // for world creation
      m_itemMemory.Set(new Location(m_Actor.Location.Map, x), null, m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs> WhatHaveISeen() { return m_itemMemory.WhatHaveISeen(); }
    public Dictionary<Location, int> WhereIs(Gameplay.GameItems.IDs x) { return m_itemMemory.WhereIs(x); }

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

#if ALPHA_SAY
    private void HandleSay(object sender, Actor.SayArgs e)
    {
      Actor speaker = (sender as Actor);
      lock(speaker) {
        if (null == speaker || null == e._target || e.shown) return;
        if (m_Actor.IsSleeping) return;
        if (!CanSee(speaker.Location) && !CanSee(e._target.Location)) return;
        e.shown = true;
      }
      RogueForm.Game.PanViewportTo(m_Actor);

      if (e._important) RogueForm.Game.ClearMessages();
      foreach(Data.Message tmp in e.messages) {
        RogueForm.Game.AddMessage(tmp);
      }
      if (!e._important) return;

      RogueForm.Game.AddOverlay(new RogueGame.OverlayRect(Color.Yellow, new Rectangle(RogueForm.Game.MapToScreen(speaker.Location.Position), new Size(32, 32))));
      RogueForm.Game.AddMessagePressEnter();
      RogueForm.Game.ClearOverlays();
      RogueForm.Game.RemoveLastMessage();
      RogueForm.Game.RedrawPlayScreen();
    }
#endif
    }
}
