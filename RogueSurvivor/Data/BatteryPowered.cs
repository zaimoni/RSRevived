using System;

namespace djack.RogueSurvivor.Data
{
    internal interface BatteryPowered
    {
        int Batteries { get; set; }
        int MaxBatteries { get; }
        void Recharge() { Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, MaxBatteries / 8); }
    }
}