using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BirdSort
{
    public class RotateObject : MonoBehaviour {

        [SerializeField] float speed = 1;
        private bool wasRoate;

        private void OnDisable()
        {
            wasRoate = true;
        }
        private void OnEnable()
        {
            wasRoate = false;
        }
        void Update () 
        {
            if(!wasRoate)
            {
                if (Time.timeScale == 0) return;
                this.transform.Rotate(new Vector3(0, 0, 1) * speed);
            }
        
        }
    }
}
