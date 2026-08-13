using Assets.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class CheckAndExtendTerrainEvent : Simulation.Event
    {
        public GameObject RoadSegment = null;
        public override bool Precondition()
        {
            return TerrainManager.Instance.ShouldExtend();
        }


        public override void Execute()
        {
            TerrainManager.Instance.ExtendOne(RoadSegment);
        }
    }


    public class CheckAndRemoveTerrainEvent : Simulation.Event
    {
        public override bool Precondition()
        {
            return TerrainManager.Instance.ShouldRemove();
        }
        public override void Execute()
        {
            TerrainManager.Instance.RemoveOne();
        }
    }

}
