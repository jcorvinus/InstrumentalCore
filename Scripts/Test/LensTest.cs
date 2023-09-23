using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensTest : MonoBehaviour
{
    LensedValue<bool> lensedValue;
    Lens<bool> lens0;
    Lens<bool> lens1;
    Lens<bool> lens2;

    public bool ShowValue = false;

    // Start is called before the first frame update
    void Start()
    {
        lens0 = new Lens<bool>(0, (previous) => 
        {
            Debug.Log("lens 0, previous was " + previous.ToString());
            return true; 
        });

        lens1 = new Lens<bool>(1, (previous) =>
		{
            Debug.Log("lens 1, previous was " + previous.ToString());
            return false;
        });

        lens2 = new Lens<bool>(2, (previous) =>
		{
            Debug.Log("lens 2, previous was " + previous.ToString());
            return true;
        });

        lensedValue = new LensedValue<bool>(false);
        lensedValue.AddLens(lens0);
        lensedValue.AddLens(lens1);
        lensedValue.AddLens(lens2);
	}

    // Update is called once per frame
    void Update()
    {
        if(ShowValue)
		{
            ShowValue = false;
            Debug.Log("Final lens value was: " + lensedValue.GetValue());
		}
    }
}
