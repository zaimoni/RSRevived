// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.InsaneHumanAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class InsaneHumanAI : BaseAI
  {
    private static readonly string[] INSANITIES = new string[56]
    {
      "WHY WALK THERE?",
      "WHAT MAKES YOU FUN?",
      "YOU WEAR TOO MUCH COLORS!",
      "YOU HAVE BAD HABITS!",
      "MOM DIDN'T HANG ME!",
      "LUCIE! LUCIA!",
      "WHAT EGGS AND PASTA?",
      "TURTLE CATS!",
      "I REMEMBER THE CRABS!",
      "IT WAS AFTER THAT NOW!",
      "CUT IT! CUT IT NOW!",
      "DECEASED TOES!",
      "ICE-CREAM COPS!",
      "I SAW THAT FUCKING TWICE! TWICE! TWICE!",
      "FUCK BASTARD SAUSAGE!",
      "HEY YOU! STOP MOVING THE FLOOR!",
      "DROP THE FUCKING EGGS NOW!",
      "YOU GO FIRST AFTER ME!",
      "IT HURTS BUT ITS OK!",
      "LAST TIME WAS OK...",
      "SHE ISN'T NOT YET!",
      "I WAS CRAWLING HAHA!",
      "ROLLING LIKE AN EGG!",
      "THAT IS NOT DECENT!",
      "JUMP LIKE A FLOWER!",
      "NIGGER TRIGGER!",
      "CHEESE LIKE THESE...",
      "ILL-ADVISED LOBSTER!",
      "SSSHHHH! SILENCE... DO YOU SMELL?",
      "NOTHING BEATS. NOTHING!",
      "GROWN-UP MEN DON'T DO THAT!",
      "BARN BUSTER!",
      "SUPER SUPER?",
      "ONE MORE PASTA CRAP!",
      "LAZY LADY!",
      "I HATE TAP WATER!",
      "STILL WANKING FOR FOOD?",
      "LOOK! IT FITS LIKE A HOLE!",
      "PESKY POLAR PRANKS!",
      "LITTLE BY LITTLE YOU DIE...",
      "PLEASE TIE YOUR NECK PROPERLY!",
      "I SEE WHAT I SHIT ALL THE TIME!",
      "THAT'S FUCKING ANNOYING!",
      "I UNLOCK THE WALLS!",
      "I'M NOT SO SURE NOW!?",
      "RUSTY BUT TRUSTY!",
      "CHEESE LICKER!",
      "LAUNDRY TIME AGAIN AND AGAIN!",
      "DON'T YOU SEE I'M ASSEMBLED?",
      "MEXICAN MIDGETS!",
      "RAZOR RASCALS!",
      "PUNCH MY BALLS!",
      "STUCK IN A VICIOUS SQUARE!",
      "HORSE HOLSTER!",
      "THAT WAS COMPLETLY UNCALLED FOR!",
      "ROBOTS WON'T FOOL ME!"
    };
    // private const int ATTACK_CHANCE = 80;    // alpha 10.1: obsolete
    private const int SHOUT_CHANCE = 80;
    private const int USE_EXIT_CHANCE = 50;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private readonly LOSSensor m_LOSSensor = new LOSSensor(VISION_SEES);

    public InsaneHumanAI()
    {
    }

    public override List<Percept> UpdateSensors()
    {
      return m_LOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    public override Dictionary<Location, Actor> friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor> enemies_in_FOV { get { return m_LOSSensor.enemies; } }
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

    protected override ActorAction SelectAction(RogueGame game)
    {
      m_Actor.Walk();    // alpha 10: don't run by default
      ActorAction tmpAction = BehaviorEquipWeapon();
      if (null != tmpAction) return tmpAction;

      /* if (game.Rules.RollChance(ATTACK_CHANCE)) */ { // alpha 10.1: unconditional
        if (null != (_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))))) {
          tmpAction = TargetGridMelee(_enemies);
          if (null != tmpAction) return tmpAction;
        }
      }
      if (game.Rules.RollChance(SHOUT_CHANCE)) {
        m_Actor.Activity = Activity.IDLE;
        game.DoEmote(m_Actor, game.Rules.DiceRoller.Choose(INSANITIES),true);
      }
      if (game.Rules.RollChance(USE_EXIT_CHANCE)) {
        tmpAction = BehaviorUseExit(UseExitFlags.BREAK_BLOCKING_OBJECTS | UseExitFlags.ATTACK_BLOCKING_ENEMIES);
        if (null != tmpAction) return tmpAction;
      }
      return BehaviorWander();
    }
  }
}
