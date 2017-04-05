using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    internal abstract class ObjectiveAI : BaseAI
    {
        readonly protected List<Objective> Objectives = new List<Objective>();

    }
}
