using Assets.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Player
{
    public abstract class PlayerEvent : Simulation.Event
    {
        public PlayerController Player { get; set; }

    }


    class InvincibleTimeEndEvent : PlayerEvent
    {
        public override void Execute()
        {
            Player.SetBlinking(false);
            Player.SetInvincible(false);
            //Player.GetComponent<Collider>().enabled = true;
        }
    }
}
