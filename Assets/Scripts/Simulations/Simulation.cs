using Assets.Collections;
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
        private static PriorityQueue<Event, float> eventQueue = new();
        static Dictionary<System.Type, Stack<Event>> eventPools = new Dictionary<System.Type, Stack<Event>>();

        // 优化，避免频繁创建和销毁事件对象
        static public T New<T>() where T : Event, new()
        {
            Stack<Event> pool;
            if (!eventPools.TryGetValue(typeof(T), out pool))
            {
                pool = new Stack<Event>(4);
                pool.Push(new T());
                eventPools[typeof(T)] = pool;
            }
            if (pool.Count > 0)
                return (T)pool.Pop();
            else
                return new T();
        }

        static public T Schedule<T>(float tick) where T : Event, new()
        {
            T ev = New<T>();
            ev.tick = Time.time + tick;
            eventQueue.Enqueue(ev, ev.tick);
            return ev;
        }

        static public T Reschedule<T>(T ev, float tick) where T : Event, new()
        {
            ev.tick = Time.time + tick;
            eventQueue.Enqueue(ev, ev.tick);
            return ev;
        }

        static public int Tick()
        {
            var time = Time.time;
            var executedEventCount = 0;
            while (eventQueue.Count > 0 && eventQueue.Peek().tick <= time)
            {
                var ev = eventQueue.Dequeue();
                var tick = ev.tick;
                ev.ExecuteEvent();
                if (ev.tick > tick)
                {
                    //重新调度了自己，说明需要在未来再次执行，所以不放回对象池
                }
                else
                {
                    // Debug.Log($"<color=green>{ev.tick} {ev.GetType().Name}</color>");
                    ev.Cleanup();
                    try
                    {
                        eventPools[ev.GetType()].Push(ev);
                    }
                    catch (KeyNotFoundException)
                    {
                        //不应该发生，说明忘了在New<T>中创建对象池了
                        Debug.LogError($"No Pool for: {ev.GetType()}");
                    }
                }
                executedEventCount++;
            }
            return eventQueue.Count;
        }

    }



}
