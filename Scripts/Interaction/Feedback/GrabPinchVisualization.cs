using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Feedback
{
    public class GrabPinchVisualization : PinchVisualization
    {
        protected override PinchInfo GetPinchInfo()
		{
            return hand.GraspPinch;
        }
    }
}