using Assets.Player;
using Assets.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Road
{
    public abstract class RoadEvent : Simulation.Event
    {
        public RoadManager roadManager = null;
        public GameObject gameObject = null;
    }

    public class ExtendRoadEvent : RoadEvent
    {
        public override void Execute()
        {
            if (roadManager == null)
                return;
            roadManager.Extend();
        }
    }

    public class DestroyRoadEvent : RoadEvent {
        public override void Execute()
        {
            if (roadManager == null)
                return;
            roadManager.DestroyRoadSegment(gameObject);
        }
    }


}