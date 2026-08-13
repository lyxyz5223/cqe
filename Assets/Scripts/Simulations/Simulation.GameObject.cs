using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Simulations
{
    public class SimulationObject : MonoBehaviour
    {
        private static SimulationObject instance = null;
        public static SimulationObject Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SimulationObject>();
                    if (instance == null)
                    {
                        GameObject obj = new ("SimulationObject");
                        instance = obj.AddComponent<SimulationObject>();
                    }
                }
                return instance;
            }
        }

        private bool paused = true;

        private void OnEnable()
        {
            paused = false;
        }

        private void OnDisable()
        {
            paused = true;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {

        }

        private static float totalTime = 0; // 总时间，单位秒
        private static float timer = 0; // 计时器，单位秒，每当timer达到1秒时，timer重置为0

        void Update()
        {
            totalTime += Time.deltaTime;
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                timer = 0;
            }
            if (!paused)
            {
                Simulation.Tick();
            }
        }

    }
}
