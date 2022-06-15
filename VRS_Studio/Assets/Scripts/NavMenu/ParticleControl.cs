using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour
{
    public ParticleSystem particleSystem;
	private void Start()
	{
		if (particleSystem == null)
		{
			particleSystem = GetComponent<ParticleSystem>();
		}
	}

	public void PlayParticle()
	{
		if (!particleSystem.isPlaying) particleSystem.Play();
	}

	public void StopParticle()
	{
		if (particleSystem.isPlaying) particleSystem.Stop();
	}
}
