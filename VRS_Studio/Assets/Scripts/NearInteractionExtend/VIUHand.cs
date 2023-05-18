using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;

namespace Wave.Essence.Hand.NearInteraction.Extend
{ 
	public static class VIUHand
	{
		private static ViveRoleProperty rightTrackedHandRole = ViveRoleProperty.New(TrackedHandRole.RightHand);
		private static ViveRoleProperty leftTrackedHandRole = ViveRoleProperty.New(TrackedHandRole.LeftHand);
		private static NearHandData left, right;
		private static void validate(bool isLeft)
		{
			if (isLeft)
			{
				var deviceState = VRModule.GetCurrentDeviceState(leftTrackedHandRole.GetDeviceIndex());
				left.valid = deviceState.isPoseValid;
				if (!left.valid) { left.isTracked = false; }
			}
			else
			{
				var deviceState = VRModule.GetCurrentDeviceState(rightTrackedHandRole.GetDeviceIndex());
				right.valid = deviceState.isPoseValid;
				if (!right.valid) { right.isTracked = false; }
			}
		}

		static int updateFrameCountLeft = 0, updateFrameCountRight = 0;
		private static bool AllowUpdateData(bool isLeft)
		{
			if (isLeft && (updateFrameCountLeft != Time.frameCount))
			{
				updateFrameCountLeft = Time.frameCount;
				return true;
			}
			if (!isLeft && (updateFrameCountRight != Time.frameCount))
			{
				updateFrameCountRight = Time.frameCount;
				return true;
			}
			return false;
		}
		private static void UpdateData(bool isLeft)
		{
			if (!AllowUpdateData(isLeft)) { return; }

			if (left.fingers == null || left.fingers.Length != 5) { left.fingers = new NearFingerData[5]; }
			if (right.fingers == null || right.fingers.Length != 5) { right.fingers = new NearFingerData[5]; }

			validate(isLeft);
			if (isLeft && left.valid)
			{
				var deviceState = VRModule.GetCurrentDeviceState(leftTrackedHandRole.GetDeviceIndex());
				//var inst = HandManager.Instance;
				left.isLeft = true;
				left.isTracked = deviceState.isPoseValid;

				/// Use middle joint 0 as the palm due to current palm and wrist pose are equivalent.
				/// VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
				HTC.UnityPlugin.Utility.JointPose middleProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleProximal, out middleProximal);
				left.position = middleProximal.pose.pos;
				left.rotation = middleProximal.pose.rot;
				left.direction = left.rotation * Vector3.up;
				left.normal = left.rotation * Vector3.forward;

				/// Wrist
				HTC.UnityPlugin.Utility.JointPose wrist;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.Wrist, out wrist);
				left.wrist.position = wrist.pose.pos;
				left.wrist.rotation = wrist.pose.rot;

				/// Thumb
				HTC.UnityPlugin.Utility.JointPose thumbMetacarpal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.ThumbMetacarpal, out thumbMetacarpal);
				left.thumb.joint0.position = thumbMetacarpal.pose.pos;
				left.thumb.joint0.rotation = thumbMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.ThumbProximal, out thumbProximal);
				left.thumb.joint1.position = thumbProximal.pose.pos;
				left.thumb.joint1.rotation = thumbProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbDistal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.ThumbDistal, out thumbDistal);
				left.thumb.joint2.position = thumbDistal.pose.pos;
				left.thumb.joint2.rotation = thumbDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbTip;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.ThumbTip, out thumbTip);
				left.thumb.tip.position = thumbTip.pose.pos;
				left.thumb.tip.rotation = thumbTip.pose.rot;
				left.thumb.direction = left.thumb.tip.position - left.thumb.joint2.position;

				/// Index
				HTC.UnityPlugin.Utility.JointPose indexMetacarpal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.IndexMetacarpal, out indexMetacarpal);
				left.index.joint0.position = indexMetacarpal.pose.pos;
				left.index.joint0.rotation = indexMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.IndexProximal, out indexProximal);
				left.index.joint1.position = indexProximal.pose.pos;
				left.index.joint1.rotation = indexProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexIntermediate;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.IndexIntermediate, out indexIntermediate);
				left.index.joint2.position = indexIntermediate.pose.pos;
				left.index.joint2.rotation = indexIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexDistal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.IndexDistal, out indexDistal);
				left.index.joint3.position = indexDistal.pose.pos;
				left.index.joint3.rotation = indexDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexTip;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.IndexTip, out indexTip);
				left.index.tip.position = indexTip.pose.pos;
				left.index.tip.rotation = indexTip.pose.rot;
				left.index.direction = left.index.tip.position - left.index.joint3.position;

				/// Middle
				HTC.UnityPlugin.Utility.JointPose middleMetacarpal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleMetacarpal, out middleMetacarpal);
				left.middle.joint0.position = middleMetacarpal.pose.pos;
				left.middle.joint0.rotation = middleMetacarpal.pose.rot;

				//HTC.UnityPlugin.Utility.JointPose middleProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleProximal, out middleProximal);
				left.middle.joint1.position = middleProximal.pose.pos;
				left.middle.joint1.rotation = middleProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleIntermediate;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleIntermediate, out middleIntermediate);
				left.middle.joint2.position = middleIntermediate.pose.pos;
				left.middle.joint2.rotation = middleIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleDistal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleDistal, out middleDistal);
				left.middle.joint3.position = middleDistal.pose.pos;
				left.middle.joint3.rotation = middleDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleTip;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.MiddleTip, out middleTip);
				left.middle.tip.position = middleTip.pose.pos;
				left.middle.tip.rotation = middleTip.pose.rot;
				left.middle.direction = left.middle.tip.position - left.middle.joint3.position;

				/// Ring
				HTC.UnityPlugin.Utility.JointPose ringMetacarpal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.RingMetacarpal, out ringMetacarpal);
				left.ring.joint0.position = ringMetacarpal.pose.pos;
				left.ring.joint0.rotation = ringMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.RingProximal, out ringProximal);
				left.ring.joint1.position = ringProximal.pose.pos;
				left.ring.joint1.rotation = ringProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringIntermediate;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.RingIntermediate, out ringIntermediate);
				left.ring.joint2.position = ringIntermediate.pose.pos;
				left.ring.joint2.rotation = ringIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringDistal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.RingDistal, out ringDistal);
				left.ring.joint3.position = ringDistal.pose.pos;
				left.ring.joint3.rotation = ringDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringTip;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.RingTip, out ringTip);
				left.ring.tip.position = ringTip.pose.pos;
				left.ring.tip.rotation = ringTip.pose.rot;
				left.ring.direction = left.ring.tip.position - left.ring.joint3.position;

				/// Pinky
				HTC.UnityPlugin.Utility.JointPose pinkyMetacarpal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.PinkyMetacarpal, out pinkyMetacarpal);
				left.pinky.joint0.position = pinkyMetacarpal.pose.pos;
				left.pinky.joint0.rotation = pinkyMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyProximal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.PinkyProximal, out pinkyProximal);
				left.pinky.joint1.position = pinkyProximal.pose.pos;
				left.pinky.joint1.rotation = pinkyProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyIntermediate;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.PinkyIntermediate, out pinkyIntermediate);
				left.pinky.joint2.position = pinkyIntermediate.pose.pos;
				left.pinky.joint2.rotation = pinkyIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyDistal;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.PinkyDistal, out pinkyDistal);
				left.pinky.joint3.position = pinkyDistal.pose.pos;
				left.pinky.joint3.rotation = pinkyDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyTip;
				VivePose.TryGetHandJointPose(leftTrackedHandRole, HandJointName.PinkyTip, out pinkyTip);
				left.pinky.tip.position = pinkyTip.pose.pos;
				left.pinky.tip.rotation = pinkyTip.pose.rot;
				left.pinky.direction = left.pinky.tip.position - left.pinky.joint3.position;

				left.fingers[0] = left.thumb;
				left.fingers[1] = left.index;
				left.fingers[2] = left.middle;
				left.fingers[3] = left.ring;
				left.fingers[4] = left.pinky;
			}
			if (!isLeft && right.valid)
			{
				var deviceState = VRModule.GetCurrentDeviceState(rightTrackedHandRole.GetDeviceIndex());
				//var inst = HandManager.Instance;
				right.isLeft = false;
				right.isTracked = deviceState.isPoseValid;

				/// Use middle joint 0 as the palm due to current palm and wrist pose are equivalent.
				/// VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
				HTC.UnityPlugin.Utility.JointPose middleProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleProximal, out middleProximal);
				right.position = middleProximal.pose.pos;
				right.rotation = middleProximal.pose.rot;
				right.direction = right.rotation * Vector3.up;
				right.normal = right.rotation * Vector3.forward;

				/// Wrist
				HTC.UnityPlugin.Utility.JointPose wrist;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.Wrist, out wrist);
				right.wrist.position = wrist.pose.pos;
				right.wrist.rotation = wrist.pose.rot;

				/// Thumb
				HTC.UnityPlugin.Utility.JointPose thumbMetacarpal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.ThumbMetacarpal, out thumbMetacarpal);
				right.thumb.joint0.position = thumbMetacarpal.pose.pos;
				right.thumb.joint0.rotation = thumbMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.ThumbProximal, out thumbProximal);
				right.thumb.joint1.position = thumbProximal.pose.pos;
				right.thumb.joint1.rotation = thumbProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbDistal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.ThumbDistal, out thumbDistal);
				right.thumb.joint2.position = thumbDistal.pose.pos;
				right.thumb.joint2.rotation = thumbDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose thumbTip;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.ThumbTip, out thumbTip);
				right.thumb.tip.position = thumbTip.pose.pos;
				right.thumb.tip.rotation = thumbTip.pose.rot;
				right.thumb.direction = right.thumb.tip.position - right.thumb.joint2.position;

				/// Index
				HTC.UnityPlugin.Utility.JointPose indexMetacarpal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.IndexMetacarpal, out indexMetacarpal);
				right.index.joint0.position = indexMetacarpal.pose.pos;
				right.index.joint0.rotation = indexMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.IndexProximal, out indexProximal);
				right.index.joint1.position = indexProximal.pose.pos;
				right.index.joint1.rotation = indexProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexIntermediate;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.IndexIntermediate, out indexIntermediate);
				right.index.joint2.position = indexIntermediate.pose.pos;
				right.index.joint2.rotation = indexIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexDistal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.IndexDistal, out indexDistal);
				right.index.joint3.position = indexDistal.pose.pos;
				right.index.joint3.rotation = indexDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose indexTip;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.IndexTip, out indexTip);
				right.index.tip.position = indexTip.pose.pos;
				right.index.tip.rotation = indexTip.pose.rot;
				right.index.direction = right.index.tip.position - right.index.joint3.position;

				/// Middle
				HTC.UnityPlugin.Utility.JointPose middleMetacarpal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleMetacarpal, out middleMetacarpal);
				right.middle.joint0.position = middleMetacarpal.pose.pos;
				right.middle.joint0.rotation = middleMetacarpal.pose.rot;

				//HTC.UnityPlugin.Utility.JointPose middleProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleProximal, out middleProximal);
				right.middle.joint1.position = middleProximal.pose.pos;
				right.middle.joint1.rotation = middleProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleIntermediate;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleIntermediate, out middleIntermediate);
				right.middle.joint2.position = middleIntermediate.pose.pos;
				right.middle.joint2.rotation = middleIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleDistal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleDistal, out middleDistal);
				right.middle.joint3.position = middleDistal.pose.pos;
				right.middle.joint3.rotation = middleDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose middleTip;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.MiddleTip, out middleTip);
				right.middle.tip.position = middleTip.pose.pos;
				right.middle.tip.rotation = middleTip.pose.rot;
				right.middle.direction = right.middle.tip.position - right.middle.joint3.position;

				/// Ring
				HTC.UnityPlugin.Utility.JointPose ringMetacarpal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.RingMetacarpal, out ringMetacarpal);
				right.ring.joint0.position = ringMetacarpal.pose.pos;
				right.ring.joint0.rotation = ringMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.RingProximal, out ringProximal);
				right.ring.joint1.position = ringProximal.pose.pos;
				right.ring.joint1.rotation = ringProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringIntermediate;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.RingIntermediate, out ringIntermediate);
				right.ring.joint2.position = ringIntermediate.pose.pos;
				right.ring.joint2.rotation = ringIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringDistal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.RingDistal, out ringDistal);
				right.ring.joint3.position = ringDistal.pose.pos;
				right.ring.joint3.rotation = ringDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose ringTip;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.RingTip, out ringTip);
				right.ring.tip.position = ringTip.pose.pos;
				right.ring.tip.rotation = ringTip.pose.rot;
				right.ring.direction = right.ring.tip.position - right.ring.joint3.position;

				/// Pinky
				HTC.UnityPlugin.Utility.JointPose pinkyMetacarpal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.PinkyMetacarpal, out pinkyMetacarpal);
				right.pinky.joint0.position = pinkyMetacarpal.pose.pos;
				right.pinky.joint0.rotation = pinkyMetacarpal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyProximal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.PinkyProximal, out pinkyProximal);
				right.pinky.joint1.position = pinkyProximal.pose.pos;
				right.pinky.joint1.rotation = pinkyProximal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyIntermediate;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.PinkyIntermediate, out pinkyIntermediate);
				right.pinky.joint2.position = pinkyIntermediate.pose.pos;
				right.pinky.joint2.rotation = pinkyIntermediate.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyDistal;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.PinkyDistal, out pinkyDistal);
				right.pinky.joint3.position = pinkyDistal.pose.pos;
				right.pinky.joint3.rotation = pinkyDistal.pose.rot;

				HTC.UnityPlugin.Utility.JointPose pinkyTip;
				VivePose.TryGetHandJointPose(rightTrackedHandRole, HandJointName.PinkyTip, out pinkyTip);
				right.pinky.tip.position = pinkyTip.pose.pos;
				right.pinky.tip.rotation = pinkyTip.pose.rot;
				right.pinky.direction = right.pinky.tip.position - right.pinky.joint3.position;

				right.fingers[0] = right.thumb;
				right.fingers[1] = right.index;
				right.fingers[2] = right.middle;
				right.fingers[3] = right.ring;
				right.fingers[4] = right.pinky;
			}
		}
		public static NearHandData Get(bool isLeft)
		{
			UpdateData(isLeft);
			return isLeft ? left : right;
		}
		public static Vector3 GetHand3DPosition(bool isLeft)
		{
			return isLeft ? left.wrist.position : right.wrist.position;
		}
	}
}