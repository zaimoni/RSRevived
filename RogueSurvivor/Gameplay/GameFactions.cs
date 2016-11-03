﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameFactions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameFactions : FactionDB
  {
    public static readonly GameItems.IDs[] BAD_POLICE_OUTFITS = new GameItems.IDs[2]
    {
      GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,
      GameItems.IDs.ARMOR_HELLS_SOULS_JACKET
    };
    public static readonly GameItems.IDs[] GOOD_POLICE_OUTFITS = new GameItems.IDs[2]
    {
      GameItems.IDs.ARMOR_POLICE_JACKET,
      GameItems.IDs.ARMOR_POLICE_RIOT
    };
    private readonly Faction[] m_Factions = new Faction[(int) IDs._COUNT];

    public Faction this[int id] {
      get {
        return m_Factions[id];
      }
    }

    public Faction this[GameFactions.IDs id]
    {
      get {
        return this[(int) id];
      }
    }

    public Faction TheArmy {
      get {
        return this[GameFactions.IDs.TheArmy];
      }
    }

    public Faction TheBikers {
      get {
        return this[GameFactions.IDs.TheBikers];
      }
    }

    public Faction TheBlackOps {
      get {
        return this[GameFactions.IDs.TheBlackOps];
      }
    }

    public Faction TheCHARCorporation {
      get {
        return this[GameFactions.IDs.TheCHARCorporation];
      }
    }

    public Faction TheCivilians {
      get {
        return this[GameFactions.IDs.TheCivilians];
      }
    }

    public Faction TheGangstas {
      get {
        return this[GameFactions.IDs.TheGangstas];
      }
    }

    public Faction ThePolice {
      get {
        return this[GameFactions.IDs.ThePolice];
      }
    }

    public Faction TheUndeads {
      get {
        return this[GameFactions.IDs.TheUndeads];
      }
    }

    public Faction ThePsychopaths {
      get {
        return this[GameFactions.IDs.ThePsychopaths];
      }
    }

    public Faction TheSurvivors {
      get {
        return this[GameFactions.IDs.TheSurvivors];
      }
    }

    public Faction TheFerals {
      get {
        return this[GameFactions.IDs.TheFerals];
      }
    }

    public GameFactions()
    {
      Models.Factions = this;
      m_Factions[(int)GameFactions.IDs.TheArmy] = new Faction("Army", "soldier", (int)GameFactions.IDs.TheArmy, true);
      m_Factions[(int)GameFactions.IDs.TheBikers] = new Faction("Bikers", "biker", (int)GameFactions.IDs.TheBikers, true);
      m_Factions[(int)GameFactions.IDs.TheCHARCorporation] = new Faction("CHAR Corp.", "CHAR employee", (int)GameFactions.IDs.TheCHARCorporation, true);
      m_Factions[(int)GameFactions.IDs.TheBlackOps] = new Faction("BlackOps", "blackOp", (int)GameFactions.IDs.TheBlackOps, true);
      m_Factions[(int)GameFactions.IDs.TheCivilians] = new Faction("Civilians", "civilian", (int)GameFactions.IDs.TheCivilians);
      m_Factions[(int)GameFactions.IDs.TheGangstas] = new Faction("Gangstas", "gangsta", (int)GameFactions.IDs.TheGangstas, true);
      m_Factions[(int)GameFactions.IDs.ThePolice] = new Faction("Police", "police officer", (int)GameFactions.IDs.ThePolice, true);
      m_Factions[(int)GameFactions.IDs.TheUndeads] = new Faction("Undeads", "undead", (int)GameFactions.IDs.TheUndeads);
      m_Factions[(int)GameFactions.IDs.ThePsychopaths] = new Faction("Psychopaths", "psychopath", (int)GameFactions.IDs.ThePsychopaths);
      m_Factions[(int)GameFactions.IDs.TheSurvivors] = new Faction("Survivors", "survivor", (int)GameFactions.IDs.TheSurvivors);
      m_Factions[(int)GameFactions.IDs.TheFerals] = new Faction("Ferals", "feral", (int)GameFactions.IDs.TheFerals, true);
      
      // set up faction-level enemies
      // XXX now we have a working reflexive AddEnemy we can simplify this considerably
      this[GameFactions.IDs.TheArmy].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.TheArmy].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheArmy].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.TheArmy].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheArmy].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.TheCHARCorporation]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.ThePolice]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheBikers].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheCHARCorporation]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheCivilians]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.ThePolice]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheBlackOps].AddEnemy(this[GameFactions.IDs.TheSurvivors]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheCHARCorporation].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheCivilians].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheCivilians].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheCivilians].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.TheCHARCorporation]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.ThePolice]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheGangstas].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.ThePolice].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.ThePolice].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.ThePolice].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.ThePolice].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.ThePolice].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheCHARCorporation]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheCivilians]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.ThePolice]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheSurvivors]);
      this[GameFactions.IDs.TheUndeads].AddEnemy(this[GameFactions.IDs.TheFerals]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheArmy]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheBikers]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheCHARCorporation]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheCivilians]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheGangstas]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.ThePolice]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.ThePsychopaths].AddEnemy(this[GameFactions.IDs.TheSurvivors]);
      this[GameFactions.IDs.TheSurvivors].AddEnemy(this[GameFactions.IDs.TheBlackOps]);
      this[GameFactions.IDs.TheSurvivors].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      this[GameFactions.IDs.TheSurvivors].AddEnemy(this[GameFactions.IDs.ThePsychopaths]);
      this[GameFactions.IDs.TheFerals].AddEnemy(this[GameFactions.IDs.TheUndeads]);
      foreach (Faction mFaction in m_Factions)
      {
        foreach (Faction enemy in mFaction.Enemies)
        {
          if (!enemy.IsEnemyOf(enemy))
            enemy.AddEnemy(mFaction);
        }
      }
    }
    
    public enum IDs
    {
      TheCHARCorporation = 0,
      TheCivilians = 1,
      TheUndeads = 2,
      TheArmy = 3,
      TheBikers = 4,
      TheGangstas = 5,
      ThePolice = 6,
      TheBlackOps = 7,
      ThePsychopaths = 8,
      TheSurvivors = 9,
      TheFerals = 10,
      _COUNT = 11,
    }
  }
}
