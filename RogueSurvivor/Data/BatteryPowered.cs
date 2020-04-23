namespace djack.RogueSurvivor.Data
{
    internal interface BatteryPowered
    {
        int Batteries { get; set; }
        int MaxBatteries { get; }
        void Recharge();
    }
}