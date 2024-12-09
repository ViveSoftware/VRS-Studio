// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Text;
using UnityEngine;

using Wave.Essence.BodyTracking.RuntimeDependency;
using Wave.Essence.BodyTracking.AvatarCoordinate;

namespace Wave.Essence.BodyTracking.Demo
{
	public class AvatarTrackingSample : MonoBehaviour
	{
		#region Rdp
		const string LOG_TAG = "Wave.Essence.BodyTracking.Demo.AvatarTrackingSample";
		private StringBuilder m_sb = null;
		private StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Rdp.d(LOG_TAG, msg, true); }
		int logFrame = -1;
		bool printIntervalLog = false;
		#endregion

		#region Inspector
		public Body inputBody;
		private Body m_InitialBody = null;
		private Vector3 m_InitialScale = Vector3.one;

		public TrackerExtrinsic ext;
		public bool autoUpdate = true;

		public AvatarCoordinateProducer JointCoordinate = null;
		#endregion

		private BTDemoHelper.TrackingMode m_TrackingMode = BTDemoHelper.TrackingMode.FullBody;
		public BTDemoHelper.TrackingMode TrackingMode { get { return m_TrackingMode; } }
		public void SetArmMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.Arm;
			sb.Clear().Append("SetArmMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetUpperMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.UpperBody;
			sb.Clear().Append("SetUpperMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetFullMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.FullBody;
			sb.Clear().Append("SetFullMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}
		public void SetUpperBodyAndLegMode()
		{
			m_TrackingMode = BTDemoHelper.TrackingMode.UpperBodyAndLeg;
			sb.Clear().Append("SetUpperBodyAndLegMode() m_TrackingMode: ").Append(m_TrackingMode.Name()); DEBUG(sb);
		}

		private void Awake()
		{
			if (m_InitialBody == null) { m_InitialBody = new Body(); }
			sb.Clear().Append("Awake() Records the initial standard pose and body scale."); DEBUG(sb);
			m_InitialBody.UpdateData(inputBody);
			m_InitialScale = transform.localScale;
		}
		private void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);
		}
		private void OnDisable()
		{
			StopTracking();
		}

		public void BeginCalibration(CalibrationStatusDelegate callback)
		{
			if (BodyManager.Instance == null) { return; }

			sb.Clear().Append("BeginCalibration() ").Append(m_TrackingMode.Name()); DEBUG(sb);
			BodyManager.Instance.StartCalibration((BodyTrackingMode)m_TrackingMode, callback);
		}
		public void StopCalibration()
		{
			if (BodyManager.Instance == null) { return; }

			sb.Clear().Append("StopCalibration() ").Append(m_TrackingMode.Name()); DEBUG(sb);
			BodyManager.Instance.StopCalibration((BodyTrackingMode)m_TrackingMode);
		}

		bool updateTrackingData = false;
		public void BeginTracking()
		{
			sb.Clear().Append("BeginTracking() autoUpdate: ").Append(autoUpdate).Append(", tracking mode: ").Append(m_TrackingMode); DEBUG(sb);

			updateTrackingData = true;
			if (autoUpdate)
				StartCoroutine(StartRenderUpdateBodyTracking());
			else
				StartCoroutine(StartFixUpdateBodyTracking());
		}
		public void StopTracking()
		{
			updateTrackingData = false;
			sb.Clear().Append("StopTracking() Recovers the initial standard pose and body scale."); DEBUG(sb);
			if (inputBody != null && m_InitialBody != null) { m_InitialBody.UpdateBody(ref inputBody); }
			transform.localScale = m_InitialScale;
		}

		private Vector3 ApplyBodyScale(float scale)
		{
			sb.Clear().Append("ApplyBodyScale() ")
				.Append(" old scale (").Append(transform.localScale.x).Append(", ").Append(transform.localScale.y).Append(", ").Append(transform.localScale.z).Append(")")
				.Append(", apply scale ").Append(scale);
			DEBUG(sb);

			transform.localScale *= scale;

			sb.Clear().Append("ApplyBodyScale() ")
				.Append(" new scale (").Append(transform.localScale.x).Append(", ").Append(transform.localScale.y).Append(", ").Append(transform.localScale.z).Append(")");
			DEBUG(sb);

			return transform.localScale;
		}
		private int m_AvatarID = -1;
		public IEnumerator StartRenderUpdateBodyTracking()
		{
			if (BodyManager.Instance == null) { yield return null; }
			sb.Clear().Append("StartRenderUpdateBodyTracking()"); DEBUG(sb);

			BodyTrackingResult result = BodyManager.Instance.CreateBodyTracking(ref m_AvatarID, inputBody, ext, (BodyTrackingMode)m_TrackingMode);
			sb.Clear().Append("StartRenderUpdateBodyTracking() CreateBodyTracking result ").Append(result.Name()).Append(", id: ").Append(m_AvatarID); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			result = BodyManager.Instance.GetBodyTrackingInfo(m_AvatarID, out float avatarHeight, out float avatarScale);
			sb.Clear().Append("StartRenderUpdateBodyTracking() GetBodyTrackingInfo result ").Append(result.Name()); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			// Due to the pose from GetBodyTrackingPoseOnce is "scaled pose", we need to change the avatar mesh size first.
			sb.Clear().Append("StartRenderUpdateBodyTracking() Apply avatar scale with ").Append(avatarScale); DEBUG(sb);
			ApplyBodyScale(avatarScale);

			result = BodyManager.Instance.StartUpdatingBodyTracking(m_AvatarID);
			sb.Clear().Append("StartRenderUpdateBodyTracking() StartUpdatingBodyTracking ").Append(m_AvatarID).Append(" result ").Append(result.Name()); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			while (updateTrackingData)
			{
				result = BodyManager.Instance.GetBodyTrackingPoses(m_AvatarID, out BodyAvatar avatarBody);
				if (result == BodyTrackingResult.SUCCESS) { UpdateBodyPosesInOrder(avatarBody); }
				yield return new WaitForEndOfFrame();
			}

			result = BodyManager.Instance.StopUpdatingBodyTracking(m_AvatarID);
			sb.Clear().Append("StartRenderUpdateBodyTracking() StopUpdatingBodyTracking ").Append(m_AvatarID).Append(" result ").Append(result.Name()); DEBUG(sb);
			
			result = BodyManager.Instance.DestroyBodyTracking(m_AvatarID);
			sb.Clear().Append("StartRenderUpdateBodyTracking() DestroyBodyTracking result ").Append(result.Name()).Append(", id: ").Append(m_AvatarID); DEBUG(sb);
			yield return null;
		}
		public IEnumerator StartFixUpdateBodyTracking()
		{
			if (BodyManager.Instance == null) { yield return null; }
			sb.Clear().Append("StartFixUpdateBodyTracking()"); DEBUG(sb);

			BodyTrackingResult result = BodyManager.Instance.CreateBodyTracking(ref m_AvatarID, inputBody, ext, (BodyTrackingMode)m_TrackingMode);
			sb.Clear().Append("StartFixUpdateBodyTracking() CreateBodyTracking result ").Append(result.Name()).Append(", id: ").Append(m_AvatarID); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			result = BodyManager.Instance.GetBodyTrackingInfo(m_AvatarID, out float avatarHeight, out float avatarScale);
			sb.Clear().Append("StartFixUpdateBodyTracking() GetBodyTrackingInfo result ").Append(result.Name()); DEBUG(sb);
			if (result != BodyTrackingResult.SUCCESS) { yield break; }

			// Due to the pose from GetBodyTrackingPoseOnce is "scaled pose", we need to change the avatar mesh size first.
			sb.Clear().Append("StartFixUpdateBodyTracking() Apply avatar scale with ").Append(avatarScale); DEBUG(sb);
			ApplyBodyScale(avatarScale);

			while (updateTrackingData)
			{
				result = BodyManager.Instance.GetBodyTrackingPoseOnce(m_AvatarID, out BodyAvatar avatarBody);
				if (result == BodyTrackingResult.SUCCESS) { UpdateBodyPosesInOrder(avatarBody); }
				yield return new WaitForEndOfFrame();
			}

			result = BodyManager.Instance.DestroyBodyTracking(m_AvatarID);
			sb.Clear().Append("StartFixUpdateBodyTracking() DestroyBodyTracking result ").Append(result.Name()).Append(", id: ").Append(m_AvatarID); DEBUG(sb);
			yield return null;
		}

		/// <summary>
		/// Update the body joints poses according to the avatar joint order.
		/// If your avatar joint order is different, you have to modify this function.
		/// </summary>
		/// <param name="avatarBody">The avatar IK pose from plugin.</param>
		private void UpdateBodyPosesInOrder(BodyAvatar avatarBody)
		{
			if (inputBody == null || avatarBody == null) { return; }
			if (printIntervalLog)
			{
				sb.Clear().Append("UpdateBodyPosesInOrder() new avatar height ").Append(avatarBody.height)
					.Append(", original avatar height ").Append(m_InitialBody.height)
					.Append(", scale: ").Append(avatarBody.scale);
				DEBUG(sb);
			}

			avatarBody.ChangeJointCoordinate(JointCoordinate);

			if (inputBody.root != null) avatarBody.Update(JointType.HIP, ref inputBody.root);

			if (inputBody.leftThigh != null) avatarBody.Update(JointType.LEFTTHIGH, ref inputBody.leftThigh);
			if (inputBody.leftLeg != null) avatarBody.Update(JointType.LEFTLEG, ref inputBody.leftLeg);
			if (inputBody.leftAnkle != null) avatarBody.Update(JointType.LEFTANKLE, ref inputBody.leftAnkle);
			if (inputBody.leftFoot != null) avatarBody.Update(JointType.LEFTFOOT, ref inputBody.leftFoot);

			if (inputBody.rightThigh != null) avatarBody.Update(JointType.RIGHTTHIGH, ref inputBody.rightThigh); // 5
			if (inputBody.rightLeg != null) avatarBody.Update(JointType.RIGHTLEG, ref inputBody.rightLeg);
			if (inputBody.rightAnkle != null) avatarBody.Update(JointType.RIGHTANKLE, ref inputBody.rightAnkle);
			if (inputBody.rightFoot != null) avatarBody.Update(JointType.RIGHTFOOT, ref inputBody.rightFoot);

			if (inputBody.waist != null) avatarBody.Update(JointType.WAIST, ref inputBody.waist);

			if (inputBody.spineLower != null) avatarBody.Update(JointType.SPINELOWER, ref inputBody.spineLower); // 10
			if (inputBody.spineMiddle != null) avatarBody.Update(JointType.SPINEMIDDLE, ref inputBody.spineMiddle);
			if (inputBody.spineHigh != null) avatarBody.Update(JointType.SPINEHIGH, ref inputBody.spineHigh);

			if (inputBody.chest != null) avatarBody.Update(JointType.CHEST, ref inputBody.chest);
			if (inputBody.neck != null) avatarBody.Update(JointType.NECK, ref inputBody.neck);
			if (inputBody.head != null) avatarBody.Update(JointType.HEAD, ref inputBody.head); // 15

			if (inputBody.leftClavicle != null) avatarBody.Update(JointType.LEFTCLAVICLE, ref inputBody.leftClavicle);
			if (inputBody.leftScapula != null) avatarBody.Update(JointType.LEFTSCAPULA, ref inputBody.leftScapula);
			if (inputBody.leftUpperarm != null) avatarBody.Update(JointType.LEFTUPPERARM, ref inputBody.leftUpperarm);
			if (inputBody.leftForearm != null) avatarBody.Update(JointType.LEFTFOREARM, ref inputBody.leftForearm);
			if (inputBody.leftHand != null) avatarBody.Update(JointType.LEFTHAND, ref inputBody.leftHand); // 20

			if (inputBody.rightClavicle != null) avatarBody.Update(JointType.RIGHTCLAVICLE, ref inputBody.rightClavicle);
			if (inputBody.rightScapula != null) avatarBody.Update(JointType.RIGHTSCAPULA, ref inputBody.rightScapula);
			if (inputBody.rightUpperarm != null) avatarBody.Update(JointType.RIGHTUPPERARM, ref inputBody.rightUpperarm);
			if (inputBody.rightForearm != null) avatarBody.Update(JointType.RIGHTFOREARM, ref inputBody.rightForearm);
			if (inputBody.rightHand != null) avatarBody.Update(JointType.RIGHTHAND, ref inputBody.rightHand); // 25
		}
	}
}
