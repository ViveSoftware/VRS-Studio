// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Wave.OpenXR;
using Wave.Native;

namespace Wave.Essence.FacialExpression.Maker
{
	public class VIVEFacialExpressionAdapter : MonoBehaviour
	{
		#region Log
		const string LOG_TAG = "Wave.Essence.FacialExpression.Maker.VIVEFacialExpressionAdapter";
		StringBuilder m_sb = null;
		StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder msg) { Log.d(LOG_TAG, msg, true); }
		int logFrame = -1;
		bool printIntervalLog = false;
		void INFO(StringBuilder msg) { Log.i(LOG_TAG, msg, true); }
		#endregion

		//[SerializeField] VIVEFacialExpressionController MappingConfig0;//Controller
		[SerializeField]
		private VIVEFacialExpressionConfig m_MappingConfig;
		public VIVEFacialExpressionConfig MappingConfig { get { return m_MappingConfig; } set { m_MappingConfig = value; } }
		[SerializeField]
		private GameObject m_TargetFace = null;
		public GameObject TargetFace { get { return m_TargetFace; } set { m_TargetFace = value; } }

		[SerializeField]
		private List<GameObject> m_TargetFaceAppend = new List<GameObject>();
		public List<GameObject> TargetFaceAppend { get { return m_TargetFaceAppend; } set { m_TargetFaceAppend = value; } }

		public bool blendShapeOnly = true;
		public static string targetFaceStr = "";
		public static List<string> targetFaceAppendStr = new List<String>();


		private static float[] eyeExps = new float[(int)InputDeviceEye.Expressions.MAX];
		private static float[] lipExps = new float[(int)InputDeviceLip.Expressions.Max];

		public static List<Transform> ListOfTrans;

		public class ObjSkinnedMeshInfo {
			public string Target { get; set; }
			public int ID { get; set; }
			public string Name { get; set; }
			public float OriginWeight { get; set; }
		}
		static List<List<ObjSkinnedMeshInfo>> ListOfBlendShapes= new List<List<ObjSkinnedMeshInfo>>();
		static List<ObjSkinnedMeshInfo> ListOfBlendShapesSub;//= new List<ObjSkinnedMeshInfo>();
		public static List<ObjSkinnedMeshInfo> ListOfBlendShapesAll= new List<ObjSkinnedMeshInfo>();

		static Transform rootTransform = null;


		void OnValidate() { rootTransform = this.transform; }
		void Reset()
		{
			if (null == rootTransform)
			    rootTransform = this.transform;
		}
        void OnEnable()
        {
            if (null == rootTransform)
                rootTransform = this.transform;
        }
        public static void Prepareblendshapes() {
			ListOfTrans = Get_ListOfTrans(rootTransform);
			List<Transform> Get_ListOfTrans(Transform root)
			{
				List<Transform> _TempList = new List<Transform>();

				_AddJointsInfo(root);

				void _AddJointsInfo(Transform _Joint)
				{
					if (_Joint == null) { return; }
					for (int i = 0; i < _Joint.childCount; i++)
					{
						Transform _TempTrans = _Joint.GetChild(i);
						_AddJointsInfo(_TempTrans);
					}
					if (_Joint.tag != "Dont")
					{
						_TempList.Add(_Joint);
					}
				}
				return _TempList;
			}


			for (int i = 0; i < ListOfTrans.Count; i++)
			{
				ListOfBlendShapesSub = new List<ObjSkinnedMeshInfo>();

				if ((ListOfTrans[i].GetComponent<SkinnedMeshRenderer>() != null) &&
						ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0)
				{
					for (int j = 0; j < ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; j++)
						ListOfBlendShapesSub.Add(new ObjSkinnedMeshInfo { Target = ListOfTrans[i].name, ID = j, Name = ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(j), OriginWeight = ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(j) });

					ListOfBlendShapes.Add(ListOfBlendShapesSub);
				}
			}
			for (int i = 0; i < ListOfBlendShapes.Count; i++)
			{
				List<ObjSkinnedMeshInfo> blendShapesList = ListOfBlendShapes[i];
				for (int j = 0; j < blendShapesList.Count; j++)
				{
					ObjSkinnedMeshInfo info = blendShapesList[j];
					// must collect all blendshape
					ListOfBlendShapesAll.Add(new ObjSkinnedMeshInfo { Target = info.Target, ID = info.ID, Name = info.Name, OriginWeight = info.OriginWeight });
				}
			}
		}


		void Start()
		{
			ListOfTrans = Get_ListOfTrans();
			InputDeviceEye.ActivateEyeExpression(true);
			InputDeviceLip.ActivateLipExp(true);
			sb.Clear().Append("VIVEFacialExpressionMakerAdapter Start() Eye Avilable: ").Append(InputDeviceEye.IsEyeExpressionAvailable()); INFO(sb);
			sb.Clear().Append("VIVEFacialExpressionMakerAdapter Start() Lip Avilable: ").Append(InputDeviceLip.IsLipExpAvailable()); INFO(sb);
			for (int i = 0; i < ListOfTrans.Count; i++)
			{
				ListOfBlendShapesSub = new List<ObjSkinnedMeshInfo>();
				//Debug.Log("Start() ListOfTrans[] name: " + ListOfTrans[i].name+", /i: "+i+", count:"+ ListOfTrans.Count);
				if ((ListOfTrans[i].GetComponent<SkinnedMeshRenderer>() != null) &&
						ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0)
				{
					//Debug.Log("Start() Obj has SkinnedMeshRenderer! " + ListOfTrans[i].name + ", count: " + ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount);
					for (int j = 0; j < ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; j++)
						ListOfBlendShapesSub.Add(new ObjSkinnedMeshInfo { Target = ListOfTrans[i].name, ID = j, Name = ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(j), OriginWeight = ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(j) });

					ListOfBlendShapes.Add(ListOfBlendShapesSub);
				}
			}

			// trace the avatar blendshapes
			for (int i = 0; i < ListOfBlendShapes.Count; i++)
			{
				//Debug.Log("Start() ListOfBlendShapes[{i}] data:===>");
				List<ObjSkinnedMeshInfo> blendShapesList = ListOfBlendShapes[i];
				for (int j = 0; j < blendShapesList.Count; j++)
				{
					//Debug.Log($"Start() i: " + i + ", j: " + j);
					ObjSkinnedMeshInfo info = blendShapesList[j];
					//Debug.Log($"Start() Count: {blendShapesList.Count}, Target: {info.Target}, ID: {info.ID}, Name: {info.Name}");
					// must collect all blendshape
					ListOfBlendShapesAll.Add(new ObjSkinnedMeshInfo { Target = info.Target, ID = info.ID, Name = info.Name , OriginWeight = info.OriginWeight});
				}
			}
			// debug all blendshape information
			//for (int i = 0; i < ListOfBlendShapesAll.Count; i++)
			//	Debug.Log($"Start() BlendShapesAll: " + ListOfBlendShapesAll[i].Target + ", id: " + ListOfBlendShapesAll[i].ID+",name:"+ ListOfBlendShapesAll[i].Name);
		}
		void LogEyeValues()
		{
			if (printIntervalLog)
			{
				for (int i = 0; i < InputDeviceEye.s_EyeExpressions.Length; i++)
				{
					sb.Clear().Append(InputDeviceEye.s_EyeExpressions[i]).Append(": ").Append(eyeExps[i]);
					DEBUG(sb);
				}
			}
		}

		void LogLipValues()
		{
			if (printIntervalLog)
			{
				for (int i = 0; i < InputDeviceLip.s_LipExps.Length; i++)
				{
					sb.Clear().Append(InputDeviceLip.s_LipExps[i]).Append(": ").Append(lipExps[i]);
					DEBUG(sb);
				}
			}
		}

		void Update()
		{
			logFrame++;
			logFrame %= 300;
			printIntervalLog = (logFrame == 0);

			bool hasEyeVal = false, hasLipVal = false;
			if (!InputDeviceEye.IsEyeExpressionAvailable() || !InputDeviceLip.IsLipExpAvailable())
			{
				InputDeviceEye.ActivateEyeExpression(true);
				InputDeviceLip.ActivateLipExp(true);
			}

			//Debug.Log("VIVEFacialExpressionMakerAdapter Update() Eye Avilable: " + InputDeviceEye.IsEyeExpressionAvailable());
			//Debug.Log("VIVEFacialExpressionMakerAdapter Update() Lip Avilable: " + InputDeviceLip.IsLipExpAvailable());
			if (InputDeviceEye.HasEyeExpressionValue())// Eye expressions
			{
				//Debug.Log("VIVEFacialExpressionMakerAdapter Update(6) 999 has eye expression");
				hasEyeVal = InputDeviceEye.GetEyeExpressionValues(out float[] exps);
                if (hasEyeVal)
				{
					eyeExps = exps;
					LogEyeValues();
				}
			}
			//else { Debug.Log("Update(6) 999 not has eye expression"); }


			if (InputDeviceLip.HasLipExpValue())// Lip expressions
			{
				//Debug.Log("VIVEFacialExpressionMakerAdapter Update(6) 999 has lip expression");
				hasLipVal = InputDeviceLip.GetLipExpValues(out float[] exps);
                if (hasLipVal)
				{
					lipExps = exps;
					LogLipValues();
				}
			}
			//else { Debug.Log("Update(6) 999 not has lip expression"); }

            if ((InputDeviceEye.IsEyeExpressionAvailable() ) || (InputDeviceLip.IsLipExpAvailable() ))
                LerModel3();

		}


		void LerModel3()
		{
			//Debug.Log("VIVEFacialExpressionMakerAdapter LerModel3(FaceObject) has BlendShapeSets:"+TargetFace.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount+", name:"+ TargetFace.name);
            if ((TargetFace.GetComponent<SkinnedMeshRenderer>() != null) && TargetFace.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0)
            {
                for (int j = 0; j < TargetFace.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; j++)
                    TargetFace.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(j, 0.0f);
            }
			for (int n = 0; n < TargetFaceAppend.Count; n++)
			{
                for (int m = 0; m < TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; m++)
                    TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(m, 0.0f);

            }

            //List <JointInfo> originalJointInfos = MappingConfig.OriginalNodeNew.OriginalRigStatus.JointInfos;
            for (int i = 0; i < ListOfTrans.Count; i++) //111 objects
			{
				if (ListOfTrans[i] == null) { continue; }
                //ListOfTrans[i].localPosition = this.transform.localPosition;//originalJointInfos[i].LocalPos;
                //ListOfTrans[i].localRotation = this.transform.localRotation;//Quaternion.Euler(originalJointInfos[i].LocalRotation);
                //ListOfTrans[i].localScale = this.transform.localScale;//originalJointInfos[i].LocalScale;

                //Debug.Log("VIVEFacialExpressionMakerAdapter LerModel3(FaceObject) count:" + ListOfTrans.Count);
                //ListOfBlendShapesSub = new List<ObjSkinnedMeshInfo>();
                //if ((ListOfTrans[i].GetComponent<SkinnedMeshRenderer>() != null) && ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0)
				//{
				//	for (int j = 0; j < ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; j++)
				//		ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(j, 0.0f);
				//}

            }



			for (int j = 0; j < MappingConfig.OriginalNodeNew.LinkingExpNodes.Length; j++) //51 wave
			{
				//Debug.Log("LerModel3(FaceObject) has BlendShapeSets51:" + j + ", target: " + TargetFace.name.ToString() + ", WaveOriLinkExp:" + MappingConfig.OriginalNodeNew.LinkingExpNodes.Length);
				if (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count > 0)
				{
					for (int k = 0; k < MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count; k++)
					{
						   //Debug.Log("LerModel3() has BlendShapeSets51:" + k + ", BSName:" + MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName + ", count: " + MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count);
						if ((j < eyeExps.Length) && (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight > 0))
						{
							var result = from r in ListOfBlendShapesAll where (r.Target == TargetFace.name.ToString() && r.Name == MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName) select r;
							if (result.Any())
							{
								//Debug.Log("LerModel3() Eye target: " + result.First().Target + " ,BS: " + result.First().Name + ", ID" + result.First().ID);
								//GameObject.Find(TargetFace.name.ToString()).GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSId, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * eyeExps[j]);
								if(TargetFace.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID)>0.0f)
                                    TargetFace.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, TargetFace.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID)  + (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * eyeExps[j]));
								else
                                    TargetFace.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * eyeExps[j]);
							}
						}
						else if ((MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight > 0))
						{
							int lipIdx = j - eyeExps.Length;
							var result = from r in ListOfBlendShapesAll where (r.Target == TargetFace.name.ToString() && r.Name == MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName) select r;
							if (result.Any())
							{
								//Debug.Log("LerModel3() Lip target: " + result.First().Target + " ,BS: " + result.First().Name + ", ID" + result.First().ID);
								//GameObject.Find(TargetFaceAppend[n].name.ToString()).GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSId, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * lipExps[lipIdx]);
								if(TargetFace.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID)>0.0f)
                                    TargetFace.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, TargetFace.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID) + (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * lipExps[lipIdx]));
								else
                                    TargetFace.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * lipExps[lipIdx]);
							}
						}
					}
				}
			}


			for (int n = 0; n < TargetFaceAppend.Count; n++)
			{
				//Debug.Log("LerModel3(Additional) has BlendShapeSets51: count:" + TargetFaceAppend.Count + ", targetAdd: " + TargetFaceAppend[n].name.ToString()+", eyeLeng:"+ eyeExps.Length);
				//if (ListOfTrans[i].name.ToString().Equals(TargetFaceAppend[n].name.ToString()))
				{
			    for (int j = 0; j < MappingConfig.OriginalNodeNew.LinkingExpNodes.Length; j++) //51 wave
			    {
				    //Debug.Log("LerModel3(Additional) has BlendShapeSets51(j):" + j + ", targetAdd: " + TargetFaceAppend[n].name.ToString() + ", count: " + TargetFaceAppend.Count);
				    if (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count > 0)
					{
							for (int k = 0; k < MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count; k++)
							{
								//Debug.Log("LerModel3() has BlendShapeSets51(k):" + k + ", BSName:" + MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName + ", count: " + MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets.Count);
								if ((j < eyeExps.Length) && (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight > 0))
								{
									var result = from r in ListOfBlendShapesAll where (r.Target == TargetFaceAppend[n].name.ToString() && r.Name == MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName) select r;
									if (result.Any())
									{
										//Debug.Log("LerModel3() Eye targetAdd: " + result.First().Target + " ,BS: " + result.First().Name + ", ID" + result.First().ID);
										if(TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID) > 0.0f)
                                            TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID) + (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * eyeExps[j]));
										else
                                            TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * eyeExps[j]);
									}
								}
								else if ((MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight > 0))
								{
									int lipIdx = j - eyeExps.Length;
									var result = from r in ListOfBlendShapesAll where (r.Target == TargetFaceAppend[n].name.ToString() && r.Name == MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].BSName) select r;
									if (result.Any())
									{
										//Debug.Log("LerModel3() Lip targetAdd: " + result.First().Target + " ,BS: " + result.First().Name + ", ID" + result.First().ID);
										if (TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID) > 0.0f)
                                            TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(result.First().ID) + (MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * lipExps[lipIdx]));
										else
                                            TargetFaceAppend[n].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(result.First().ID, MappingConfig.OriginalNodeNew.LinkingExpNodes[j].BlendShapeSets[k].Weight * lipExps[lipIdx]);
									}
								}
							}
					}
				}
				}
			}


		}

		List<Transform> Get_ListOfTrans()
		{
			List<Transform> _TempList = new List<Transform>();

			_AddJointsInfo(transform);

			void _AddJointsInfo(Transform _Joint)
			{
				if (_Joint == null) { return; }
				for (int i = 0; i < _Joint.childCount; i++)
				{
					Transform _TempTrans = _Joint.GetChild(i);
					_AddJointsInfo(_TempTrans);
				}
				if (_Joint.tag != "Dont")
				{
					_TempList.Add(_Joint);
				}
			}
			return _TempList;
		}
	}
}
