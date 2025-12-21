using System;
using System.Threading;

namespace djack.RogueSurvivor.Data._Item
{
    [Serializable]
    public sealed class PrimedExplosive : Engine.Items.ItemExplosive
    {
        private int m_FuseTimeLeft;

        public int FuseTimeLeft { get { return m_FuseTimeLeft; } }

        public new Data.Model.PrimedExplosive Model { get { return base.Model.Primed; } }
        public Data.Model.Explosive Unprimed { get { return base.Model; } }

        public PrimedExplosive(Data.Model.Explosive model, int fuse_left) : base(model) {
            m_FuseTimeLeft = fuse_left;
        }
        public PrimedExplosive(Data.Model.Explosive model) : this(model, model.FuseDelay) { }

        public void Cook() { Interlocked.Exchange(ref m_FuseTimeLeft, 0); }    // detonate immediately
        public bool Expire() { return 0 >= Interlocked.Decrement(ref m_FuseTimeLeft); }
        static public bool IsExpired(PrimedExplosive e) { return 0 >= e.m_FuseTimeLeft; }
        public override Data.Item_s toStruct() { return new Data.Item_s(ModelID, m_FuseTimeLeft); }

    }
}
