using UnityEngine;

namespace Wave.Essence.Hand
{
	public interface IInteractable
	{
		GameObject actable { get; }
		Transform trans { get; }
		Rigidbody rigid { get; }
	}
}