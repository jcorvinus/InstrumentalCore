using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class ConeSignifier : MonoBehaviour
    {
        [SerializeField] MeshRenderer innerCone;
        [SerializeField] MeshRenderer outerCone;

        private float scale; // between 0 and 1
        private bool isActive;

        public float Scale { get { return scale; } set { scale = value; } }
        public bool IsActive { get { return isActive; } set { isActive = value; } }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            innerCone.transform.localScale = Vector3.one * scale;
            outerCone.enabled = IsActive;
        }
    }
}