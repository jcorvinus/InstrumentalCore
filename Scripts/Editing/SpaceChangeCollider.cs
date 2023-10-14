using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;

namespace Instrumental.Editing
{ 
    public class SpaceChangeCollider : MonoBehaviour
    {
        int layerMask;
        SpaceItem changer;

        private void Awake()
        {
            changer = GetComponentInParent<SpaceItem>();
            layerMask = LayerMask.NameToLayer("SpaceZone");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == layerMask)
            {
                TransformSpace newSpace = other.GetComponent<TransformSpace>();

                if (newSpace)
                {
                    changer.ChangeSpaces(newSpace);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == layerMask)
            {
                TransformSpace spaceCandidate = other.GetComponent<TransformSpace>();

                if (spaceCandidate != null /*&& rendererCandidate != GlobalSpace.Instance.GraphicRenderer*/)
                {
                    changer.ChangeSpaces(null);
                }
            }
        }
    }
}