// "Wave SDK
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;
using Wave.OpenXR;

namespace Wave.Essence.FacialExpression.Maker
{
	public class VIVEFacialExpressionController : ScriptableObject
    {
        //public GameObject Target;
        public OriginalNode OriginalNode;
    }

	[System.Serializable]
    public class OriginalNode
    {
        public RigNode[] LinkingExpNodes = new RigNode[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];
		public VIVEOriginalExpressionStatus OriginalRigStatus;
        public Rect Position;
    }

	[System.Serializable]
    public class RigNode
    {
		public string OriginalName;
		public string NodeName;
        public VIVEExpressionStatus ExpStatus; //custom facial
        public bool NotNull = false;
        public Rect Position;
        public RigNode(string _NodeName, VIVEExpressionStatus _RigStatus, Rect _Position)
        {
            NodeName = _NodeName;
            ExpStatus = _RigStatus;
            Position = _Position;
            NotNull = true;
            //Debug.Log("RigNode(6) 999 node name: "+_NodeName);
        }
    }


}
