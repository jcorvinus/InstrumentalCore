using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Input
{
    [System.Serializable]
    public struct SampleData
    {
        public Vect3 Velocity;
        public Vect3 AngularVelocity;
    }

    public class VelocityEstimation : MonoBehaviour
    {
        SampleData[] sampleData; // this is a ring buffer of samples. Start at currentSampleIndex,
            // then walk backwards till you hit zero, then loop, and keep walking backwards until you
            // hit currentSampleIndex + 1. Remember to handle the case of sampleIndex == sampleData.length - 1.
            // that means just stop at 0

        Vect3 currentVelocityEstimation;
		Vect3 currentAngularEstimation;

        [SerializeField] int sampleWindow=10;
        int currentSampleIndex = 0;
        int currentSampleCount = 0;

        bool isEstimating = false;
        public bool IsEstimating { get { return isEstimating; } }
        public Vect3 Velocity { get { return currentVelocityEstimation; } }
        public Vect3 AngularVelocity { get { return currentAngularEstimation; } }

        // Start is called before the first frame update
        void Start()
        {
            sampleData = new SampleData[sampleWindow];
        }

        // Update is called once per frame
        void Update()
        {

        }

		private void LateUpdate()
		{
            if (isEstimating) DoEstimation();
		}

        void DoEstimation()
		{
			// get our starting values
			Vect3 velocity = Vect3.zero;
			Vect3 angularVelocity = Vect3.zero;

            // walk backwards through the ring buffer
            int scanIndex = currentSampleIndex;

            for(int walkIndex=0; walkIndex < currentSampleCount; walkIndex++)
			{
                if (scanIndex == 0) scanIndex = currentSampleCount - 1;

                velocity += sampleData[scanIndex].Velocity;
                velocity *= (1.0f / currentSampleCount);

                angularVelocity += sampleData[scanIndex].AngularVelocity;
                angularVelocity *= (1.0f / currentSampleCount);

                scanIndex--;
			}

            currentVelocityEstimation = velocity;
            currentAngularEstimation = angularVelocity;
		}

		public void StartEstimation()
        {
            isEstimating = true;

            SampleData firstSample = new SampleData
            {
                AngularVelocity = Vect3.zero,
                Velocity = Vect3.zero,
            };

            // clear the buffer
            for (int i=0; i < sampleData.Length; i++)
			{
                sampleData[i] = new SampleData();
			}

            // set the current sample count to 0
            currentSampleCount = 0;

            // set current sample index to 0
            currentSampleIndex = 0;

            // insert the sample
            sampleData[currentSampleIndex] = firstSample;

            // increment the sample count
            currentSampleCount++;
        }

        public void StopEstimation()
        {
            isEstimating = false;
        }

		Vect3 CalculateSingleShotVelocity(Vect3 position, Vect3 previousPosition)
		{
            float velocityFactor = 1.0f / Core.Time.deltaTime;
            return velocityFactor * (position - previousPosition);
		}

        Vect3 CalculateSingleShotAngularVelocity(Quatn rotation, Quatn previousRotation)
		{
            float velocityFactor = 1.0f / Core.Time.deltaTime;

            Quatn offsetRotation = rotation * Quatn.Inverse(previousRotation);
            float theta = 2.0f * Mathf.Acos(Mathf.Clamp(offsetRotation.w, -1.0f, 1.0f));

            if(theta > Mathf.PI)
			{
                theta -= 2.0f * Mathf.PI;
			}

            Vect3 angularVelocity = new Vect3(offsetRotation.x, offsetRotation.y,
                offsetRotation.z);

            if(angularVelocity.sqrMagnitude > 0.0f)
			{
                angularVelocity = theta * velocityFactor * angularVelocity.normalized;
			}

            return angularVelocity;
		}

        SampleData CalculateValues(Vect3 position, Vect3 previousPosition,
            Quatn rotation, Quatn previousRotation)
		{
			Vect3 velocity = CalculateSingleShotVelocity(position, previousPosition);
			Vect3 angularVelocity = CalculateSingleShotAngularVelocity(rotation, previousRotation);
            return new SampleData() { Velocity = velocity, AngularVelocity = angularVelocity };
		}

        // note: calculate your deltas in the class that calls this
        // alternately, provide them here so they can be calculated
        public void SubmitSample(Vect3 position, Vect3 previousPosition,
            Quatn rotation, Quatn previousRotation)
        {
            SampleData sample = CalculateValues(position, previousPosition,
                rotation, previousRotation);

            // adjust sample index
            int newSampleIndex = currentSampleIndex + 1;
            if (newSampleIndex == sampleData.Length) newSampleIndex = 0;

            // figure out if we need to increase the current sample count
            if (currentSampleCount < sampleData.Length) currentSampleCount++;

            // place the sample
            sampleData[newSampleIndex] = sample;
            currentSampleIndex = newSampleIndex;
        }
    }
}