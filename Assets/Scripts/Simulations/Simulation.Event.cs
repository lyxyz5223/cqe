using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Simulations
{

    public partial class Simulation
    {
        public abstract class Event : System.IComparable<Event>
        {
            internal float tick;

            public int CompareTo(Event other)
            {
                return tick.CompareTo(other.tick);
            }

            public abstract void Execute();

            public virtual bool Precondition() => true;

            internal virtual void ExecuteEvent()
            {
                if (Precondition())
                    Execute();
            }

            internal virtual void Cleanup()
            {

            }
        }

        public abstract class Event<T> : Event where T : Event<T>
        {
            public static System.Action<T> OnExecute;

            internal override void ExecuteEvent()
            {
                if (Precondition())
                {
                    Execute();
                    OnExecute?.Invoke((T)this);
                }
            }
        }

        public class DelayedEvent : Event
        {
            public System.Action OnExecute;
            public override void Execute()
            {
                OnExecute?.Invoke();
            }
        }
        public class DelayedEvent<T> : Event
        {
            public T Params { get; set; }
            public System.Action<T> OnExecute;
            public override void Execute()
            {
                OnExecute?.Invoke(Params);
            }
        }
    }
}
