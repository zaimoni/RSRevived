// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameFactions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameFactions : FactionDB
  {
    private static readonly Faction[] m_Factions = new Faction[(int) IDs._COUNT];

    public Faction this[int id] {
      get {
        return m_Factions[id];
      }
    }

    public Faction this[IDs id]
    {
      get {
        return this[(int) id];
      }
    }

    static public Faction TheArmy { get { return m_Factions[(int) IDs.TheArmy]; } }
    static public Faction TheBikers { get { return m_Factions[(int) IDs.TheBikers]; } }
    static public Faction TheBlackOps { get { return m_Factions[(int) IDs.TheBlackOps]; } }
    static public Faction TheCHARCorporation { get { return m_Factions[(int) IDs.TheCHARCorporation]; } }
    static public Faction TheCivilians { get { return m_Factions[(int) IDs.TheCivilians]; } }
    static public Faction TheGangstas { get { return m_Factions[(int) IDs.TheGangstas]; } }
    static public Faction ThePolice { get { return m_Factions[(int) IDs.ThePolice]; } }
    static public Faction TheUndeads { get { return m_Factions[(int) IDs.TheUndeads]; } }
    static public Faction ThePsychopaths { get { return m_Factions[(int) IDs.ThePsychopaths]; } }
    static public Faction TheSurvivors { get { return m_Factions[(int) IDs.TheSurvivors]; } }
    static public Faction TheFerals { get { return m_Factions[(int) IDs.TheFerals]; } }

    public GameFactions()
    {
      m_Factions[(int)IDs.TheArmy] = new Faction("Army", "soldier", (int)IDs.TheArmy, true);
      m_Factions[(int)IDs.TheBikers] = new Faction("Bikers", "biker", (int)IDs.TheBikers, true);
      m_Factions[(int)IDs.TheCHARCorporation] = new Faction("CHAR Corp.", "CHAR employee", (int)IDs.TheCHARCorporation, true);
      m_Factions[(int)IDs.TheBlackOps] = new Faction("BlackOps", "blackOp", (int)IDs.TheBlackOps, true);
      m_Factions[(int)IDs.TheCivilians] = new Faction("Civilians", "civilian", (int)IDs.TheCivilians);
      m_Factions[(int)IDs.TheGangstas] = new Faction("Gangstas", "gangsta", (int)IDs.TheGangstas, true);
      m_Factions[(int)IDs.ThePolice] = new Faction("Police", "police officer", (int)IDs.ThePolice, true);
      m_Factions[(int)IDs.TheUndeads] = new Faction("Undeads", "undead", (int)IDs.TheUndeads);
      m_Factions[(int)IDs.ThePsychopaths] = new Faction("Psychopaths", "psychopath", (int)IDs.ThePsychopaths);
      m_Factions[(int)IDs.TheSurvivors] = new Faction("Survivors", "survivor", (int)IDs.TheSurvivors);
      m_Factions[(int)IDs.TheFerals] = new Faction("Ferals", "feral", (int)IDs.TheFerals, true);

      // set up faction-level enemies
      // XXX now we have a working reflexive AddEnemy we can simplify this considerably
      this[IDs.TheArmy].AddEnemy(this[IDs.TheBikers]);
      this[IDs.TheArmy].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheArmy].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.TheArmy].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheArmy].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheBikers].AddEnemy(this[IDs.TheArmy]);
      this[IDs.TheBikers].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheBikers].AddEnemy(this[IDs.TheCHARCorporation]);
      this[IDs.TheBikers].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.TheBikers].AddEnemy(this[IDs.ThePolice]);
      this[IDs.TheBikers].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheBikers].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheArmy]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheBikers]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheCHARCorporation]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheCivilians]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.ThePolice]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheBlackOps].AddEnemy(this[IDs.TheSurvivors]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheArmy]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheBikers]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheCHARCorporation].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheCivilians].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheCivilians].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheCivilians].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.TheArmy]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.TheBikers]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.TheCHARCorporation]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.ThePolice]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheGangstas].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.ThePolice].AddEnemy(this[IDs.TheBikers]);
      this[IDs.ThePolice].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.ThePolice].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.ThePolice].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.ThePolice].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheArmy]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheBikers]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheCHARCorporation]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheCivilians]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.ThePolice]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheSurvivors]);
      this[IDs.TheUndeads].AddEnemy(this[IDs.TheFerals]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheArmy]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheBikers]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheCHARCorporation]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheCivilians]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheGangstas]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.ThePolice]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheSurvivors]);
      this[IDs.TheSurvivors].AddEnemy(this[IDs.TheBlackOps]);
      this[IDs.TheSurvivors].AddEnemy(this[IDs.TheUndeads]);
      this[IDs.TheSurvivors].AddEnemy(this[IDs.ThePsychopaths]);
      this[IDs.TheFerals].AddEnemy(this[IDs.TheUndeads]);

      // backstop.
      foreach (Faction mFaction in m_Factions)
      {
        foreach (Faction enemy in mFaction.Enemies)
        {
          if (!enemy.IsEnemyOf(mFaction))
            enemy.AddEnemy(mFaction);
        }
      }
    }

    // VAPORWARE: faction Failed Black Ops
    // true Black Ops are something like U.S. Navy SEALS or U.S. Army Rangers (i.e., they've gone through a 16-week-ish program including things like Hell Week)
    // that is, they really do *not* have material guilt about implementing orders to terminate civilians.
    // Failed Black Ops do experience material guilt and thus are not reliably hostile to civilians/survivors, and even less reliably hostile to National Guard and police.
    // At Day 0, there should be only 2,500-3,000 Black Ops Army and 2,500-3,000 Black Ops Navy
    // At Day 0, there should be about 3x as many Failed Black Ops that could be activated
    // * above assumes something like U.S. or U.K. level standing forces.
    // ** competent Communist banana republic: ~50%?  (Researchable: Cuba/Cold War)
    // ** incompetent Communist banana republic: ~10%?  (Researchable: Venezuela/2016)
    // ** Large Middle east dictatorship/oligarchy: ~25%? (researchable: Saudi Arabia, Iran, Iraq (check both Saddam Hussein and the U.S.-installed regime).  Climate doesn't match, however.)
    // ** India/China: 200%-300%? (researchable.  Again, climate doesn't match.)
    // ** anyone not mentioned other than Russia: 0%-10% (either from lack of interest pre-apocalypse or lack of resources)
    // recruiting more Black Ops takes some administrative overhead (to schedule the training session) and then 16 weeks.  Assuming pre-qualification standards are maintained,
    // the ratio of Black Ops to Failed Black Ops from the 16-week program will be 1:3
    // Weaker pre-qualification standards will only create more Failed Black Ops.
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

  static internal class GameFactions_ext
  {
    static public bool ExtortionIsAggression(this int x)
    {
      if ((int)GameFactions.IDs.ThePolice==x) return true;   // law enforcement
      if ((int)GameFactions.IDs.TheArmy==x) return true;     // martial law enforcement
      return false;
    }
    static public bool LawIgnoresExtortion(this int x)
    {
      if ((int)GameFactions.IDs.TheBikers==x) return true;   // enemy of police
      if ((int)GameFactions.IDs.TheGangstas==x) return true; // enemy of police
      if ((int)GameFactions.IDs.TheFerals==x) return true;   // people are more important than dogs
      return false;
    }
  }
}
