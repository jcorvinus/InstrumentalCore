using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

public class SoundPlaybackTest : MonoBehaviour
{
    AudioSource source;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
	}

	[ExposeMethodInEditor]
	void PlaySound()
	{
		source.time = 0;
		source.Play();
	}
}
