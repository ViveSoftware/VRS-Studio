using UnityEngine;

namespace Wave.Essence
{
	public interface IInteractor
	{
		GameObject actor { get; }
		Transform trans { get; }
		Rigidbody rigid { get; }
	}
}