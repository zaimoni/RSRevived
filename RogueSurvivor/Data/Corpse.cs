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
        return this.m_DeadGuy;
      }
    }

    public int Turn
    {
      get
      {
        return this.m_Turn;
      }
    }

    public Point Position
    {
      get
      {
        return this.m_Position;
      }
      set
      {
        this.m_Position = value;
      }
    }

    public float HitPoints
    {
      get
      {
        return this.m_HitPoints;
      }
      set
      {
        this.m_HitPoints = value;
      }
    }

    public int MaxHitPoints
    {
      get
      {
        return this.m_MaxHitPoints;
      }
    }

    public float Rotation
    {
      get
      {
        return this.m_Rotation;
      }
      set
      {
        this.m_Rotation = value;
      }
    }

    public float Scale
    {
      get
      {
        return this.m_Scale;
      }
      set
      {
        this.m_Scale = Math.Max(0.0f, Math.Min(1f, value));
      }
    }

    public bool IsDragged
    {
      get
      {
        if (this.m_DraggedBy != null)
          return !this.m_DraggedBy.IsDead;
        return false;
      }
    }

    public Actor DraggedBy
    {
      get
      {
        return this.m_DraggedBy;
      }
      set
      {
        this.m_DraggedBy = value;
      }
    }

    public Corpse(Actor deadGuy, int hitPoints, int maxHitPoints, int corpseTurn, float rotation, float scale)
    {
      this.m_DeadGuy = deadGuy;
      this.m_Turn = corpseTurn;
      this.m_HitPoints = (float) hitPoints;
      this.m_MaxHitPoints = maxHitPoints;
      this.m_Rotation = rotation;
      this.m_Scale = scale;
      this.m_DraggedBy = (Actor) null;
    }
  }
}
