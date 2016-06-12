// Decompiled with JetBrains decompiler
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
    private Faction[] m_Factions = new Faction[(int) IDs._COUNT];

    public override Faction this[int id] {
      get {
        return m_Factions[id];
      }
    }

    public Faction this[GameFactions.IDs id]
    {
      get {
        return this[(int) id];
      }
      private set {
        m_Factions[(int) id] = value;
        m_Factions[(int) id].ID = (int) id;
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
      Models.Factions = (FactionDB) this;
      this[GameFactions.IDs.TheArmy] = new Faction("Army", "soldier", true);
      this[GameFactions.IDs.TheBikers] = new Faction("Bikers", "biker", true);
      this[GameFactions.IDs.TheBlackOps] = new Faction("BlackOps", "blackOp", true);
      this[GameFactions.IDs.TheCHARCorporation] = new Faction("CHAR Corp.", "CHAR employee", true);
      this[GameFactions.IDs.TheCivilians] = new Faction("Civilians", "civilian");
      this[GameFactions.IDs.TheGangstas] = new Faction("Gangstas", "gangsta", true);
      this[GameFactions.IDs.ThePolice] = new Faction("Police", "police officer", true);
      this[GameFactions.IDs.TheUndeads] = new Faction("Undeads", "undead");
      this[GameFactions.IDs.ThePsychopaths] = new Faction("Psychopaths", "psychopath");
      this[GameFactions.IDs.TheSurvivors] = new Faction("Survivors", "survivor");
      this[GameFactions.IDs.TheFerals] = new Faction("Ferals", "feral", true);
      
      // set up faction-level enemies
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
