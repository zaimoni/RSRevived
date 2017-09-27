// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.PlayerController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using ObjectiveAI = djack.RogueSurvivor.Gameplay.AI.ObjectiveAI;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class PlayerController : ObjectiveAI
    {
    private Gameplay.AI.Sensors.LOSSensor m_LOSSensor;
    private Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_itemMemory;

	public PlayerController() {
      // XXX filter should be by the normal filter type of the AI being substituted for
      m_LOSSensor = new Gameplay.AI.Sensors.LOSSensor(Gameplay.AI.Sensors.LOSSensor.SensingFilter.ACTORS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.ITEMS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.CORPSES);
      m_itemMemory = new Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
    }

    public override Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory {
       get {
         return m_itemMemory;
       }
    }

	private Gameplay.AI.Sensors.LOSSensor.SensingFilter VISION_SEES() {
	  switch(m_Actor.Model.DefaultController.Name)
	  {
	  case "CHARGuardAI": return Gameplay.AI.CHARGuardAI.VISION_SEES;
	  case "CivilianAI": return Gameplay.AI.CivilianAI.VISION_SEES;
	  case "FeralDogAI": return Gameplay.AI.FeralDogAI.VISION_SEES;
	  case "GangAI": return Gameplay.AI.GangAI.VISION_SEES;
	  case "InsaneHumanAI": return Gameplay.AI.InsaneHumanAI.VISION_SEES;
	  case "RatAI": return Gameplay.AI.RatAI.VISION_SEES;
	  case "SewersThingAI": return Gameplay.AI.SewersThingAI.VISION_SEES;
	  case "SkeletonAI": return Gameplay.AI.SkeletonAI.VISION_SEES;
	  case "SoldierAI": return Gameplay.AI.SoldierAI.VISION_SEES;
	  case "ZombieAI": return Gameplay.AI.ZombieAI.VISION_SEES;
	  default: return Gameplay.AI.Sensors.LOSSensor.SensingFilter.ACTORS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.ITEMS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.CORPSES;
	  }
	}

	public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      Actor.Says += HandleSay;
      if ((int)Gameplay.GameFactions.IDs.ThePolice == actor.Faction.ID) {
        // use police item memory rather than ours
        m_itemMemory = Session.Get.PoliceItemMemory;
      }
	  // deal with vision capabilities
      m_LOSSensor = new Gameplay.AI.Sensors.LOSSensor(VISION_SEES());
      SensorsOwnedBy(actor);
    }

    public override void LeaveControl()
    {
      base.LeaveControl();
      Actor.Says -= HandleSay;
    }

    public override List<Percept> UpdateSensors()
    {
      return m_LOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

    public override ActorAction GetAction(RogueGame game)
    {
      throw new InvalidOperationException("do not call PlayerController.GetAction()");
    }

    protected override ActorAction SelectAction(RogueGame game)
    {
      throw new InvalidOperationException("do not call PlayerController.SelectAction()");
    }

    public override bool IsInterestingTradeItem(Actor speaker, Item offeredItem)
    {
      return true;
    }

    private void HandleSay(object sender, Actor.SayArgs e)
    {
      Actor speaker = (sender as Actor);
      if (null == speaker) throw new ArgumentNullException(nameof(sender));
      if (null == e._target) throw new ArgumentNullException("e.target");
      lock (speaker) {
        if (e.shown) return;
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

      RogueForm.Game.AddOverlay(new RogueGame.OverlayRect(Color.Yellow, new Rectangle(RogueForm.Game.MapToScreen(speaker.Location), RogueGame.SIZE_OF_ACTOR)));
      RogueForm.Game.AddMessagePressEnter();
      RogueForm.Game.ClearOverlays();
      RogueForm.Game.RemoveLastMessage();
      RogueForm.Game.RedrawPlayScreen();
    }
  }
}
