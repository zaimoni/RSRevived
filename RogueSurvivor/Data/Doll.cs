// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Doll
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  // while we *could* lift this into the Actor class, isolating screen UI from game data is technically reasonable.
  [Serializable]
  public readonly struct Doll
  {
    private readonly List<string>?[]? m_Decorations;  // XXX only valid if the Model imageID is null

    public Doll(ActorModel model)
    {
      m_Decorations = (string.IsNullOrEmpty(model.ImageID) ? new List<string>[(int) DollPart._COUNT] : null);
    }

    public List<string>? GetDecorations(DollPart part) => m_Decorations?[(int) part];

    public void AddDecoration(DollPart part, string imageID)
    {
#if DEBUG
      if (null == m_Decorations) throw new ArgumentNullException(nameof(m_Decorations));
#endif
      (GetDecorations(part) ?? (m_Decorations[(int) part] = new(1))).Add(imageID);
    }

    public void RemoveDecoration(string imageID)
    {
      for (DollPart index = DollPart.NONE; index < DollPart._COUNT; ++index) {
        var x = GetDecorations(index);
        if (null != x && x.Remove(imageID) && 0 >= x.Count) {
          m_Decorations![(int)index] = null;
          break;
        }
      }
    }

    public void RemoveAllDecorations()
    {
#if DEBUG
      if (null == m_Decorations) throw new ArgumentNullException(nameof(m_Decorations));
#endif
      for (DollPart index = DollPart.NONE; index < DollPart._COUNT; ++index)
        m_Decorations[(int)index] = null;
    }

    // JSON: an object of body part to image IDs.  Our owner writes us only when HasDecorations.
    public bool HasDecorations {
      get {
        if (null != m_Decorations) foreach (var x in m_Decorations) if (null != x && 0 < x.Count) return true;
        return false;
      }
    }

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      if (null != m_Decorations) {
        for (DollPart index = DollPart.NONE; index < DollPart._COUNT; ++index) {
          var x = m_Decorations[(int)index];
          if (null != x && 0 < x.Count) writer.WriteArray(index.ToString(), x, options);
        }
      }
      writer.WriteEndObject();
    }

    public Doll(ActorModel model, JsonElement src) : this(model)
    {
      if (null == m_Decorations) throw new JsonException(model.ID.ToString() + " is not drawn from parts: it cannot have a Doll");
      foreach (var x in src.EnumerateObject()) {
        if (char.IsDigit(x.Name, 0) || !Enum.TryParse<DollPart>(x.Name, out var part) || 0 > (int)part || DollPart._COUNT <= part) throw new JsonException("unrecognized doll part: " + x.Name);
        foreach (var imageID in x.Value.EnumerateArray()) AddDecoration(part, imageID.GetString() ?? throw new JsonException("null doll decoration"));
      }
    }
  }
}
