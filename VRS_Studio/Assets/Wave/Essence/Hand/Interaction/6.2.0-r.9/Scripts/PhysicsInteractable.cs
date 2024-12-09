using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Wave.Essence.Hand.Interaction
{
	public class PhysicsInteractable : MonoBehaviour
	{
		[SerializeField]
		private float forceMultiplier = 1.0f;

		private readonly int MIN_POSE_SAMPLES = 2;
		private readonly int MAX_POSE_SAMPLES = 10;

		private Rigidbody interactableRigidbody;
		private List<Pose> movementPoses = new List<Pose>();
		private List<float> timestamps = new List<float>();
		private bool isBegin = false;
		private bool isEnd = false;
		private object lockVel = new object();
		private object lockAngVel = new object();

		private void Update()
		{
			if (interactableRigidbody == null) { return; }

			if (isBegin)
			{
				RecordMovement();
			}
		}

		private void FixedUpdate()
		{
			if (interactableRigidbody == null) { return; }

			if (isEnd)
			{
				Vector3 velocity = CalculateVelocity();
				Vector3 angularVelocity = CalculateAngularVelocity();
				interactableRigidbody.ResetInertiaTensor();
				interactableRigidbody.velocity = velocity * forceMultiplier;
				interactableRigidbody.angularVelocity = angularVelocity;
				interactableRigidbody = null;

				movementPoses.Clear();
				timestamps.Clear();
				isEnd = false;
			}
		}

		private void RecordMovement()
		{
			float time = Time.time;
			if (movementPoses.Count == 0 ||
				timestamps[movementPoses.Count - 1] != time)
			{
				movementPoses.Add(new Pose(interactableRigidbody.position, interactableRigidbody.rotation));
				timestamps.Add(time);
			}

			if (movementPoses.Count > MAX_POSE_SAMPLES)
			{
				movementPoses.RemoveAt(0);
				timestamps.RemoveAt(0);
			}
		}

		#region Velocity
		private Vector3 CalculateVelocity()
		{
			if (movementPoses.Count >= MIN_POSE_SAMPLES)
			{
				List<Vector3> velocities = new List<Vector3>();
				for (int i = 0; i < movementPoses.Count - 1; i++)
				{
					for (int j = i + 1; j < movementPoses.Count; j++)
					{
						Vector3 vel = GetVelocity(i, j);
						if (vel != Vector3.zero)
						{
							velocities.Add(vel);
						}
					}
				}
				return FindBestVelocity(velocities);
			}
			return Vector3.zero;
		}

		private Vector3 GetVelocity(int idx1, int idx2)
		{
			if (idx1 >= movementPoses.Count || idx2 >= movementPoses.Count || idx1 == idx2)
			{
				return Vector3.zero;
			}

			var pose1 = movementPoses[idx1];
			var pose2 = movementPoses[idx2];
			var time1 = timestamps[idx1];
			var time2 = timestamps[idx2];

			if (time2 == time1)
			{
				return Vector3.zero;
			}

			return (pose2.position - pose1.position) / (time2 - time1);
		}

		private Vector3 FindBestVelocity(List<Vector3> velocities)
		{
			if (velocities.Count == 0)
			{
				return Vector3.zero;
			}

			Vector3 bestVelocity = Vector3.zero;
			float bestScore = float.PositiveInfinity;

			Parallel.For(0, velocities.Count, i =>
			{
				float score = 0f;
				for (int j = 0; j < velocities.Count; j++)
				{
					if (i != j)
					{
						score += (velocities[i] - velocities[j]).magnitude;
					}
				}

				lock (lockVel)
				{
					if (score < bestScore)
					{
						bestVelocity = velocities[i];
						bestScore = score;
					}
				}
			});

			return bestVelocity;
		}
		#endregion

		#region AngularVelocity
		private Vector3 CalculateAngularVelocity()
		{
			if (movementPoses.Count >= MIN_POSE_SAMPLES)
			{
				List<Vector3> angularVelocities = new List<Vector3>();
				for (int i = 0; i < movementPoses.Count - 1; i++)
				{
					for (int j = i + 1; j < movementPoses.Count; j++)
					{
						Vector3 angVel = GetAngularVelocity(i, j);
						if (angVel != Vector3.zero)
						{
							angularVelocities.Add(angVel);
						}
					}
				}
				return FindBestAngularVelocities(angularVelocities);
			}
			return Vector3.zero;
		}

		private Vector3 GetAngularVelocity(int idx1, int idx2)
		{
			if (idx1 >= movementPoses.Count || idx2 >= movementPoses.Count || idx1 == idx2)
			{
				return Vector3.zero;
			}

			var pose1 = movementPoses[idx1];
			var pose2 = movementPoses[idx2];
			var time1 = timestamps[idx1];
			var time2 = timestamps[idx2];
			if (time2 == time1)
			{
				return Vector3.zero;
			}

			Quaternion diffRotation = pose2.rotation * Quaternion.Inverse(pose1.rotation);
			diffRotation.ToAngleAxis(out float angularSpeed, out Vector3 torqueAxis);
			angularSpeed = (angularSpeed * Mathf.Deg2Rad) / (time2 - time1);
			return torqueAxis * angularSpeed;
		}

		private Vector3 FindBestAngularVelocities(List<Vector3> velocities)
		{
			if (velocities.Count == 0)
			{
				return Vector3.zero;
			}

			Vector3 bestVelocity = Vector3.zero;
			float bestScore = float.PositiveInfinity;

			Parallel.For(0, velocities.Count, i =>
			{
				float score = 0f;
				for (int j = 0; j < velocities.Count; j++)
				{
					if (i != j)
					{
						score += (velocities[i] - velocities[j]).magnitude;
					}
				}

				lock (lockAngVel)
				{
					if (score < bestScore)
					{
						bestVelocity = velocities[i];
						bestScore = score;
					}
				}
			});

			return bestVelocity;
		}
		#endregion

		public void OnBeginInteractabled(IGrabbable grabbable)
		{
			if (grabbable is HandGrabInteractable handGrabbable)
			{
				interactableRigidbody = handGrabbable.rigidbody;
			}
			isBegin = true;
		}

		public void OnEndInteractabled(IGrabbable grabbable)
		{
			isBegin = false;
			isEnd = true;
		}
	}
}
