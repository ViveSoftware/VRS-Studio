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
using UnityEngine.UIElements;
using System.Collections.Generic;
using Wave.OpenXR;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace Wave.Essence.FacialExpression.Maker
{
    public class VIVEFacialTrackingGraphNode : Node
    {
        public string GUID = "";
        public bool ExtryPoint = false;
        //public Rect Position;
    }

    public class OriginalStateNode : VIVEFacialTrackingGraphNode
    {
        private VIVEOriginalExpressionStatus OriginalRigStatus { get; set; }
        public ObjectField TokenForOriginalRigStatus { get; set; }
        public ObjectField TokenForTarget { get; set; }
        public GameObject Target { get; set; }
        public Port[] OutPutPorts { get; set; }
		public GameObject TargetFace { get; set; }
		public ObjectField TokenForTargetFace { get; set; }

		public OriginalStateNode()
        {
            OutPutPorts = new Port[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];
        }
    }

    public class ExpressionStateNode : VIVEFacialTrackingGraphNode
    {
        public string NodeName { get; set; }
        public Label NameLabel { get; set; }

		public VIVEExpressionStatus ExpStatus { get; set; }
        public Port InputPort { get; set; }
        public ObjectField TokenForRigStatus { get; set; }
        public bool Is_Editting = false;
        public Button EditBtn { get; set; }
		//public Button EditBtn2 { get; set; }
		public Port[] InputPortArray { get; set; }
		public Rect position { get; set; }

		public int BlendShapeId { get; set; }
		public List<Label> NameLabelSet { get; set; }
		public List<JointGapInfoBS> BSWeight { get; set; }
		public System.Collections.Generic.List<Port> InPorts { get; set; }

		public ExpressionStateNode()
		{
			//InputPortArray = new Port[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];

			NameLabelSet = new List<Label>();
			BSWeight = new List<JointGapInfoBS>();
			InPorts = new System.Collections.Generic.List<Port>();
		}
	}

	public class ExpressionStateNodeOriginal : VIVEFacialTrackingGraphNode
	{
		public string NodeName { get; set; }
		public Label NameLabel { get; set; }
		public bool Is_Editting = false;
		public ObjectField TokenForOriginalRigStatus { get; set; }
		public ObjectField TokenForTarget { get; set; }
		public GameObject Target { get; set; }
		public Port[] OutPutPorts { get; set; }
		public GameObject TargetFace { get; set; }
		public ObjectField TokenForTargetFace { get; set; }

		public ExpressionStateNodeOriginal()
		{
			OutPutPorts = new Port[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];
		}
	}

	public class CustomStateNodeMapping : VIVEFacialTrackingGraphNode
	{
		public string NodePortName { get; set; }
		public VIVEExpressionStatus ExpStatus { get; set; }
		public ObjectField TokenForRigStatus { get; set; }
	}

	public class LinkEdgeData //: ScriptableObject
	{
		public Node OutNode { get; set; }
		public ExpressionStateNode InNode { get; set; }
		public Port OutPort { get; set; }
		public Port InPort { get; set; }
		public string InPortName { get; set; }
		public float weight { get; set; }
		public Rect position { get; set; }

		public LinkEdgeData(Node _Onode, ExpressionStateNode _Inode, Port _Oport, Port _iPort, string _name, float _val, Rect _pose) {
			OutNode = _Onode;
			InNode = _Inode;
			OutPort = _Oport;
			InPort = _iPort;
			InPortName = _name;
			weight = _val;
			position = _pose;
		}
	}
	public class CustomNode : Node
	{
		public CustomNode()
		{
			title = "Custom Node";
			Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
			outputPort.portName = "Output Port";
			outputContainer.Add(outputPort);
			SetPosition(new Rect(100, 100, 200, 150));
		}
	}
	/*public class ExpressionStateNodeMulti : VIVEFacialTrackingGraphNode
	{
		public string NodeName { get; set; }
		//public Label NameLabel { get; set; }
		public int BlendShapeId { get; set; }
		public System.Collections.Generic.List<float> BSWeight { get; set; }
		//public Button EditBtn { get; set; }
		//public Button EditBtn2 { get; set; }
		public System.Collections.Generic.List<Port> InPorts { get; set; }
		//public Port[] InPorts { get; set; }

		public ExpressionStateNodeMulti()
		{
			BSWeight = new System.Collections.Generic.List<float>();
			InPorts = new System.Collections.Generic.List<Port>();
			//InPorts = new Port[(int)InputDeviceEye.Expressions.MAX + (int)InputDeviceLip.Expressions.Max];
		}
	}*/
	public class weightDataSet {
		string portName;
		float weight;
		public weightDataSet(string _name, float _weight)
		{
			portName = _name;
			weight = _weight;
		}
	}
	public class ExpressionStateNodeReserve : VIVEFacialTrackingGraphNode
	{
		public string name;

		//public List<float> weightList = new List<float>();
		//public Dictionary<float, string> weightList = new Dictionary<float, string>() { };
		public Dictionary<string, float> weightList = new Dictionary<string, float>() { };
		//public List<weightDataSet> weightList = new List<weightDataSet>() { };
		//public List<string> portName = new List<string>() { };
		//public List<float> weight = new List<float>() { };

		public int id;
		public Rect position;
		public void AddWeight(float newWeight, string name, int _portName)
		{
			if (!weightList.ContainsKey("Element" + _portName.ToString()))
				weightList.Add("Element" + _portName.ToString(), newWeight );
		}

		public void AddWeightByName(string targetName, float newWeight, int _portName)
		{
			if (name == targetName)
			{
				AddWeight(newWeight, targetName, _portName);
			}
		}

		//public void AddWeight(float newWeight, int _portName)
		//{
		//	//if (!weightList.ContainsKey(name))
		//	//	weightList.Add(name, newWeight);

		//	portName.Insert(_portName, "Element" + portName.ToString());
		//	weight.Insert(_portName, newWeight);
		//}
		//public void AddWeightByName(string targetName, float newWeight, int _portName)
		//{
		//	if (BSName == targetName)
		//	{
		//		//AddWeight(newWeight, portName);
		//		//weightList.Insert(portName, new weightDataSet("Element" + portName, newWeight ));

		//		portName.Insert(_portName, "Element" + portName.ToString());
		//		weight.Insert(_portName, newWeight);

		//	}
		//}


	}

}
#endif
