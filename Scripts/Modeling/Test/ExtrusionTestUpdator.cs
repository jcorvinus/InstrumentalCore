using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;

namespace Instrumental.Modeling
{
    public class ExtrusionTestUpdator : MonoBehaviour
    {
        ExtrusionTest test;
        [SerializeField] ExtrusionTest.ExtrusionTestVariables testVariables;

        [SerializeField] bool updateBatch;
        [SerializeField] bool updateExtrustionDepth;
        [SerializeField] bool updateRadius;
        [SerializeField] bool updateCloseLoop;
        [SerializeField] bool updateFillFace;
        [SerializeField] bool updateCornerVertCount;
        [SerializeField] bool updateWidthVertCount;
        [SerializeField] bool updateWidth;

        private void Awake()
        {
            test = GetComponent<ExtrusionTest>();
        }

        // Update is called once per frame
        void Update()
        {
            if(updateBatch)
            {
                updateBatch = false;
                test.BatchSet(testVariables);
            }

            if(updateExtrustionDepth)
            {
                updateExtrustionDepth = false;
                test.SetExtrusionDepth(testVariables.extrusionDepth);
            }
            if(updateRadius)
            {
                updateRadius = false;
                test.SetRadius(testVariables.radius);
            }
            if(updateCloseLoop)
            {
                updateCloseLoop = false;
                test.SetCloseLoop(testVariables.closeLoop);
            }
            if (updateFillFace)
            {
                updateFillFace = false;
                test.SetFillFace(testVariables.fillFace);
            }
            if (updateCornerVertCount)
            {
                updateCornerVertCount = false;
                test.SetCornerVertCount(testVariables.cornerVertCount);
            }
            if (updateWidthVertCount)
            {
                updateWidthVertCount = false;
                test.SetWidthVertCount(testVariables.widthVertCount);
            }
            if (updateWidth)
            {
                updateWidth = false;
                test.SetWidth(testVariables.width);
            }
        }
    }
}
#endif