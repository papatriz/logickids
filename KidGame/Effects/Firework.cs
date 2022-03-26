using Orbox.Utils;
using System;
using UnityEngine;

namespace KidGame.Effects
{
    public class Firework : IEffect
    {
        private readonly IResourceManager ResourceManager;

        private GameObject Firework1;
        private GameObject Firework2;

        public Firework(IResourceManager rm)
        {
            ResourceManager = rm;
        }


        public void Play()
        {
            var values = Enum.GetValues(typeof(Fireworks));

            int index1 = UnityEngine.Random.Range(0, values.Length);
            int index2 = UnityEngine.Random.Range(0, values.Length);

            var firework1 = (Fireworks)values.GetValue(index1);
            var firework2 = (Fireworks)values.GetValue(index2);

            Firework1 = ResourceManager.GetFromPool(firework1);
            Firework2 = ResourceManager.GetFromPool(firework2);
        }

        public void Stop()
        {
            Firework1.SetActive(false);
            Firework2.SetActive(false);

            Firework1 = null;
            Firework2 = null;
        }
    }
}