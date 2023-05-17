using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class ConeSignifier : MonoBehaviour
    {
        [SerializeField] MeshRenderer innerCone;
        [SerializeField] MeshRenderer outerCone;
        [SerializeField] Color activeColor = Color.cyan;

        private float scale; // between 0 and 1

        public float Scale { get { return scale; } set { scale = value; } }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            innerCone.transform.localScale = Vector3.one * scale;

            bool isNearMax = scale > 0.99f;
            outerCone.enabled = !isNearMax; 
            outerCone.material.color = (isNearMax) ?
                activeColor : Color.white;
        }
    }
}