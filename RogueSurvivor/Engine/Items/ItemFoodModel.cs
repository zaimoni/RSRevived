// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemFoodModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemFoodModel : ItemModel
  {
    private int m_Nutrition;
    private bool m_IsPerishable;
    private int m_BestBeforeDays;

    public int Nutrition
    {
      get
      {
        return m_Nutrition;
      }
    }

    public bool IsPerishable
    {
      get
      {
        return m_IsPerishable;
      }
    }

    public int BestBeforeDays
    {
      get
      {
        return m_BestBeforeDays;
      }
    }

    public ItemFoodModel(string aName, string theNames, string imageID, int nutrition, int bestBeforeDays)
      : base(aName, theNames, imageID)
    {
            m_Nutrition = nutrition;
      if (bestBeforeDays < 0)
      {
                m_IsPerishable = false;
      }
      else
      {
                m_IsPerishable = true;
                m_BestBeforeDays = bestBeforeDays;
      }
    }
  }
}
