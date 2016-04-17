// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Verb
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Verb
  {
    public string YouForm { get; protected set; }

    public string HeForm { get; protected set; }

    public Verb(string youForm, string heForm)
    {
      this.YouForm = youForm;
      this.HeForm = heForm;
    }

    public Verb(string youForm)
      : this(youForm, youForm + "s")
    {
    }
  }
}
