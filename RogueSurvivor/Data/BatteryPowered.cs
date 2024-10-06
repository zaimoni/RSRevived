using System;

namespace djack.RogueSurvivor.Data
{
    public interface BatteryPowered
    {
        int Batteries { get; set; }
        int MaxBatteries { get; }
        int RechargeRate { get { return Math.Max(WorldTime.TURNS_PER_HOUR, MaxBatteries / 8); } }
        void Recharge() { Batteries += RechargeRate; }

        bool AugmentsSenses(Actor a);
    }
}