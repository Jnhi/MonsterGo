using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class CoroutineManager : MonoSingleton<CoroutineManager>
    {

        private List<Coroutine> activeCoroutines;

        protected override void Init()
        {
            base.Init();
            activeCoroutines = new List<Coroutine>();
        }


        public void StartCustomCoroutine(IEnumerator routine)
        {

            if (Instance != null)
            {
                Coroutine coroutine = Instance.StartCoroutine(routine);
                Instance.activeCoroutines.Add(coroutine);
            }
        }

        public void StopAllCustomCoroutines()
        {
            if (Instance != null)
            {
                foreach (Coroutine coroutine in Instance.activeCoroutines)
                {
                    Instance.StopCoroutine(coroutine);
                }
                Instance.activeCoroutines.Clear();
            }
        }

        public IEnumerator WaitForAllCoroutines(List<IEnumerator> yieldables)
        {
            var coroutines = new Coroutine[yieldables.Count];

            for (int i = 0; i < coroutines.Length; i++)
            {
                coroutines[i] = Instance.StartCoroutine(yieldables[i]);
            }

            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
                Instance.StopCoroutine(coroutine);

            }
        }

    }
}


