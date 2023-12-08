using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine.UI;

using CatchCo;

using Instrumental.Controls;

namespace VRKeyboard
{
    /// <summary>
    /// physical virtual keyboard
    /// </summary>
    public class PhysKeyboard : Keyboard
    {
		[Header("Scaling Variables")]
		[SerializeField] AnimationCurve xScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField] AnimationCurve yScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField] AnimationCurve zScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        Coroutine showCoroutine;
        Coroutine hideCoroutine;

        public Text CharacterPrefabText; // duplicate this for each button, anchor it to the button face.

        public override void Show()
        {
			gameObject.SetActive(true);
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            showCoroutine = StartCoroutine(ShowCoroutine());
        }

		void SetButtonEnable(KeyInfo key, bool enable)
		{
			key.Button.gameObject.SetActive(enable);
			key.FaceIcon.gameObject.SetActive(enable);
		}

		void SetButtonRowEnable(KeyRow row, bool enable)
		{
			foreach(KeyInfo key in row.KeyInfo)
			{
				SetButtonEnable(key, enable);
			}
		}

		public void LookAtPoint(Vector3 point, Vector3 up)
		{
			transform.LookAt(point, up);
			transform.Rotate(new Vector3(0, 180, 0), Space.Self);
			transform.Rotate(new Vector3(-15, 0, 0), Space.Self);
		}

		IEnumerator ShowCoroutine()
        {
			textInput.enabled = false;

			textInput.gameObject.transform.parent.gameObject.SetActive(true);

			// enable all of our components!
			for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++) SetButtonRowEnable(rows[rowIndex], true);
			SetButtonEnable(returnButton, true);
			SetButtonEnable(caseButton, true);
			SetButtonEnable(symbolButton, true);
			SetButtonEnable(backspaceButton, true);

			float time = 0;
			float duration = 0.25f;

			while(time < duration)
			{
				float tValue = Mathf.InverseLerp(0, duration, time);

				Vector3 newScale = new Vector3(
					Mathf.LerpUnclamped(0, 1, xScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, yScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, zScaleCurve.Evaluate(tValue)));

				transform.localScale = newScale;

				time += Time.deltaTime;
				yield return null;
			}

			transform.localScale = Vector3.one;

			yield return null;
			textInput.enabled = true;

            visible = true;

			/*textInput.text = "";
			textInput.ActivateInputField();*/ // can't focus input field because of this.

			yield break;
        }

		protected override KeyInfo ButtonInit(KeyInfo button, bool textFace)
		{
			KeyInfo baseInfo = base.ButtonInit(button, textFace);

			if (textFace)
			{
				Transform buttonFace = baseInfo.FButton.ButtonFace;
				GameObject newTextObject = GameObject.Instantiate(CharacterPrefabText.gameObject, CharacterPrefabText.transform.parent);

				TransformAnchor textAnchor = newTextObject.gameObject.AddComponent<TransformAnchor>();
				textAnchor.Offset = new Vector3(0, 0.012f, 0);
				textAnchor.Anchor = buttonFace;
				textAnchor.AnchorForward = Vector3.down;
				textAnchor.AnchorUp = Vector3.forward;

				baseInfo.Button.CharacterText = newTextObject.GetComponent<Text>();

				baseInfo.FaceIcon = newTextObject;
			}

			return baseInfo;
		}

        public override void Hide()
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            hideCoroutine = StartCoroutine(HideCoroutine());
        }

        IEnumerator HideCoroutine()
        {
			float time = 0;
			float duration = 0.25f;

			while (time < duration)
			{
				float tValue = 1 - Mathf.InverseLerp(0, duration, time);

				Vector3 newScale = new Vector3(
					Mathf.LerpUnclamped(0, 1, xScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, yScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, zScaleCurve.Evaluate(tValue)));

				transform.localScale = newScale;

				time += Time.deltaTime;
				yield return null;
			}

			transform.localScale = Vector3.zero;

			for(int rowIndex=0; rowIndex < rows.Length; rowIndex++)
			{
				SetButtonRowEnable(rows[rowIndex], false);
			}
			SetButtonEnable(returnButton, false);
			SetButtonEnable(caseButton, false);
			SetButtonEnable(symbolButton, false);
			SetButtonEnable(backspaceButton, false);

			visible = false;

            textInput.gameObject.transform.parent.gameObject.SetActive(false);

            yield break;
        }
    }
}