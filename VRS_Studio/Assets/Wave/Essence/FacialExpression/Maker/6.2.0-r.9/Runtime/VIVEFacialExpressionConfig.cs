// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;
using Wave.OpenXR;

namespace Wave.Essence.FacialExpression.Maker
{
	public class VIVEFacialExpressionConfig : ScriptableObject
	{
		//public GameObject Target;
		public OriginalNodeNew OriginalNodeNew;
		public GeneratedNodeNew GeneratedNodeNew;
	}

	[System.Serializable]
	public class OriginalNodeNew
	{
		public RigNodeNew[] LinkingExpNodes = new RigNodeNew[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];
		private VIVEOriginalExpressionStatus OriginalRigStatus;
		public Rect Position;
	}

	[System.Serializable]
	public class GeneratedNodeNew
	{
		//public List<Node> UnLinkingExpNodes = new List<Node>();
		public List<TrackNodeNew> TrackingExpNodes = new List<TrackNodeNew>();
	}

	[System.Serializable]
	public class RigNodeNew
	{
		public string OriginalName;
		//public string BSName;
		public List<JointGapInfoBS> BlendShapeSets= new List<JointGapInfoBS>();
		public bool NotNull = false;
		//public Rect Position;
		public RigNodeNew(string _NodeName, List<JointGapInfoBS> _Sets/*, Rect _Position*/)
		{
			OriginalName = _NodeName;
			for (int i = 0; i < _Sets.Count; i++)
			{
				BlendShapeSets.Add(_Sets[i]);
			}
			//Position = _Position;
			NotNull = true;
			//Debug.Log("RigNodeNew(6) 999 node name: " );
		}
	}


	[System.Serializable]
	public class TrackNodeInfoBS
	{
		public string Inport;
		//[Range(0.0f, 100.0f)]
		public float Weight;

		public TrackNodeInfoBS(float _wt,string _inport)
		{
			this.Weight = _wt;
			this.Inport = _inport;
		}
	}
	[System.Serializable]
	public class TrackNodeNew
	{
		public string OriginalBSNodeName;
		public List<TrackNodeInfoBS> BlendShapeSets = new List<TrackNodeInfoBS>();
		public bool IsLinking = false;
		public Rect Position;

		public TrackNodeNew(string _NodeName, List<TrackNodeInfoBS> _Sets, Rect _Position, bool _linking)
		{
			OriginalBSNodeName = _NodeName;
			if (_Sets != null)
			{
				for (int i = 0; i < _Sets.Count; i++)
				{
					BlendShapeSets.Add(_Sets[i]);
				}
			}
			Position = _Position;
			IsLinking = _linking;
			//Debug.Log("TrackNodeNew(6) 999 node name: " );
		}
	}


}
