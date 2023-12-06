// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal readonly struct UniqueItem
  {
    public readonly Item TheItem;
    public readonly bool IsSpawned;

    public UniqueItem(Item src, bool spawn=true)
    {
      TheItem = src;
      IsSpawned = spawn;
    }
  }
}

namespace Zaimoni.Serialization {

    public partial interface ISave
    {
        // handler must save to the target location field, or else
        internal static void Load(DecodeObjects decode, ref UniqueItem dest, Action<Item, bool> handler)
        {
            byte is_spawned = 0;
            Formatter.Deserialize(decode.src, ref is_spawned);

            ulong code;
            var stage_item = decode.Load<Item>(out code);
            if (null != stage_item) {
                dest = new(stage_item, 0 != is_spawned);
                return;
            }
            if (0 >= code) throw new ArgumentNullException("src.TheItem");
            decode.Schedule(code, (o) => {
                if (o is Item it) handler(it, 0 != is_spawned); // local copy doesn't work for structs
                else throw new ArgumentNullException("src.TheItem");
            });
        }

        internal static void Save(Zaimoni.Serialization.EncodeObjects encode, in UniqueItem src)
        {
            byte is_spawned = (byte)(src.IsSpawned ? 1 : 0);
            Formatter.Serialize(encode.dest, is_spawned);

            var code = encode.Saving(src.TheItem); // obligatory, in spite of type prefix/suffix
            if (0 < code) Formatter.SerializeObjCode(encode.dest, code);
            else throw new ArgumentNullException("src.TheItem");
        }
    }
}

