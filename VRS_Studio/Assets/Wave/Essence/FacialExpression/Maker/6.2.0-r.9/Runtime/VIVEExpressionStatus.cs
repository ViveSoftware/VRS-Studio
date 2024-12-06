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

namespace Wave.Essence.FacialExpression.Maker
{
	[System.Serializable]
	public class JointGapInfoBS
	{
		public string BSName;
		//[Range(0.0f, 100.0f)]
		public float Weight;
		public int BSId;
		public string Inport;
		public Rect poistion;
		public JointGapInfoBS(string _name, float _wt, int _id, string _inport, Rect _pose)
		{
			this.BSName = _name;
			this.Weight = _wt;
			this.BSId = _id;
			this.Inport = _inport;
			this.poistion = _pose;
		}
	}

	public class VIVEExpressionStatus : ScriptableObject
    {
        public List<JointGapInfo> JointGapInfos;


		[System.Serializable]
        public class JointGapInfo
        {
            public string Name;
            public int Index;
            public Vector3 Pos;
            public Vector3 Rotation;
            public Vector3 Scale;
            //public List<float> BlendShapes = new List<float>();
			public List<JointGapInfoBS> BlendShapeSets = new List<JointGapInfoBS>();

			public JointGapInfo(string _Name, Vector3 _Pos, Vector3 _Rotation, Vector3 _Scale, List<float> _BlendShapes)
            {
				//Debug.Log("JointGapInfo(5) 999: " + _Name+ ",count: "+ _BlendShapes.Count);
				Name = _Name;
                Pos = _Pos;
                Rotation = _Rotation;
                Scale = _Scale;
                for (int i = 0; i < _BlendShapes.Count; i++)
                {
                    //BlendShapes.Add(_BlendShapes[i]);
                }
            }
			public JointGapInfo(string _Name, int _Index, Vector3 _Pos, Vector3 _Rotation, Vector3 _Scale, List<JointGapInfoBS> _BlendShapeSets)
			{
				//Debug.Log("JointGapInfo(6) 999: " + _Name + ",count: " + _BlendShapeSets.Count+", Idx"+ _Index);
				Name = _Name;
				Index = _Index;
				Pos = _Pos;
				Rotation = _Rotation;
				Scale = _Scale;
				for (int i = 0; i < _BlendShapeSets.Count; i++)
				{
					BlendShapeSets.Add(_BlendShapeSets[i]);//BlendShapes.Add(_BlendShapeSets[i]);
				}
			}


			public JointGapInfo(string _Name, int _Index, Vector3 _Pos, Vector3 _Rotation, Vector3 _Scale, List<float> _BlendShapes)
            {
				//Debug.Log("JointGapInfo(7) 999: " + _Name +",Idx: "+ _Index + ",count: " + _BlendShapes.Count);
				Name = _Name;
                Index = _Index;
                Pos = _Pos;
                Rotation = _Rotation;
                Scale = _Scale;
                for (int i = 0; i < _BlendShapes.Count; i++)
                {
                    //BlendShapes.Add(_BlendShapes[i]);
                }
            }
        }
    }


}
