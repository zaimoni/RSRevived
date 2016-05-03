// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Corpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Corpse
  {
    private Actor m_DeadGuy;
    private int m_Turn;
    private Point m_Position;
    private float m_HitPoints;
    private int m_MaxHitPoints;
    private float m_Rotation;
    private float m_Scale;
    private Actor m_DraggedBy;

    public Actor DeadGuy
    {
      get
      {
        return m_DeadGuy;
      }
    }

    public int Turn
    {
      get
      {
        return m_Turn;
      }
    }

    public Point Position
    {
      get
      {
        return m_Position;
      }
      set
      {
                m_Position = value;
      }
    }

    public float HitPoints
    {
      get
      {
        return m_HitPoints;
      }
      set
      {
                m_HitPoints = value;
      }
    }

    public int MaxHitPoints
    {
      get
      {
        return m_MaxHitPoints;
      }
    }

    public float Rotation
    {
      get
      {
        return m_Rotation;
      }
      set
      {
                m_Rotation = value;
      }
    }

    public float Scale
    {
      get
      {
        return m_Scale;
      }
      set
      {
                m_Scale = Math.Max(0.0f, Math.Min(1f, value));
      }
    }

    public bool IsDragged
    {
      get
      {
        if (m_DraggedBy != null)
          return !m_DraggedBy.IsDead;
        return false;
      }
    }

    public Actor DraggedBy
    {
      get
      {
        return m_DraggedBy;
      }
      set
      {
                m_DraggedBy = value;
      }
    }

    public Corpse(Actor deadGuy, int hitPoints, int maxHitPoints, int corpseTurn, float rotation, float scale)
    {
            m_DeadGuy = deadGuy;
            m_Turn = corpseTurn;
            m_HitPoints = (float) hitPoints;
            m_MaxHitPoints = maxHitPoints;
            m_Rotation = rotation;
            m_Scale = scale;
            m_DraggedBy = (Actor) null;
    }
  }
}
