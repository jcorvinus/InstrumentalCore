using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;
using Instrumental.Interaction.Constraints;

namespace Instrumental.Controls
{
    public class PanelHandle : MonoBehaviour
    {
        public delegate void HandleHandler(PanelHandle handle);
        public event HandleHandler OnHandleMoved;
        public event HandleHandler OnHandleGrasped;
        public event HandleHandler OnhandleUngrasped;

        public enum HandleType
        {
            UpperRail,
            LowerRail,
            LeftRail,
            RightRail,
            UpperLeftCorner,
            LowerLeftCorner,
            UpperRightCorner,
            LowerRightCorner
        }

        [SerializeField] HandleType type;         
        public HandleType Type { get { return type; } }
        public bool IsGrasped { get { return false; /*return interaction.isGrasped;*/ } }

        InteractiveItem interaction;
        PanelHandleConstraint constraint;
        Collider[] colliders;
        Transform model;
        Panel owningPanel;
        Vector3 startScale;

        private void Awake()
        {
            interaction = GetComponent<InteractiveItem>();
            model = transform.GetChild(0);
            colliders = GetComponentsInChildren<Collider>();
            constraint = gameObject.AddComponent<PanelHandleConstraint>();
        }

        // Use this for initialization
        void Start()
        {
            interaction.OnGrasped += (InteractiveItem sender) =>
            {
                if (OnHandleGrasped != null)
                {
                    OnHandleGrasped(this);
                }
            };

            interaction.OnUngrasped += (InteractiveItem sender) =>
            {
                if(OnhandleUngrasped != null)
                {
                    OnhandleUngrasped(this);
                }
            };

            interaction.OnGraspMoved += (InteractiveItem sender) =>
            {
                owningPanel.SetDimensionsForPanel(this);
            };

            startScale = model.transform.localScale;
        }

        public void SetPanel(Panel panel)
        {
            owningPanel = panel;
            foreach(Collider collider in colliders)
            {
                Physics.IgnoreCollision(collider, panel.PanelCollider);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetGrabbable(bool grabbable)
        {
            //interaction.ignoreGrasping = !grabbable;
            model.transform.localScale = (grabbable) ? startScale : startScale * 0.4f;
        }
    }
}