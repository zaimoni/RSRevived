// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Verb
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  sealed internal class Verb
  {
    public readonly string YouForm;   // 2nd person singular; in English typically also 3rd person plural
    public readonly string HeForm;    // 3rd person singular

    public Verb(string youForm, string heForm)
    {
      YouForm = youForm;
      HeForm = heForm;
    }

    public Verb(string youForm)
      : this(youForm, youForm + "s")
    {
    }
  }
}
