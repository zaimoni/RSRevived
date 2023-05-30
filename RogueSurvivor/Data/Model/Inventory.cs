using djack.RogueSurvivor.Engine.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

namespace djack.RogueSurvivor.Data.Model
{
    // for mental modeling by ObjectiveAI
    internal class Inventory
    {
        private Item_s[]? m_Items = null;

        public Inventory(IEnumerable<Item_s>? src) {
            if (null != src) {
                List<Item_s> stage = new();
                foreach (var x in src) {
                    if (0 < x.QtyLike) stage.Add(x);
                    else if (0 != QtyLikeZero_ok(x.ModelID) && 0 == x.QtyLike) stage.Add(x);
                }
                m_Items = stage.ToArray();
            }
        }

        public Inventory(IEnumerable<Item>? src) {
            if (null != src) {
                List<Item_s> stage = new();
                foreach (var y in src) {
                    var x = y.toStruct();
                    if (0 <= x.QtyLike) stage.Add(x);
                    else if (0 != QtyLikeZero_ok(x.ModelID) && 0 == x.QtyLike) stage.Add(x);
                }
                m_Items = stage.ToArray();
            }
        }

        // value copy
        public Inventory(Inventory src) {
            if (null != src?.m_Items && 0 < src.m_Items.Length) {
                var stage = new Item_s[src.m_Items.Length];
                Array.Copy(src.m_Items, stage, src.m_Items.Length);
                m_Items = stage;
            }
        }

        public IEnumerable<Item_s>? Items { get { return m_Items; } }
        public bool IsEmpty { get { return null == m_Items || 0 == m_Items.Length; } }

        /// <returns>0: no; 1: ok; -1: discard on drop</returns>
        static public int QtyLikeZero_ok(Gameplay.Item_IDs ModelID) {
            var model = Gameplay.GameItems.From(ModelID);
            if (model is ItemRangedWeaponModel) return 1;
            // simulate is BatteryPowered test
            if (model is ItemLightModel) return -1;
            if (model is ItemTrackerModel) return -1;
            return 0;
        }

        public int? GetBestDestackable(ItemModel it)    // alpha10 equivalent: GetSmallestStackByModel.  XXX \todo rename for legibility?
        {
            int? ret = default;
            if (null != m_Items) {
                int ub = m_Items.Length;
                while (0 <= --ub) {
                    if (m_Items[ub].Model != it) continue;
                    if (null == ret || m_Items[ub].QtyLike < ret.Value) ret = ub;
                }
            }
            return ret;
        }

        public int? GetWorstDestackable(ItemModel it)
        {
            int? ret = default;
            if (null != m_Items) {
                int ub = m_Items.Length;
                while (0 <= --ub) {
                    if (m_Items[ub].Model != it) continue;
                    if (null == ret || m_Items[ub].QtyLike > ret.Value) ret = ub;
                }
            }
            return ret;
        }

        [Conditional("DEBUG")]
        private void LegalIndex(int n) {
            if (null == m_Items) throw new ArgumentNullException(nameof(m_Items));
            var ub = m_Items.Length;
            if (0 > n || ub <= n) throw new ArgumentOutOfRangeException(nameof(n));
        }

        private void RemoveAt(int n) {
            LegalIndex(n);
            var ub = m_Items!.Length;
            if (1 == ub) {
                m_Items = null;
                return;
            }
            var stage = new Item_s[ub - 1];
            if (0 < n) Array.ConstrainedCopy(m_Items, 0, stage, 0, n);
            if (n < ub - 1) Array.ConstrainedCopy(m_Items, n+1, stage, n, (ub - 1)-n);
            m_Items = stage;
        }

        public void Consume(int n)
        {
            LegalIndex(n);
            if (m_Items![n].Consume()) RemoveAt(n);
        }

        public void Consume(Gameplay.Item_IDs ModelID) {
            var target = GetBestDestackable(Gameplay.GameItems.From(ModelID));
            if (null == target)
#if DEBUG
                throw new ArgumentNullException(nameof(target))
#else
                return // fail-open in release mode
#endif
            ;
            Consume(target.Value);
        }

    }
}
