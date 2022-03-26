using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KidGame.UI
{
    public class WaitIndicator : MonoBehaviour
    {
        private const float SpinSpeed = 90f;

        void Update()
        {
            transform.Rotate(transform.forward, -SpinSpeed * Time.deltaTime);
        }
    }
}