// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameFactions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  internal static class GameFactions
  {
    private static readonly Faction[] m_Factions = new Faction[(int) IDs._COUNT];

    static public Faction From(int id) { return m_Factions[id]; }
    static public Faction From(IDs id) { return m_Factions[(int)id]; }

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

    static GameFactions()
    {
      m_Factions[(int)IDs.TheArmy] = new Faction("Army", "soldier", IDs.TheArmy, true);
      m_Factions[(int)IDs.TheBikers] = new Faction("Bikers", "biker", IDs.TheBikers, true);
      m_Factions[(int)IDs.TheCHARCorporation] = new Faction("CHAR Corp.", "CHAR employee", IDs.TheCHARCorporation, true);
      m_Factions[(int)IDs.TheBlackOps] = new Faction("BlackOps", "blackOp", IDs.TheBlackOps, true);
      m_Factions[(int)IDs.TheCivilians] = new Faction("Civilians", "civilian", IDs.TheCivilians);
      m_Factions[(int)IDs.TheGangstas] = new Faction("Gangstas", "gangsta", IDs.TheGangstas, true);
      m_Factions[(int)IDs.ThePolice] = new Faction("Police", "police officer", IDs.ThePolice, true);
      m_Factions[(int)IDs.TheUndeads] = new Faction("Undeads", "undead", IDs.TheUndeads);
      m_Factions[(int)IDs.ThePsychopaths] = new Faction("Psychopaths", "psychopath", IDs.ThePsychopaths);
      m_Factions[(int)IDs.TheSurvivors] = new Faction("Survivors", "survivor", IDs.TheSurvivors);
      m_Factions[(int)IDs.TheFerals] = new Faction("Ferals", "feral", IDs.TheFerals, true);

      // set up faction-level enemies
      // XXX now we have a working reflexive AddEnemy we can simplify this considerably
      From(IDs.TheArmy).AddEnemy(From(IDs.TheBikers));
      From(IDs.TheArmy).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheArmy).AddEnemy(From(IDs.TheGangstas));
      From(IDs.TheArmy).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheArmy).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheBikers).AddEnemy(From(IDs.TheArmy));
      From(IDs.TheBikers).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheBikers).AddEnemy(From(IDs.TheCHARCorporation));
      From(IDs.TheBikers).AddEnemy(From(IDs.TheGangstas));
      From(IDs.TheBikers).AddEnemy(From(IDs.ThePolice));
      From(IDs.TheBikers).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheBikers).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheArmy));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheBikers));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheCHARCorporation));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheCivilians));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheGangstas));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.ThePolice));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheBlackOps).AddEnemy(From(IDs.TheSurvivors));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.TheArmy));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.TheBikers));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.TheGangstas));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheCHARCorporation).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheCivilians).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheCivilians).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheCivilians).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheGangstas).AddEnemy(From(IDs.TheArmy));
      From(IDs.TheGangstas).AddEnemy(From(IDs.TheBikers));
      From(IDs.TheGangstas).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheGangstas).AddEnemy(From(IDs.TheCHARCorporation));
      From(IDs.TheGangstas).AddEnemy(From(IDs.ThePolice));
      From(IDs.TheGangstas).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheGangstas).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.ThePolice).AddEnemy(From(IDs.TheBikers));
      From(IDs.ThePolice).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.ThePolice).AddEnemy(From(IDs.TheGangstas));
      From(IDs.ThePolice).AddEnemy(From(IDs.TheUndeads));
      From(IDs.ThePolice).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheArmy));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheBikers));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheCHARCorporation));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheCivilians));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheGangstas));
      From(IDs.TheUndeads).AddEnemy(From(IDs.ThePolice));
      From(IDs.TheUndeads).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheSurvivors));
      From(IDs.TheUndeads).AddEnemy(From(IDs.TheFerals));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheArmy));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheBikers));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheCHARCorporation));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheCivilians));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheGangstas));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.ThePolice));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheUndeads));
      From(IDs.ThePsychopaths).AddEnemy(From(IDs.TheSurvivors));
      From(IDs.TheSurvivors).AddEnemy(From(IDs.TheBlackOps));
      From(IDs.TheSurvivors).AddEnemy(From(IDs.TheUndeads));
      From(IDs.TheSurvivors).AddEnemy(From(IDs.ThePsychopaths));
      From(IDs.TheFerals).AddEnemy(From(IDs.TheUndeads));

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
    static public bool ExtortionIsAggression(this GameFactions.IDs x)
    {
      if (GameFactions.IDs.ThePolice==x) return true;   // law enforcement
      if (GameFactions.IDs.TheArmy==x) return true;     // martial law enforcement
      return false;
    }
    static public bool LawIgnoresExtortion(this GameFactions.IDs x)
    {
      if (GameFactions.IDs.TheBikers==x) return true;   // enemy of police
      if (GameFactions.IDs.TheGangstas==x) return true; // enemy of police
      if (GameFactions.IDs.TheFerals==x) return true;   // people are more important than dogs
      return false;
    }
  }
}
