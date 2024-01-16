using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;
using Instrumental.Editing;
using Instrumental.Interaction;
using Instrumental.Interaction.Slottables;

namespace Instrumental.Controls
{
    public enum ControlMode
    {
        /// <summary>
        /// UI is live, recieving user manipulation and operating
        /// on data. Usser cannot alter placement of controls.
        /// </summary>
        Runtime,
        /// <summary>
        /// Design mode. User can place controls but not activate them.
        /// </summary>
        Design,
		/// <summary>
		/// Control belongs to a palette menu - user will grab and pull to 
		/// instantiate.
		/// </summary>
		Design_Palette
	}

	public abstract class UIControl : MonoBehaviour
	{
		[SerializeField] ControlMode mode = ControlMode.Design_Palette;

		// check to see if we're a member of a panel
		// should we emit events for property changed so that
		// the panel can update the schema data?

		private Panel attachedPanel; // this can be null, we don't have to be attached to a panel,
									 // but it is significant if we are.

		protected SlottableItem anchorable;
		protected InteractiveItem placementInteraction;
		private Rigidbody placementRigidbody;
		protected GameObject editSoundEmitterGameObject;
		protected AudioSource placementGrabSource;
		protected AudioSource placementDropSource;
		protected SpaceItem spaceItem;
		protected SpaceChangeCollider spaceChangeCollider;

		protected string _name = "";
		private bool isPrecisionPlacement = false;
		private bool placementGraspEventsSubscribed = false;
		private bool spaceChangeEventsSubscribed = false;

		[Header("Debug Vars")]
		[SerializeField]
		[Range(0, 3)]
		int orientationPreviewID;

		protected void EnsureGraspableExists()
		{
			placementInteraction = GetComponent<InteractiveItem>();
			if (!placementInteraction) placementInteraction = gameObject.AddComponent<InteractiveItem>();

			placementRigidbody = GetComponent<Rigidbody>();
			if (!placementRigidbody) placementRigidbody = gameObject.AddComponent<Rigidbody>();
		}

		protected void ClearAnyGraspable()
		{
			InteractiveItem placementItem = GetComponent<InteractiveItem>();
			if (placementItem)
			{
				if (Application.isPlaying)
				{
					Destroy(placementItem);
				}
				else
				{
#if UNITY_EDITOR
					UnityEditor.EditorApplication.delayCall += () =>
					{
						UnityEditor.Undo.DestroyObjectImmediate(placementItem);
					};
#endif
				}
			}
		}

		protected void EnsureSlottableExists()
		{
			anchorable = GetComponent<SlottableItem>();
			if (!anchorable) anchorable = gameObject.AddComponent<SlottableItem>();
		}

		protected void ClearSlottable()
		{
			SlottableItem slottableitem = GetComponent<SlottableItem>();
			if (slottableitem)
			{
				if (Application.isPlaying)
				{
					Destroy(slottableitem);
				}
				else
				{
#if UNITY_EDITOR
					UnityEditor.EditorApplication.delayCall += () =>
					{
						UnityEditor.Undo.DestroyObjectImmediate(slottableitem);
					};
#endif
				}
			}
		}

		protected void EnsureSpaceItemExists()
		{
			spaceItem = GetComponent<SpaceItem>();
			if (!spaceItem) spaceItem = gameObject.AddComponent<SpaceItem>();
		}

		protected void EnsureSpaceChangeColliderExists(Transform target)
		{
			SpaceChangeCollider candidateCollider = target.GetComponent<SpaceChangeCollider>();

			if (!candidateCollider)
			{
				candidateCollider = target.gameObject.AddComponent<SpaceChangeCollider>();
			}

			// I think we might need to check to see if the existing space change collider is
			// different from the candidate one we find.
			// This would be un-intended usage and I think we should alert and be like 'yo wtf' to the user
			if (spaceChangeCollider && spaceChangeCollider != candidateCollider)
			{
				Debug.LogError(string.Format("Existing space change collider was not the same as one found. Control {0}, previous collider: {1} new collider: {2}",
					gameObject.name, spaceChangeCollider.name, candidateCollider.name));
			}

			spaceChangeCollider = candidateCollider;
		}

		protected void ClearSpaceChangeCollider()
		{
			if(spaceChangeCollider)
			{
				if(Application.isPlaying)
				{
					Destroy(spaceChangeCollider);
				}
				else
				{
#if UNITY_EDITOR
					UnityEditor.EditorApplication.delayCall += () =>
					{
						UnityEditor.Undo.DestroyObjectImmediate(spaceChangeCollider);
					};
#endif
				}
			}
		}

		private void SetupDesignMode()
		{
			// if InteractionBehavior doesn't exist,
			// create it. AnchorableBehavior not necessary?
			// delete it if it exists?
			EnsureGraspableExists();
			placementRigidbody.isKinematic = true;

			// if we're in design mode or design palette mode,
			// create our edit sound emitters
			CreateEditSoundEmitters();
			if(spaceChangeEventsSubscribed) spaceItem.SpaceChanged += SpaceChanger_SpaceChanged;

			if (!placementGraspEventsSubscribed) SubscribePlacementGraspEvents();
		}

		private void SetupPaletteMode()
		{
			// if anchorable and InteractionBehavior don't exist,
			// create them
			EnsureGraspableExists();
			EnsureSlottableExists();
			placementRigidbody.isKinematic = false;

			// if we're in design mode or design palette mode,
			// create our edit sound emitters
			CreateEditSoundEmitters();
			if (!spaceChangeEventsSubscribed) spaceItem.SpaceChanged += SpaceChanger_SpaceChanged;

			if (!placementGraspEventsSubscribed) SubscribePlacementGraspEvents();
		}

		protected virtual void Awake()
		{
			EnsureSpaceItemExists();

			switch (mode)
			{
				case ControlMode.Runtime:
					// look for panel in parent
					// todo: make sure this control gets added to the panel's control list.
					attachedPanel = GetComponentInParent<Panel>();
					break;

				case ControlMode.Design:
					SetupDesignMode();
					break;

				case ControlMode.Design_Palette:
					SetupPaletteMode();
					break;

				default:
					break;
			}
		}

		public virtual void SwitchMode(ControlMode newMode)
		{
			ControlMode oldMode = mode;

			switch (oldMode)
			{
				case ControlMode.Runtime:
					attachedPanel.RemoveUIControl(this);
					attachedPanel = null;

					if (newMode == ControlMode.Design)
					{
						SetupDesignMode();
					}
					else if (newMode == ControlMode.Design_Palette)
					{
						SetupPaletteMode();
					}
					break;

				case ControlMode.Design:
					if(newMode == ControlMode.Runtime)
					{
						// remove old design mode components
						ClearAnyGraspable();
						ClearSlottable();
						ClearEditSoundEmitters();

						// todo: make sure this control gets added to the panel's control list.
						attachedPanel = GetComponentInParent<Panel>();

						if (placementGraspEventsSubscribed) UnsubscribePlacementGraspEvents();
					}
					else if (newMode == ControlMode.Design_Palette)
					{
						SetupPaletteMode();
					}
					break;

				case ControlMode.Design_Palette:
					if(newMode == ControlMode.Runtime)
					{
						ClearAnyGraspable();
						ClearSlottable();
						ClearEditSoundEmitters();

						// todo: make sure this control gets added to the panel's control list.
						attachedPanel = GetComponentInParent<Panel>();

						if (placementGraspEventsSubscribed) UnsubscribePlacementGraspEvents();
					}
					else if (newMode == ControlMode.Design)
					{
						ClearSlottable();
					}
					break;

				default:
					break;
			}

			mode = newMode;
		}

		string NameOrNull(Object target)
		{
			if (!target) return "null";
			else return target.name;
		}

		private void SpaceChanger_SpaceChanged(SpaceItem sender, TransformSpace oldSpace,
			TransformSpace newSpace)
		{
			Debug.Log(string.Format("Control {0} changed spaces from {1} to {2}",
				sender.name, NameOrNull(oldSpace), NameOrNull(newSpace)));

			if (attachedPanel)
			{
				// we're leaving our current panel, tell it to remove us from its recordkeeping system
				attachedPanel.RemoveUIControl(this);
				attachedPanel = null;
			}

			if (newSpace)
			{
				Panel panelCandidate = newSpace.GetComponent<Panel>();
				if (panelCandidate)
				{
					attachedPanel = panelCandidate;

					// we're joining this new panel, tell it to add us to its recordkeeping system.
					string newName = "";
					if (attachedPanel.MustRenameControl(this._name, out newName))
					{
						this._name = newName;
						attachedPanel.AddUIControl(this);
					}
				}

				transform.SetParent(newSpace.transform);
			}
			else
			{
				transform.SetParent(GlobalSpace.Instance.transform);
			}
		}

		private void CreateEditSoundEmitters()
		{
			if (!editSoundEmitterGameObject)
			{
				editSoundEmitterGameObject = new GameObject("EditSoundEmitters", new System.Type[] { typeof(AudioSource),
					typeof(AudioSource)});

				editSoundEmitterGameObject.transform.SetParent(transform);
				editSoundEmitterGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

				AudioSource[] audioSourceComponents = editSoundEmitterGameObject.GetComponents<AudioSource>();
				placementGrabSource = audioSourceComponents[0];
				placementDropSource = audioSourceComponents[1];

				placementGrabSource.playOnAwake = false;
				placementGrabSource.spatialBlend = 1;
				placementGrabSource.spatialize = true;
				placementGrabSource.Stop();
				placementGrabSource.clip = Instrumental.Space.GlobalSpace.Instance.UICommon.GrabClip;
				placementGrabSource.outputAudioMixerGroup = Instrumental.Space.GlobalSpace.Instance.UICommon.MasterGroup;
				placementGrabSource.minDistance = 0.1f;
				placementGrabSource.maxDistance = 2f;

				placementDropSource.playOnAwake = false;
				placementDropSource.spatialBlend = 1;
				placementDropSource.spatialize = true;
				placementDropSource.Stop();
				placementDropSource.clip = Instrumental.Space.GlobalSpace.Instance.UICommon.ItemPlaceClip;
				placementDropSource.outputAudioMixerGroup = Instrumental.Space.GlobalSpace.Instance.UICommon.MasterGroup;
				placementDropSource.minDistance = 0.1f;
				placementDropSource.maxDistance = 2f;
			}
		}

		private void ClearEditSoundEmitters()
		{
			if (editSoundEmitterGameObject)
			{
				placementGrabSource = null;
				placementDropSource = null;

				GameObject.Destroy(editSoundEmitterGameObject);
			}
		}

		void SubscribePlacementGraspEvents()
		{
			placementInteraction.OnGrasped += PlacementGraspBegin;
			//placementInteraction.allowMultiGrasp = true;
			placementInteraction.OnGraspMoved += DoEditorGraspMovement;
			placementInteraction.OnUngrasped += PlacementGraspEnd;

			placementGraspEventsSubscribed = true;
		}

		void UnsubscribePlacementGraspEvents()
		{
			placementInteraction.OnGrasped -= PlacementGraspBegin;
			placementInteraction.OnGraspMoved -= DoEditorGraspMovement;
			placementInteraction.OnUngrasped -= PlacementGraspEnd;

			placementGraspEventsSubscribed = false;
		}

        protected virtual void Start()
        {

        }

        void PlacementGraspBegin(InteractiveItem sender)
        {
            placementGrabSource.Play();
            //if (placementInteraction.graspingControllers.Count > 1) isPrecisionPlacement = true;
        }

        void PlacementGraspEnd(InteractiveItem sender)
        {
            /*if (placementInteraction.graspingControllers.Count == 0)
            {
                if (attachedPanel) placementDropSource.Play();

                isPrecisionPlacement = false;
                placementInteraction.rigidbody.isKinematic = true;
            }*/

            if(!isPrecisionPlacement)
            {
                if(attachedPanel) InstantAngleSnap();
            }
        }

        [System.Serializable]
        struct AngleSnap
        {
            public Quaternion orientation;
            public float angleDist;
        }

        // up down, left, right angle snaps
        AngleSnap[] angleSnap = new AngleSnap[4];

        void InstantAngleSnap()
        {
            angleSnap[0].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
            angleSnap[0].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[0].orientation);

            angleSnap[1].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(270, Vector3.forward);
            angleSnap[1].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[1].orientation);

            angleSnap[2].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(180, Vector3.forward);
            angleSnap[2].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[2].orientation);

            angleSnap[3].orientation = attachedPanel.transform.rotation;
            angleSnap[3].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[3].orientation);

            AngleSnap closestSnap = angleSnap.First(item => item.angleDist == angleSnap.Min(subItem => subItem.angleDist));

            //placementInteraction.rigidbody.MoveRotation(Quaternion.Slerp(placementRigidbody.rotation, closestSnap.orientation, Time.deltaTime * 6f));
            //placementInteraction.rigidbody.MoveRotation(closestSnap.orientation);
        }

        void SlerpAngleSnap()
        {
            angleSnap[0].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
            angleSnap[0].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[0].orientation);

            angleSnap[1].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(270, Vector3.forward);
            angleSnap[1].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[1].orientation);

            angleSnap[2].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(180, Vector3.forward);
            angleSnap[2].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[2].orientation);

            angleSnap[3].orientation = attachedPanel.transform.rotation;
            angleSnap[3].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[3].orientation);

            AngleSnap closestSnap = angleSnap.First(item => item.angleDist == angleSnap.Min(subItem => subItem.angleDist));

            //placementInteraction.rigidbody.rotation = (Quaternion.Slerp(placementRigidbody.rotation, closestSnap.orientation, 0.5f));
            //placementInteraction.rigidbody.MoveRotation(closestSnap.orientation);
        }

        void DoEditorGraspMovement(InteractiveItem sender/*Vector3 preSolvedPos, Quaternion preSolvedRot,
            Vector3 solvedPos, Quaternion solvedRot*/)
        {
            if (attachedPanel != null)
            {
                // todo: add placement assistance and precision movement here
                if(!isPrecisionPlacement)
                {
                    // angle snap
                    // target quaternion should be closest 45 degree angle
                    SlerpAngleSnap();

                    // check for alignment to other bounds

                    // snap to surface
                }
                else
                {
                    // precision placement mode
                }
            }
        }

        /// <summary>Call this when instantiating a control from schema values.</summary>
        /// <param name="controlSchema">Schema to reference when initializing.</param>
        public abstract void SetSchema(Schema.ControlSchema controlSchema);
        /// <summary>Gets schema data for the current control.</summary>
        /// <returns>Schema data for current control.</returns>
        public abstract Schema.ControlSchema GetSchema(); 

        public ControlMode Mode { get { return mode; } }
        public string Name { get { return _name; } }
        public abstract ControlType GetControlType();

        private void OnDrawGizmos()
        {
            if(attachedPanel)
            {
                Quaternion orientation = Quaternion.identity;

                orientation = angleSnap[orientationPreviewID].orientation;

                Vector3 forward, up, right;

                forward = orientation * Vector3.forward;
                up = orientation * Vector3.up;
                right = orientation * Vector3.right;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + up * 0.2f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + forward * 0.2f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + right * 0.2f);
            }
        }
    }
}