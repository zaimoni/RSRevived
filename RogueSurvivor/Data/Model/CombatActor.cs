using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace djack.RogueSurvivor.Data.Model
{
    public record struct CombatActor : ILocation
    {
        public readonly Actor who;
        private int m_HitPoints;
        private int m_StaminaPoints;
        private Location m_Location;
        private int m_ActionPoints;
        private WorldTime time;
        private sbyte _recoil = 0;

        public sbyte Recoil { get => _recoil; }

        public CombatActor(Actor src) {
            who = src;
            m_HitPoints = src.HitPoints;
            m_StaminaPoints = src.StaminaPoints;
            m_Location = src.Location;
            m_ActionPoints = src.ActionPoints;
            time = src.Location.Map.LocalTime;
            _recoil = (who.Controller as Gameplay.AI.ObjectiveAI)?.Recoil ?? 0;

            normalizeAP();
        }

        public Location Location
        {
            get => m_Location;
            set { m_Location = value; }
        }

        private void normalizeAP() {
            while (0 >= m_ActionPoints) {
                m_ActionPoints += who.Speed;
                time.TurnCounter++;
            }
        }

        public static int CompareAP(CombatActor lhs, CombatActor rhs) {
            var code = lhs.time.TurnCounter.CompareTo(rhs.time.TurnCounter);
            if (code != 0) return code;
            if (lhs.who.Location.Map != rhs.who.Location.Map) return District.IsBefore(lhs.who.Location.Map, rhs.who.Location.Map) ? -1 : 1;
            foreach (var a in lhs.Location.Map.Actors) {
                if (a == lhs.who) return -1;
                if (a == rhs.who) return 1;
            }
            return lhs.m_ActionPoints.CompareTo(rhs.m_ActionPoints);
        }

        public override string ToString() { return who.ToString()+"@"+m_Location.ToString()+"; "+time.TurnCounter.ToString()+":"+m_ActionPoints.ToString()+"; " +m_HitPoints.ToString() + "/" + m_StaminaPoints.ToString() + "/" + _recoil.ToString(); }

    }
}

