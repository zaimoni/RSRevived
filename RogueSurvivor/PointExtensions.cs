// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.PointExtensions
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Drawing;

namespace djack.RogueSurvivor
{
  public static class PointExtensions
  {
    public static Point Add(this Point pt, int x, int y)
    {
      return new Point(pt.X + x, pt.Y + y);
    }

    public static Point Add(this Point pt, Point other)
    {
      return new Point(pt.X + other.X, pt.Y + other.Y);
    }
  }
}
