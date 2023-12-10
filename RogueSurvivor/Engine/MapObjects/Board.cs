// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.Board
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  internal sealed class Board : MapObject, Zaimoni.Serialization.ISerialize
    {
    public readonly string[] Text;

    public Board(string imageID, string[] text) : base(imageID) {
      Text = text;
    }

#region implement Zaimoni.Serialization.ISerialize
    protected Board(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        Zaimoni.Serialization.ISave.LinearLoad(decode, out Text);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        Zaimoni.Serialization.ISave.LinearSave(encode, Text);
    }
#endregion
  }
}
