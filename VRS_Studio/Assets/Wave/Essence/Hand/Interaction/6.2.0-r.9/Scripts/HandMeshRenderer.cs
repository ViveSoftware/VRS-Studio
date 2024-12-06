// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Wave.Essence.Events;
using Wave.Essence.Extra;
using Wave.Native;

namespace Wave.Essence.Hand.Interaction
{

	[DisallowMultipleComponent]
	public class HandMeshRenderer : MonoBehaviour
	{
		private const string TAG = "HandMeshRenderer";

		private StringBuilder HSB
		{
			get
			{
				return Log.CSB.Append(isLeft ? "Left, " : "Right, ");
			}
		}

		public struct BoneMap
		{
			public HandManager.HandJoint BoneID;
			public string DisplayName;
			public HandManager.HandJoint BoneParentID;

			public BoneMap(HandManager.HandJoint b, string name, HandManager.HandJoint p)
			{
				BoneID = b;
				DisplayName = name;
				BoneParentID = p;
			}
		}

		private const int BONE_MAX_ID = 26;

		#region Name Definition
		// The order of joint name MUST align with runtime's definition
		private readonly string[] BNames = new string[]
		{
				"WaveBone_0",  // WVR_HandJoint_Palm = 0
				"WaveBone_1", // WVR_HandJoint_Wrist = 1
				"WaveBone_2", // WVR_HandJoint_Thumb_Joint0 = 2
				"WaveBone_3", // WVR_HandJoint_Thumb_Joint1 = 3
				"WaveBone_4", // WVR_HandJoint_Thumb_Joint2 = 4
				"WaveBone_5", // WVR_HandJoint_Thumb_Tip = 5
				"WaveBone_6", // WVR_HandJoint_Index_Joint0 = 6
				"WaveBone_7", // WVR_HandJoint_Index_Joint1 = 7
				"WaveBone_8", // WVR_HandJoint_Index_Joint2 = 8
				"WaveBone_9", // WVR_HandJoint_Index_Joint3 = 9
				"WaveBone_10", // WVR_HandJoint_Index_Tip = 10
				"WaveBone_11", // WVR_HandJoint_Middle_Joint0 = 11
				"WaveBone_12", // WVR_HandJoint_Middle_Joint1 = 12
				"WaveBone_13", // WVR_HandJoint_Middle_Joint2 = 13
				"WaveBone_14", // WVR_HandJoint_Middle_Joint3 = 14
				"WaveBone_15", // WVR_HandJoint_Middle_Tip = 15
				"WaveBone_16", // WVR_HandJoint_Ring_Joint0 = 16
				"WaveBone_17", // WVR_HandJoint_Ring_Joint1 = 17
				"WaveBone_18", // WVR_HandJoint_Ring_Joint2 = 18
				"WaveBone_19", // WVR_HandJoint_Ring_Joint3 = 19
				"WaveBone_20", // WVR_HandJoint_Ring_Tip = 20
				"WaveBone_21", // WVR_HandJoint_Pinky_Joint0 = 21
				"WaveBone_22", // WVR_HandJoint_Pinky_Joint0 = 22
				"WaveBone_23", // WVR_HandJoint_Pinky_Joint0 = 23
				"WaveBone_24", // WVR_HandJoint_Pinky_Joint0 = 24
				"WaveBone_25" // WVR_HandJoint_Pinky_Tip = 25
		};
		#endregion

		public BoneMap[] boneMap = new BoneMap[]
		{
			new BoneMap(HandManager.HandJoint.Palm, "Palm", HandManager.HandJoint.Wrist),  // 0
			new BoneMap(HandManager.HandJoint.Wrist, "Wrist", HandManager.HandJoint.Wrist),// 1
			new BoneMap(HandManager.HandJoint.Thumb_Joint0, "Thumb root", HandManager.HandJoint.Wrist), // 2
			new BoneMap(HandManager.HandJoint.Thumb_Joint1, "Thumb joint1", HandManager.HandJoint.Thumb_Joint0), // 3
			new BoneMap(HandManager.HandJoint.Thumb_Joint2, "Thumb joint2", HandManager.HandJoint.Thumb_Joint1), // 4
			new BoneMap(HandManager.HandJoint.Thumb_Tip, "Thumb tip", HandManager.HandJoint.Thumb_Joint2), // 5
			new BoneMap(HandManager.HandJoint.Index_Joint0, "Index root", HandManager.HandJoint.Wrist),  // 6
			new BoneMap(HandManager.HandJoint.Index_Joint1, "Index joint1", HandManager.HandJoint.Index_Joint0), // 7
			new BoneMap(HandManager.HandJoint.Index_Joint2, "Index joint2", HandManager.HandJoint.Index_Joint1), // 8
			new BoneMap(HandManager.HandJoint.Index_Joint3, "Index joint3", HandManager.HandJoint.Index_Joint2), // 9
			new BoneMap(HandManager.HandJoint.Index_Tip, "Index tip", HandManager.HandJoint.Index_Joint3), // 10
			new BoneMap(HandManager.HandJoint.Middle_Joint0, "Middle root", HandManager.HandJoint.Wrist), // 11
			new BoneMap(HandManager.HandJoint.Middle_Joint1, "Middle joint1", HandManager.HandJoint.Middle_Joint0), // 12
			new BoneMap(HandManager.HandJoint.Middle_Joint2, "Middle joint2", HandManager.HandJoint.Middle_Joint1), // 13
			new BoneMap(HandManager.HandJoint.Middle_Joint3, "Middle joint3", HandManager.HandJoint.Middle_Joint2), // 14
			new BoneMap(HandManager.HandJoint.Middle_Tip, "Middle tip", HandManager.HandJoint.Middle_Joint3), // 15
			new BoneMap(HandManager.HandJoint.Ring_Joint0, "Ring root", HandManager.HandJoint.Wrist), // 16
			new BoneMap(HandManager.HandJoint.Ring_Joint1, "Ring joint1", HandManager.HandJoint.Ring_Joint0), // 17
			new BoneMap(HandManager.HandJoint.Ring_Joint2, "Ring joint2", HandManager.HandJoint.Ring_Joint1), // 18
			new BoneMap(HandManager.HandJoint.Ring_Joint3, "Ring joint3", HandManager.HandJoint.Ring_Joint2), // 19
			new BoneMap(HandManager.HandJoint.Ring_Tip, "Ring tip", HandManager.HandJoint.Ring_Joint3), // 20
			new BoneMap(HandManager.HandJoint.Pinky_Joint0, "Pinky root", HandManager.HandJoint.Wrist), // 21
			new BoneMap(HandManager.HandJoint.Pinky_Joint1, "Pinky joint1", HandManager.HandJoint.Pinky_Joint0), // 22
			new BoneMap(HandManager.HandJoint.Pinky_Joint2, "Pinky joint2", HandManager.HandJoint.Pinky_Joint1), // 23
			new BoneMap(HandManager.HandJoint.Pinky_Joint3, "Pinky joint3", HandManager.HandJoint.Pinky_Joint2), // 24
			new BoneMap(HandManager.HandJoint.Pinky_Tip, "Pinky tip", HandManager.HandJoint.Pinky_Joint3), // 25
		};

		private const float minAlpha = 0.2f;

		#region Inspector

		[SerializeField]
		private Handedness m_Handedness = Handedness.Left;
		public bool isLeft => m_Handedness == Handedness.Left;

		[SerializeField]
		private bool m_EnableCollider = false;
		public bool enableCollider
		{
			get { return m_EnableCollider; }
			set
			{
				m_EnableCollider = value;
				if (m_EnableCollider && m_HandCollider == null)
				{
					InitHandCollider();
				}
				else if (m_HandCollider != null)
				{
					m_HandCollider.enableCollider = m_EnableCollider;
				}
			}
		}

		[SerializeField]
		private bool m_UseRuntimeModel = true;
		[SerializeField]
		private bool m_UseScale = true;

		#endregion

		[Tooltip("Show electronic hand in controller mode")]
		private bool showElectronicHandInControllerMode = false;
		[Tooltip("Root object of skinned mesh")]
		private GameObject Hand;

		[Tooltip("Nodes of skinned mesh, must be size of 26 in same order as skeleton definition")]
		private Transform[] BonePoses = new Transform[BONE_MAX_ID];
		private Transform[] runtimeBonePoses = new Transform[BONE_MAX_ID];
		[SerializeField]
		private Transform[] customizedBonePoses = new Transform[BONE_MAX_ID];
		[Tooltip("Use hand confidence as alpha, low confidence hand becomes transparent")]
		private bool showConfidenceAsAlpha = false;

		private GameObject m_SystemHand = null;
		private GameObject m_SystemHandMesh = null;
		private GameObject[] handTran = new GameObject[BONE_MAX_ID];
		private Mesh mesh = null;
		private SkinnedMeshRenderer skinMeshRend = null;
		private SkinnedMeshRenderer customizedSkinMeshRend = null;
		IntPtr handModel = IntPtr.Zero;

		private JSON_HandModelDesc_Ext handModelDesc = null;
		private bool isHandStable = false;

		private int rootId = (int)JointType.Wrist;
		private bool updateRoot = false;
		private int updatedFrameCount = 0;
		private bool isGrabbing = false;
		private bool isConstraint = false;
		private HandGrabInteractor handGrabber;
		private Quaternion[] grabJointsRotation = new Quaternion[BONE_MAX_ID];
		private HandColliderController m_HandCollider = null;

		private Transform FindChildRecursive(Transform parent, string name)
		{
			foreach (Transform child in parent)
			{
				if (child.name.Contains(name))
					return child;

				var result = FindChildRecursive(child, name);
				if (result != null)
					return result;
			}
			return null;
		}

		#region Button Event

		public void AutoDetect()
		{
			Log.d(TAG, HSB.Append("AutoDetect()"));
			customizedSkinMeshRend = transform.GetComponentInChildren<SkinnedMeshRenderer>();
			if (customizedSkinMeshRend == null)
			{
				Log.d(TAG, HSB.Append("AutoDetect() Cannot find SkinnedMeshRenderer in ").Append(name));
				return;
			}

			for (int i = 0; i < boneMap.Length; i++)
			{
				string searchName = BNames[i];
				Transform t = FindChildRecursive(transform, searchName);

				if (t == null)
				{
					Log.d(TAG, HSB.Append("AutoDetect() ").Append(boneMap[i].DisplayName).Append(" not found!"));
					continue;
				}

				Log.d(TAG, HSB.Append("AutoDetect() ").Append(boneMap[i].DisplayName).Append(" found: ").Append(searchName));
				customizedBonePoses[i] = t;
			}
			Log.d(TAG, HSB.Append("AutoDetect--"));
		}

		public void ClearDetect()
		{
			for (int i = 0; i < BonePoses.Length; i++)
			{
				customizedBonePoses[i] = null;
			}
		}

		public void OnHandBeginGrab(IGrabber grabber)
		{
			if (grabber is HandGrabInteractor handGrabber)
			{
				this.handGrabber = handGrabber;

				if (grabber.grabbable is HandGrabInteractable handGrabbable)
				{
					if (handGrabbable.bestGrabPose != GrabPose.Identity)
					{
						if (handGrabbable.bestGrabPose.recordedGrabRotations.Length == BONE_MAX_ID)
						{
							grabJointsRotation = handGrabbable.bestGrabPose.recordedGrabRotations;
						}
						else if (handGrabbable.bestGrabPose.handGrabGesture != HandGrabGesture.Identity)
						{
							for (int i = 0; i < grabJointsRotation.Length; i++)
							{
								HandData.GetDefaultJointRotationInGesture(isLeft, handGrabbable.bestGrabPose.handGrabGesture, (JointType)i, ref grabJointsRotation[i]);
							}
						}
						isGrabbing = true;
						isConstraint = handGrabbable.isContraint;
					}
				}
			}

			if (m_EnableCollider && m_HandCollider != null)
			{
				m_HandCollider.OnHandBeginGrab(grabber);
			}
		}

		public void OnHandEndGrab(IGrabber grabber)
		{
			isGrabbing = false;
			this.handGrabber = null;

			if (m_EnableCollider && m_HandCollider != null)
			{
				m_HandCollider.OnHandEndGrab(grabber);
			}
		}

		#endregion

		private void ReadJson()
		{
			try
			{
				handModelDesc = new JSON_HandModelDesc_Ext(OEMConfig.getHandModelDesc());
			}
			catch (Exception e)
			{
				handModelDesc = null;
				Log.d(TAG, e.ToString());
			}

			if (handModelDesc == null)
			{
				Log.d(TAG, "Apply fixed OEM data.", true);
				handModelDesc = new JSON_HandModelDesc_Ext();
				handModelDesc.default_style = new JSON_HandStyleDesc_Ext();
				handModelDesc.default_style.gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.default_style.gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.default_style.con_gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.default_style.con_gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.default_style.thickness = 0.001f;
				handModelDesc.default_style.filling_opacity = 0.45f;
				handModelDesc.default_style.contouring_opacity = 0.5f;
				handModelDesc.fusion_style = new JSON_HandStyleDesc_Ext();
				handModelDesc.fusion_style.gra_color_A = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.fusion_style.gra_color_B = new Color(1, 1, 1, 0);
				handModelDesc.fusion_style.con_gra_color_A = new Color(1, 1, 1, 0);
				handModelDesc.fusion_style.con_gra_color_B = new Color(0.1058824f, 0.6901961f, 0.9019608f, 0);
				handModelDesc.fusion_style.thickness = 0.002f;
				handModelDesc.fusion_style.filling_opacity = 0.45f;
				handModelDesc.fusion_style.contouring_opacity = 0.5f;
			}

			for (int i = 0; i < 2; i++)
			{
				var style = i == 0 ? handModelDesc.default_style : handModelDesc.fusion_style;
				var sb = Log.CSB.Append(i == 0 ? "default_style { " : "fusion_style { ");
				style.Dump(sb);
				sb.Append(" } ");
				Log.d(TAG, sb, true);
			}
		}

		private bool GetNaturalHandModel()
		{
			Log.d(TAG, HSB.Append("GetNaturalHandModel"));
			WVR_Result r = Interop.WVR_GetCurrentNaturalHandModel(ref handModel);
			Log.d(TAG, HSB.Append("WVR_GetCurrentNaturalHandModel, handModel IntPtr = ").Append(handModel.ToInt32()));
			Log.d(TAG, HSB.Append("sizeof(WVR_HandRenderModel) = ").Append(Marshal.SizeOf(typeof(WVR_HandRenderModel))));
			if (r == WVR_Result.WVR_Success)
			{
				if (handModel != IntPtr.Zero)
				{
					Log.d(TAG, HSB.Append("handModels"));
					WVR_HandRenderModel handModels = (WVR_HandRenderModel)Marshal.PtrToStructure(handModel, typeof(WVR_HandRenderModel));
					Log.d(TAG, HSB.Append("handModels--"));
					if (isLeft)
						CreateHandMesh(handModels.left, handModels.handAlphaTex);
					else
						CreateHandMesh(handModels.right, handModels.handAlphaTex);
				}
			}
			else
			{
				Log.d(TAG, HSB.Append("GetCurrentNaturalHandModel failed: ").Append(r));
				return false;
			}

			skinMeshRend = m_SystemHand.GetComponentInChildren<SkinnedMeshRenderer>();
			if (skinMeshRend == null)
			{
				Log.d(TAG, HSB.Append("Cannot find SkinnedMeshRenderer in ").Append(name));
				return false;
			}
			for (int i = 0; i < boneMap.Length; i++)
			{
				string searchName = BNames[i];
				Transform t = FindChildRecursive(transform, searchName);

				if (t == null)
				{
					Log.d(TAG, HSB.Append("GetNaturalHandModel() ").Append(boneMap[i].DisplayName).Append(" not found!"));
					continue;
				}

				Log.d(TAG, HSB.Append("GetNaturalHandModel() ").Append(boneMap[i].DisplayName).Append(" found: ").Append(searchName));
			}
			Interop.WVR_ReleaseNaturalHandModel(ref handModel);
			Log.d(TAG, HSB.Append("GetNaturalHandModel--"));
			return true;
		}

		private Matrix4x4 FromToGL(Matrix4x4 i)
		{
			var m = Matrix4x4.identity;
			int sign = -1;

			m[0, 0] = i[0, 0];
			m[0, 1] = i[0, 1];
			m[0, 2] = i[0, 2] * sign;
			m[0, 3] = i[0, 3];

			m[1, 0] = i[1, 0];
			m[1, 1] = i[1, 1];
			m[1, 2] = i[1, 2] * sign;
			m[1, 3] = i[1, 3];

			m[2, 0] = i[2, 0] * sign;
			m[2, 1] = i[2, 1] * sign;
			m[2, 2] = i[2, 2];
			m[2, 3] = i[2, 3] * sign;

			m[3, 0] = i[3, 0];
			m[3, 1] = i[3, 1];
			m[3, 2] = i[3, 2];
			m[3, 3] = i[3, 3];

			return m;
		}

		private void CreateHandMesh(WVR_HandModel hand, WVR_CtrlerTexBitmap texBitmap)
		{
			Log.d(TAG, HSB.Append("CreateHandMesh"));

			ReadJson();

			m_SystemHand = new GameObject("SystemHand" + (isLeft ? "Left" : "Right"));
			m_SystemHand.transform.SetParent(transform, false);
			m_SystemHandMesh = new GameObject("SystemHandMesh" + (isLeft ? "Left" : "Right"));
			m_SystemHandMesh.transform.SetParent(m_SystemHand.transform, false);

			Log.d(TAG, HSB.Append("handTran create"));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				handTran[i] = new GameObject("WaveBone_" + i);
				handTran[i].SetActive(true);
				runtimeBonePoses[i] = handTran[i].transform;
			}
			Log.d(TAG, HSB.Append("handTran create--"));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				if (hand.jointParentTable[i] == 47)
				{
					handTran[i].transform.parent = m_SystemHand.transform;
				}
				else
				{
					handTran[i].transform.parent = handTran[hand.jointParentTable[i]].transform;
				}
			}

			if (m_SystemHand != null)
				Log.d(TAG, HSB.Append(m_SystemHand.name).Append(" parent: ").Append(m_SystemHand.transform.parent.name));
			if (m_SystemHandMesh != null)
				Log.d(TAG, HSB.Append(m_SystemHandMesh.name).Append(" parent: ").Append(m_SystemHandMesh.transform.parent.name));
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				Log.d(TAG, HSB.Append(handTran[i].name).Append(" parent: ").Append(handTran[i].transform.parent.name));
			}

			/*create basic mesh*/
			mesh = new Mesh();
			Vector3[] _vertices;
			Vector3[] _normals;
			Vector2[] _uv;
			Vector2[] _uv2;
			//vertices
			WVR_VertexBuffer vertices = hand.vertices;
			if (vertices.dimension == 3)
			{
				uint verticesCount = (vertices.size / vertices.dimension);

				Log.d(TAG, HSB.Append(" vertices size = ").Append(vertices.size).Append(", dimension = ").Append(vertices.dimension).Append(", count = ").Append(verticesCount));

				_vertices = new Vector3[verticesCount];
				float[] verticeArray = new float[vertices.size];

				Marshal.Copy(vertices.buffer, verticeArray, 0, verticeArray.Length);

				int verticeIndex = 0;
				int floatIndex = 0;

				while (verticeIndex < verticesCount)
				{
					_vertices[verticeIndex] = new Vector3();
					_vertices[verticeIndex].x = verticeArray[floatIndex++];
					_vertices[verticeIndex].y = verticeArray[floatIndex++];
					_vertices[verticeIndex].z = verticeArray[floatIndex++] * -1.0f;
					verticeIndex++;
				}
				mesh.vertices = _vertices;
			}
			else
			{
				Log.d(TAG, HSB.Append("vertices buffer's dimension incorrect!"));
			}
			// normals
			WVR_VertexBuffer normals = hand.normals;

			if (normals.dimension == 3)
			{
				uint normalsCount = (normals.size / normals.dimension);
				Log.d(TAG, HSB.Append(" normals size = ").Append(normals.size).Append(", dimension = ").Append(normals.dimension).Append(", count = ").Append(normalsCount));
				_normals = new Vector3[normalsCount];
				float[] normalArray = new float[normals.size];

				Marshal.Copy(normals.buffer, normalArray, 0, normalArray.Length);

				int normalsIndex = 0;
				int floatIndex = 0;

				while (normalsIndex < normalsCount)
				{
					_normals[normalsIndex] = new Vector3();
					_normals[normalsIndex].x = normalArray[floatIndex++];
					_normals[normalsIndex].y = normalArray[floatIndex++];
					_normals[normalsIndex].z = normalArray[floatIndex++] * -1.0f;

					normalsIndex++;
				}

				mesh.normals = _normals;
			}
			else
			{
				Log.d(TAG, HSB.Append("normals buffer's dimension incorrect!"));
			}

			// texCoord
			WVR_VertexBuffer texCoord = hand.texCoords;

			if (texCoord.dimension == 2)
			{
				uint uvCount = (texCoord.size / texCoord.dimension);
				Log.d(TAG, HSB.Append(" texCoord size = ").Append(texCoord.size).Append(", dimension = ").Append(texCoord.dimension).Append(", count = ").Append(uvCount));
				_uv = new Vector2[uvCount];
				float[] texCoordArray = new float[texCoord.size];

				Marshal.Copy(texCoord.buffer, texCoordArray, 0, texCoordArray.Length);

				int uvIndex = 0;
				int floatIndex = 0;

				while (uvIndex < uvCount)
				{
					_uv[uvIndex] = new Vector2();
					_uv[uvIndex].x = texCoordArray[floatIndex++];
					_uv[uvIndex].y = 1 - texCoordArray[floatIndex++];

					uvIndex++;
				}
				mesh.uv = _uv;
			}
			else
			{
				Log.d(TAG, HSB.Append("texCoord buffer's dimension incorrect!"));
			}

			// texCoord2s
			WVR_VertexBuffer texCoord2s = hand.texCoord2s;

			if (texCoord2s.dimension == 2)
			{
				uint uvCount = (texCoord2s.size / texCoord2s.dimension);
				Log.d(TAG, HSB.Append(" texCoord2s size = ").Append(texCoord2s.size).Append(", dimension = ").Append(texCoord2s.dimension).Append(", count = ").Append(uvCount));
				_uv2 = new Vector2[uvCount];
				float[] texCoord2sArray = new float[texCoord2s.size];

				Marshal.Copy(texCoord2s.buffer, texCoord2sArray, 0, texCoord2sArray.Length);

				int uv2Index = 0;
				int uv2floatIndex = 0;

				while (uv2Index < uvCount)
				{
					_uv2[uv2Index] = new Vector2();
					_uv2[uv2Index].x = texCoord2sArray[uv2floatIndex++];
					_uv2[uv2Index].y = 1 - texCoord2sArray[uv2floatIndex++];

					uv2Index++;
				}
				mesh.uv2 = _uv2;
			}
			else
			{
				Log.d(TAG, HSB.Append("texCoord2s buffer's dimension incorrect!"));
			}

			// indices
			WVR_IndexBuffer indices = hand.indices;
			Log.d(TAG, HSB.Append(" indices size = ").Append(indices.size));
			int[] indicesArray = new int[indices.size];
			Marshal.Copy(indices.buffer, indicesArray, 0, indicesArray.Length);

			uint indiceIndex = 0;

			while (indiceIndex < indices.size)
			{
				int tmp = indicesArray[indiceIndex];
				indicesArray[indiceIndex] = indicesArray[indiceIndex + 2];
				indicesArray[indiceIndex + 2] = tmp;
				indiceIndex += 3;
			}
			mesh.SetIndices(indicesArray, MeshTopology.Triangles, 0);

			/* assign bone weights to mesh*/
			//boneIDs
			if (hand.boneIDs.dimension == 4 && hand.boneWeights.dimension == 4)
			{
				uint boneIDsCount = (hand.boneIDs.size / hand.boneIDs.dimension);
				Log.d(TAG, HSB.Append(" boneIDs size = ").Append(hand.boneIDs.size).Append(", dimension = ").Append(hand.boneIDs.dimension).Append(", count = ").Append(boneIDsCount));
				int[] boneIDsArray = new int[hand.boneIDs.size];
				Marshal.Copy(hand.boneIDs.buffer, boneIDsArray, 0, boneIDsArray.Length);
				uint boneWeightsCount = (hand.boneWeights.size / hand.boneWeights.dimension);
				Log.d(TAG, HSB.Append(" boneWeights size = ").Append(hand.boneWeights.size).Append(", dimension = ").Append(hand.boneWeights.dimension).Append(", count = ").Append(boneWeightsCount));
				float[] boneWeightsArray = new float[hand.boneWeights.size];
				Marshal.Copy(hand.boneWeights.buffer, boneWeightsArray, 0, boneWeightsArray.Length);

				BoneWeight[] weights = new BoneWeight[boneIDsCount];

				int boneIndex = 0, IDIndex = 0, weightIndex = 0;
				while (boneIndex < boneIDsCount)
				{
					int[] currentBoneIDsArray = new int[4];
					float[] currentBoneWeightsArray = new float[4];

					currentBoneIDsArray[0] = BoneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[1] = BoneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[2] = BoneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneIDsArray[3] = BoneIDCorrection(boneIDsArray[IDIndex++]);
					currentBoneWeightsArray[0] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[1] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[2] = boneWeightsArray[weightIndex++];
					currentBoneWeightsArray[3] = boneWeightsArray[weightIndex++];

					Array.Sort(currentBoneWeightsArray, currentBoneIDsArray); //Sort bone ID by weight

					//Assign by bone weight ID by descending weight order
					weights[boneIndex].boneIndex0 = currentBoneIDsArray[3];
					weights[boneIndex].boneIndex1 = currentBoneIDsArray[2];
					weights[boneIndex].boneIndex2 = currentBoneIDsArray[1];
					weights[boneIndex].boneIndex3 = currentBoneIDsArray[0];
					weights[boneIndex].weight0 = currentBoneWeightsArray[3];
					weights[boneIndex].weight1 = currentBoneWeightsArray[2];
					weights[boneIndex].weight2 = currentBoneWeightsArray[1];
					weights[boneIndex].weight3 = currentBoneWeightsArray[0];

					boneIndex++;
				}
				mesh.boneWeights = weights;
			}
			else
			{
				Log.d(TAG, HSB.Append("boneIDs buffer dimension = ").Append(hand.boneIDs.dimension).Append("or boneWeights buffer dimension = ").Append(hand.boneWeights.dimension).Append("is incorrect!"));
			}

			// model texture section
			var rawImageSize = texBitmap.height * texBitmap.stride;
			byte[] modelTextureData = new byte[rawImageSize];
			Marshal.Copy(texBitmap.bitmap, modelTextureData, 0, modelTextureData.Length);

			Texture2D modelpng = new Texture2D((int)texBitmap.width, (int)texBitmap.height, TextureFormat.RGBA32, false);
			modelpng.LoadRawTextureData(modelTextureData);
			modelpng.Apply();

			for (int q = 0; q < 10240; q += 1024)
			{
				string textureContent = "";

				for (int c = 0; c < 64; c++)
				{
					if ((q * 64 + c) >= modelTextureData.Length)
						break;
					textureContent += modelTextureData.GetValue(q * 64 + c).ToString();
					textureContent += " ";
				}
			}

			/* Create Bone Transforms and Bind poses */
			Matrix4x4[] bindPoses = new Matrix4x4[BONE_MAX_ID];
			for (int i = 0; i < runtimeBonePoses.Length; i++)
			{
				bindPoses[i] = FromToGL(hand.jointInvTransMats[i]);
				var m = FromToGL(hand.jointLocalTransMats[i]);
				var pos = handTran[i].transform.localPosition = GetPosition(m);
				var rot = handTran[i].transform.localRotation = m.rotation;
				handTran[i].transform.localScale = Vector3.one;
			}

			m_SystemHand.SetActive(true);
			m_SystemHandMesh.SetActive(true);
			mesh.bindposes = bindPoses;
			Material ImgMaterial;
			if (isLeft)
				ImgMaterial = Resources.Load("Materials/HandMatLeft", typeof(Material)) as Material;
			else
				ImgMaterial = Resources.Load("Materials/HandMatRight", typeof(Material)) as Material;
			skinMeshRend = m_SystemHandMesh.AddComponent<SkinnedMeshRenderer>();
			if (skinMeshRend != null)
			{
				skinMeshRend.bones = runtimeBonePoses;
				skinMeshRend.sharedMesh = mesh;
				skinMeshRend.rootBone = handTran[1].transform;
				if (ImgMaterial == null)
				{
					Log.d(TAG, HSB.Append("ImgMaterial is null"));
				}
				skinMeshRend.material = ImgMaterial;
				skinMeshRend.material.mainTexture = modelpng;
				skinMeshRend.enabled = true;
				isHandStable = false;  // default_style
				SetRuntimeModelMaterialStyle(isHandStable);
			}
			else
			{
				Log.d(TAG, HSB.Append("SkinnedMeshRenderer is null"));
			}
			Log.d(TAG, HSB.Append("CreateHandMesh--"));
		}

		private void SetRuntimeModelMaterialStyle(bool isStable)
		{
			if (handModelDesc == null || handModelDesc.fusion_style == null || handModelDesc.default_style == null)
			{
				Log.w(TAG, "no OEM config");
				return;
			}
			var style = isStable ? handModelDesc.fusion_style : handModelDesc.default_style;
			skinMeshRend.material.SetColor("_GraColorA", style.gra_color_A);
			skinMeshRend.material.SetColor("_GraColorB", style.gra_color_B);
			skinMeshRend.material.SetColor("_ConGraColorA", style.con_gra_color_A);
			skinMeshRend.material.SetColor("_ConGraColorB", style.con_gra_color_B);
			skinMeshRend.material.SetFloat("_OutlineThickness", style.thickness);
			skinMeshRend.material.SetFloat("_Opacity", style.filling_opacity);
			skinMeshRend.material.SetFloat("_line_opacity", style.contouring_opacity);
			// Only show variables which will be updated.
			Log.d(TAG, Log.CSB
				.Append("SetStyle=").Append(isStable ? "fusion " : "default ")
				//.Append("CA=").Append(skinMeshRend.material.GetColor("_GraColorA")).Append(", ")
				//.Append("CB=").Append(skinMeshRend.material.GetColor("_GraColorB")).Append(", ")
				.Append("CCA=").Append(skinMeshRend.material.GetColor("_ConGraColorA")).Append(", ")
				.Append("CCB=").Append(skinMeshRend.material.GetColor("_ConGraColorB")).Append(", ")
				//.Append("Op=").Append(skinMeshRend.material.GetFloat("_Opacity")).Append(", ")
				.Append("LOp=").Append(skinMeshRend.material.GetFloat("_line_opacity")).Append(", ")
				.Append("Th=").Append(skinMeshRend.material.GetFloat("_OutlineThickness")));
		}

		// This is only used for OEM config. Customized hand didn't need this.
		private void CheckMaterial()
		{
			var hm = HandManager.Instance;
			if (hm == null)
				return;
			bool st = hm.IsWristPositionFused();
			if (st == isHandStable)
				return;
			isHandStable = st;
			SetRuntimeModelMaterialStyle(isHandStable);
		}

		private void UpdateBonePose()
		{
			if (BonePoses[rootId] == null || BonePoses[(int)JointType.Palm] == null) { return; }

			HandPose handPose = HandPoseProvider.GetHandPose(isLeft ? HandPoseType.HAND_LEFT : HandPoseType.HAND_RIGHT);
			bool isTracked = handPose.IsTracked();
			EnableHandModel(isTracked);
			if (!isTracked) { return; }

			if (m_UseRuntimeModel || (!m_UseRuntimeModel && m_UseScale))
			{
				Vector3 scale = Vector3.one;
				if (GetHandScale(ref scale, isLeft))
				{
					BonePoses[rootId].localScale = scale;
				}
				else
				{
					BonePoses[rootId].localScale = Vector3.one;
					if (Log.gpl.Print)
						Log.d(TAG, HSB.Append("Invalid scale"));
				}
			}

			if (Time.frameCount - updatedFrameCount > 5)
			{
				updateRoot = false;
			}
			if (!updateRoot)
			{
				handPose.GetPosition((JointType)rootId, out Vector3 rootPosition);
				handPose.GetRotation((JointType)rootId, out Quaternion rootRotation);

				BonePoses[rootId].position = BonePoses[rootId].parent.position + rootPosition;
				BonePoses[rootId].rotation = BonePoses[rootId].parent.rotation * rootRotation;
			}

			for (int i = 0; i < BonePoses.Length; i++)
			{
				if (BonePoses[i] == null || i == rootId) { continue; }

				handPose.GetRotation((JointType)i, out Quaternion jointRotation);
				BonePoses[i].rotation = BonePoses[rootId].parent.rotation * jointRotation;
			}

			if (isGrabbing)
			{
				for (int i = 0; i < BonePoses.Length; i++)
				{
					if (i == rootId) { continue; }

					Quaternion currentRotation = BonePoses[i].rotation;
					Quaternion maxRotation = BonePoses[i].parent.rotation * grabJointsRotation[i];
					if (isConstraint ||
						handGrabber.IsRequiredJoint((JointType)i) ||
						OverFlex(currentRotation, maxRotation) >= 0 ||
						FlexAngle(currentRotation, maxRotation) >= 110)
					{
						BonePoses[i].rotation = maxRotation;
					}
				}
			}
		}

		private void EnableHandModel(bool enable)
		{
			if (BonePoses[rootId].gameObject.activeSelf != enable)
			{
				BonePoses[rootId].gameObject.SetActive(enable);
			}
		}

		private float OverFlex(Quaternion currentRot, Quaternion maxRot)
		{
			Vector3 currFwd = currentRot * Vector3.forward;
			Vector3 maxFwd = maxRot * Vector3.forward;
			return Vector3.Dot(currentRot * Vector3.left, Vector3.Cross(currFwd, maxFwd));
		}

		private float FlexAngle(Quaternion currentRot, Quaternion maxRot)
		{
			Vector3 currFwd = currentRot * Vector3.up;
			Vector3 maxFwd = maxRot * Vector3.up;
			return Mathf.Acos(Vector3.Dot(currFwd, maxFwd) / (currFwd.magnitude * maxFwd.magnitude)) * Mathf.Rad2Deg;
		}

		private bool GetHandScale(ref Vector3 scale, bool isLeft)
		{
			if (HandManager.Instance == null) { return false; }
			return HandManager.Instance.GetHandScale(ref scale, isLeft);
		}

		private float GetHandConfidence(bool isLeft)
		{
			if (HandManager.Instance == null) { return 0; }
			return HandManager.Instance.GetHandConfidence(isLeft);
		}

		private bool IsPoseValid()
		{
			bool isPoseValid = false;
			if (hasIMManager)
			{
				currInteractionMode = ClientInterface.InteractionMode;

				if (currInteractionMode != preInteractionMode)
				{
					Log.d(TAG, HSB.Append("Interaction mode changed to ").Append(currInteractionMode));
					preInteractionMode = currInteractionMode;

					if (currInteractionMode == XR_InteractionMode.Controller)
					{
						// show electronic hand?
						bool isSupported = Interop.WVR_ControllerSupportElectronicHand();

						showECHand = (isSupported && showElectronicHandInControllerMode);

						Log.d(TAG, HSB.Append("Device support electronic hand? ").Append(isSupported).Append(", show electronic hand in controller mode? ").Append(showElectronicHandInControllerMode));
					}
				}

				if (ClientInterface.InteractionMode == XR_InteractionMode.Hand)
				{
					isPoseValid = (HandManager.Instance != null) &&
						(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Natural, isLeft));
				}

				if (ClientInterface.InteractionMode == XR_InteractionMode.Controller)
					isPoseValid = showECHand && (HandManager.Instance != null) &&
						(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Electronic, isLeft));
			}
			else
			{
				isPoseValid = (HandManager.Instance != null) &&
					(HandManager.Instance.IsHandPoseValid(HandManager.TrackerType.Natural, isLeft));
			}
			return isPoseValid;
		}

		/// <summary>
		/// Gets the position and rotation of the specified joint.
		/// </summary>
		/// <param name="joint">The joint type to get position and rotation from.</param>
		/// <param name="position">The position of the joint.</param>
		/// <param name="rotation">The rotation of the joint.</param>
		/// <param name="local">Whether to get the local position and rotation.</param>
		/// <returns>True if the joint position and rotation are successfully obtained; otherwise, false.</returns>
		public bool GetJointPositionAndRotation(JointType joint, out Vector3 position, out Quaternion rotation, bool local = false)
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			int jointId = (int)joint;
			if (jointId >= 0 && jointId < BONE_MAX_ID && BonePoses[jointId] != null)
			{
				if (!local)
				{
					position = BonePoses[jointId].position;
					rotation = BonePoses[jointId].rotation;
				}
				else
				{
					position = BonePoses[jointId].localPosition;
					rotation = BonePoses[jointId].localRotation;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Sets the position and rotation of the specified joint.
		/// </summary>
		/// <param name="joint">The joint type to set position and rotation for.</param>
		/// <param name="position">The new position of the joint.</param>
		/// <param name="rotation">The new rotation of the joint.</param>
		/// <param name="local">Whether to set the local position and rotation.</param>
		/// <returns>True if the joint position and rotation are successfully set; otherwise, false.</returns>
		public bool SetJointPositionAndRotation(JointType joint, Vector3 position, Quaternion rotation, bool local = false)
		{
			int jointId = (int)joint;
			if (jointId >= 0 && jointId < BONE_MAX_ID && BonePoses[jointId] != null)
			{
				if (!local)
				{
					BonePoses[jointId].position = position;
					BonePoses[jointId].rotation = rotation;
				}
				else
				{
					BonePoses[jointId].localPosition = position;
					BonePoses[jointId].localRotation = rotation;
				}

				if (joint == JointType.Wrist)
				{
					updatedFrameCount = Time.frameCount;
					updateRoot = true;
				}
				return true;
			}
			return false;
		}

		XR_InteractionMode preInteractionMode = XR_InteractionMode.Default;
		XR_InteractionMode currInteractionMode;

		private bool showECHand = false;
		private bool enableDirectPreview = false;
		private bool hasIMManager = false;

		#region MonoBehaviour

		private void OnEnable()
		{
			if (BonePoses.Length != boneMap.Length)
			{
				Log.d(TAG, HSB.Append("OnEnable() Length of BonePoses is not equal to length of boneMap, skip!"));
				return;
			}

			skinMeshRend = null;
			customizedSkinMeshRend = null;

			ClearDetect();
			AutoDetect();

			BonePoses = customizedBonePoses;
			if (customizedSkinMeshRend != null)
			{
				customizedSkinMeshRend.gameObject.SetActive(true);
				Hand = customizedSkinMeshRend.gameObject;
				Log.d(TAG, HSB.Append("OnEnable() Set Hand to ").Append(Hand.name));
			}

			for (int i = 0; i < boneMap.Length; i++)
			{
				Log.d(TAG, HSB.Append("OnEnable() ").Append(boneMap[i].DisplayName).Append(" --> ").Append(BonePoses[i].name));
			}

			GeneralEvent.Listen(GeneralEvent.INTERACTION_MODE_MANAGER_READY, OnInteractionModeManagerReady);
			OEMConfig.onOEMConfigChanged += OnOEMConfigChanged;

			if (Hand != null) { Hand.SetActive(false); } // hidden in AP start.

			if (m_EnableCollider && m_HandCollider == null)
			{
				InitHandCollider();
			}

			MeshHandPose meshHandPose = transform.gameObject.AddComponent<MeshHandPose>();
			meshHandPose.SetHandMeshRenderer(this);
		}

		private void OnDisable()
		{
			OEMConfig.onOEMConfigChanged -= OnOEMConfigChanged;
			GeneralEvent.Remove(GeneralEvent.INTERACTION_MODE_MANAGER_READY, OnInteractionModeManagerReady);

			if (Hand != null) { Hand = null; }

			if (m_HandCollider != null)
			{
				Destroy(m_HandCollider);
			}

			MeshHandPose meshHandPose = transform.GetComponent<MeshHandPose>();
			if (meshHandPose != null)
			{
				Destroy(meshHandPose);
			}
		}

		// Start is called before the first frame update
		private void Start()
		{
#if UNITY_EDITOR
			enableDirectPreview = EditorPrefs.GetBool("Wave/DirectPreview/EnableDirectPreview");
#endif
		}

		private void Update()
		{
			if (m_UseRuntimeModel && !enableDirectPreview)
			{
				CheckLoadModel();
				CheckMaterial();
			}

			if (Hand == null)
				return;

			bool isFocused = ClientInterface.IsFocused;
			bool isPoseValid = false;
			if (isFocused)
				isPoseValid = IsPoseValid();

			bool showHand = isPoseValid && isFocused;

			if (Log.gpl.Print)
				Log.d(TAG, HSB
					.Append("Pose isValid: ").Append(isPoseValid)
					.Append(", isFocused").Append(isFocused)
					.Append(", showHand: ").Append(showHand)
					.Append(", Interaction Mode: ").Append(hasIMManager));

			if (Hand.activeInHierarchy != showHand)
				Hand.SetActive(showHand);
			if (!showHand)
				return;

			Profiler.BeginSample("UpdateBonePose");
#if UNITY_EDITOR
			if (enableDirectPreview)
#endif
				UpdateBonePose();
			Profiler.EndSample();

			if (showConfidenceAsAlpha)
			{
				float conValue = GetHandConfidence(isLeft);

				if (Log.gpl.Print)
					Log.d(TAG, HSB.Append("Confidence value: ").Append(conValue));

				var color = Hand.GetComponent<Renderer>().material.color;
				color.a = conValue > minAlpha ? conValue : minAlpha;
				Hand.GetComponent<Renderer>().material.color = color;
			}
		}

		#endregion

		private void InitHandCollider()
		{
			m_HandCollider = gameObject.AddComponent<HandColliderController>();
			m_HandCollider.InitJointColliders(BonePoses[rootId]);
			m_HandCollider.handMesh = this;
		}

		private void CheckLoadModel()
		{
			if (!m_UseRuntimeModel || skinMeshRend != null)
			{
				// If already loaded, skinMeshRend will not be null.
				return;
			}

			if (!Interop.WVR_IsDeviceConnected(
					isLeft ?
					WVR_DeviceType.WVR_DeviceType_NaturalHand_Left :
					WVR_DeviceType.WVR_DeviceType_NaturalHand_Right)
				)
			{
				return;
			}

			m_UseRuntimeModel = GetNaturalHandModel();
			Log.d(TAG, HSB.Append("CheckLoadModel() m_UseRuntimeModel: ").Append(m_UseRuntimeModel));
			if (m_UseRuntimeModel)
			{
				if (customizedSkinMeshRend != null)
				{
					customizedSkinMeshRend.gameObject.SetActive(false);
					Log.d(TAG, HSB.Append("CheckLoadModel() disable ").Append(customizedSkinMeshRend.gameObject.name));
				}
				if (skinMeshRend != null)
				{
					skinMeshRend.gameObject.SetActive(true);
					Log.d(TAG, HSB.Append("CheckLoadModel() enable ").Append(skinMeshRend.gameObject.name));
					Hand = skinMeshRend.gameObject;
					Log.d(TAG, HSB.Append("CheckLoadModel() Set Hand to ").Append(Hand.name));
				}

				BonePoses = runtimeBonePoses;
				for (int i = 0; i < boneMap.Length; i++)
				{
					Log.d(TAG, HSB.Append("CheckLoadModel() ").Append(boneMap[i].DisplayName).Append(" --> ").Append(BonePoses[i].name));
				}
			}
		}

		private void OnOEMConfigChanged()
		{
			Log.d(TAG, HSB.Append("OnOEMConfigChanged()"));
			ReadJson();
			SetRuntimeModelMaterialStyle(isHandStable);
		}

		private void OnInteractionModeManagerReady(params object[] args)
		{
			hasIMManager = true;
		}

		private int BoneIDCorrection(int ID)
		{
			if (ID == 47)
			{
				return 0;
			}

			return ID;
		}

		private Vector3 GetPosition(Matrix4x4 matrix)
		{
			var x = matrix.m03;
			var y = matrix.m13;
			var z = matrix.m23;

			return new Vector3(x, y, z);
		}
	}
}
