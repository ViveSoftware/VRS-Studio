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
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Wave.OpenXR;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace Wave.Essence.FacialExpression.Maker
{
#if UNITY_2020_3_OR_NEWER
	public class VIVEFacialTrackingGraphView : GraphView
	{
		readonly Vector2 DefaultNodeSize = new Vector2(150, 200);
		//public GameObject FacialState { get; set; }
		ContentDragger Dragger;
		public Rect WindowRect;
		//public Blackboard BB;
		//public OriginalStateNode OriginalNode0;
		public OriginalStateNode OriginalNode;
		public ExpressionStateNode CustomNode;
		public List<ObjectField> Holders = new List<ObjectField>();


		public VIVEFacialTrackingGraphView()
		{
			//Debug.Log("VIVEFacialTrackingGraphView() constructor");
			//styleSheets.Add(styleSheet: Resources.Load<StyleSheet>("VIVEFacialTrackingGraph/VIVEFacialTrackingGraphStyle"));
			SetupZoom(ContentZoomer.DefaultMinScale * 1.5f, ContentZoomer.DefaultMaxScale * 1.5f);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());

			//---BackGround
			GridBackground _Grid = new GridBackground();
			Insert(0, _Grid);
			_Grid.StretchToParentSize();
			//---BackGround

		}
		public void onDisable()
	    {
		    //Debug.Log("VIVEFacialTrackingGraphView onDisable()");
	    }

		public void OnDrop(GraphView graphView, Edge edge)
		{
			//Debug.Log("OnDrop() 0");

			if (graphView == VIVEFacialTrackingGraph.FTGraphView)
			{
				//Debug.Log("OnDrop() 1");
				if (edge.output.node.GetType() == typeof(ExpressionStateNode))
				{
					//Debug.Log("OnDrop() 2");
					foreach (Edge _Edge in edge.input.connections.ToList())
					{
						if (_Edge.output.node.GetType() == typeof(ExpressionStateNode) && _Edge != edge)
						{
							_Edge.output.Disconnect(_Edge);
						}
					}
				}
				//else if ((edge.output.node).GetType() == typeof(BlendShapeStateNode))
				//{
				//    foreach (Edge _Edge in edge.input.connections.ToList())
				//    {
				//        if (_Edge.output.GetType() == typeof(BlendShapeStateNode) && _Edge != edge)
				//        {
				//            _Edge.output.Disconnect(_Edge);
				//        }
				//    }
				//}
				else if ((edge.input.node).GetType() == typeof(ExpressionStateNode))
				{
					foreach (Edge _Edge in edge.output.connections.ToList())
					{
						if (_Edge.input.node.GetType() == typeof(ExpressionStateNode) && _Edge != edge)
						{
							_Edge.input.Disconnect(_Edge);
						}
					}
				}
				//else if ((edge.input.node).GetType() == typeof(BlendShapeStateNode))
				//{
				//    foreach (Edge _Edge in edge.output.connections.ToList())
				//    {
				//        if (_Edge.input.GetType() == typeof(BlendShapeStateNode) && _Edge != edge)
				//        {
				//            _Edge.input.Disconnect(_Edge);
				//        }
				//    }
				//}
			}
		}



		public VIVEFacialTrackingGraphNode GenerateExpressionStateNodeCustomNew(GameObject targetFace, int id)
		{
			int i = 0, j=0;
			ExpressionStateNode _Node = new ExpressionStateNode()
			{
				title = (GameObject.Find(targetFace.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(id)).ToString(), //"Custom Expression State",
				GUID = "CustomNode",//Guid.NewGuid().ToString(),
				ExtryPoint = true,
			};
			_Node.NodeName = (GameObject.Find(targetFace.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(id)).ToString();
			_Node.BlendShapeId = id;
			_Node.RegisterCallback<MouseDownEvent>(evt =>{
				//Debug.Log("Window Pos: " + _Node.GetPosition());
				//Debug.Log("GenerateExpressionStateNode(670) Node.RegisterCallback name:" + _Node.NodeName);
			});
			CustomNode = _Node;

			//---Generate Add Button
			Button _Button = new Button(clickEvent: () =>
			{
				//Debug.Log("GenerateExpressionStateNode() button Add element: " + i);
				Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
				_Port.allowMultiDrag = true;
				_Port.style.height = 30;
				_Node.InPorts.Add(_Port);
				_Node.inputContainer.Add(_Port);

				//------Generate Label
				Label _Label = new Label();
				_Label.name = _Port.portName;
				_Label.text = "Weight:100";
				//_Node.BSWeight.Insert(i, 0.0f);
				_Node.NameLabelSet.Add(_Label);
				_Label.RegisterValueChangedCallback(ValueTuple =>{ //get label text as node name need 1 first blank
				});
				_Node.outputContainer.Add(_Label);
				//------Generate Label

				//----Slider for float value
				float val = 100f;
				var slider = new Slider(0, 100, SliderDirection.Horizontal);
				slider.value = 100;
				slider.RegisterValueChangedCallback(evt =>
				{
                    slider.value = Mathf.RoundToInt(evt.newValue);
                    val = Mathf.RoundToInt(evt.newValue);
                    _Label.text = "Weight:" + val;

					//var result = from r in _Node.BSWeight where r.BSName == ("Element"+i) select r;
					//result.First().Weight = val;
					//Debug.Log("GenerateExpressionStateNode() slider CB:" + result.First().Weight);
				});
				//_Node.BSWeight.Add(val);
				_Node.outputContainer.Add(slider);
				//----Slider for float value
				_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i, _Node.position));
				j =j+2;i++;
			});
			_Button.text = "Add";
			//_Node.EditBtn = _Button;
			_Node.titleContainer.Add(_Button);
			//---Generate Add Button

			//---Generate Delete Button
			Button _ButtonDel = new Button(clickEvent: () =>
			{
				_Node.inputContainer.RemoveAt(i-1);
				_Node.outputContainer.RemoveAt(j-1);
				_Node.outputContainer.RemoveAt(j-2);
				_Node.BSWeight.RemoveAt(i - 1);
				j = j - 2;
				i--;
			});
			_ButtonDel.text = "Del";
			//_Node.EditBtn2 = _ButtonDel;
			_Node.titleContainer.Add(_ButtonDel);
			//---Generate Delete Button

			_Node.inputContainer.style.width = 25;
			//---Generate Ports for diferent States
			Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
			_Port.allowMultiDrag = true;
			_Port.style.height = 30;
			_Node.InPorts.Add(_Port);
			_Node.inputContainer.Add(_Port);
			//---Generate Ports for diferent States

			//------Generate Label
			Label _Label = new Label();
			_Label.name = _Port.portName;
			_Label.text = "Weight:100";
			//_Node.BSWeight.Insert(i, 100f);
			_Node.NameLabelSet.Add(_Label);
			_Label.RegisterValueChangedCallback(ValueTuple =>{ //get label text as node name need 1 first blank
            });
			//_Element.Add(_Label);
			_Node.outputContainer.Add(_Label);
			//------Generate Label

			//----Slider for float value
			float val = 100f;
			var slider = new Slider(0, 100, SliderDirection.Horizontal);
			slider.value = val;
			slider.RegisterValueChangedCallback(evt =>
			{
                slider.value = Mathf.RoundToInt(evt.newValue);
                val = Mathf.RoundToInt(evt.newValue);
                _Label.text = "Weight:" + val;

				//var result = from r in _Node.BSWeight where r.BSName == "Element1" select r;
				//result.First().Weight = val;
				//Debug.Log("GenerateExpressionStateNode() slider CB:" + _Node.NameLabelSet[0].name+"Val:"+ _Node.NameLabelSet[0].text);
			});
			//_Node.BSWeight.Add(val);
			_Node.outputContainer.Add(slider);
			//----Slider for float value

			Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero * 500);
			//_TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero);
			//Debug.Log("Window Pos: " + (VIVEFacialTrackingGraph.FTWindow.position.position + Vector2.right * Screen.width) + ", x:" + _TempVec.x + ",y:" + _TempVec.y + ",size x:" + DefaultNodeSize.x + ", size y:" + DefaultNodeSize.y);
			if (id > 48)
				_Node.SetPosition(new Rect(_TempVec.x + 750 + 400, _TempVec.y - 10 + (id - 48) * 40, DefaultNodeSize.x, DefaultNodeSize.y));
			else
				_Node.SetPosition(new Rect(_TempVec.x + 750, _TempVec.y - 10 + id * 40, DefaultNodeSize.x, DefaultNodeSize.y));
			//_Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTGraphView.WindowRect.x, VIVEFacialTrackingGraph.FTGraphView.WindowRect.y, DefaultNodeSize.x, DefaultNodeSize.y));

			/*JointGapInfoBS bs =*/
			_Node.BSWeight.Add(new JointGapInfoBS("Element"+i, 100f, id, "Element" + i, _Node.position));
			j =j+2;i++;



			_Node.RefreshExpandedState();
			_Node.RefreshPorts();

			_Node.style.width = new Length(240, LengthUnit.Pixel);
			//_Node.selected = true;

			//ExpStateNodeData _RigNodeData = new ExpStateNodeData(_Node, _Label, _Field);
			//ListofRigStateNodeData.Add(_RigNodeData);
			return _Node;
		}

		public VIVEFacialTrackingGraphNode GenerateOriginalStateNodeNew()
		{
			OriginalStateNode _Node = new OriginalStateNode
			{
				title = "Original Expression State",
				GUID = Guid.NewGuid().ToString(),
				ExtryPoint = true,
			};
			_Node.capabilities &= ~Capabilities.Deletable;
            OriginalNode = _Node;
			_Node.RegisterCallback<MouseDownEvent>(evt =>
			{
				//SetTargetToOriginalStatus();
			});

			Button _Button;
			////---Generate Set Button
			//_Button = new Button(clickEvent: () =>
			//{
			//	if (_Node.Target == null)
			//	{
			//		Debug.LogError("There is no target!");
			//		return;
			//	}
			//	else if (_Node.OriginalRigStatus == null)
			//	{
			//		_Node.OriginalRigStatus = CreateAsset<VIVEOriginalExpressionStatus>(_AssetName: "FT_mapping_blendshape");//OriginalExpressionStatus
			//		Debug.Log(_Node.OriginalRigStatus);
			//		_Node.TokenForOriginalRigStatus.value = _Node.OriginalRigStatus;
			//	}
			//	SetOriginalExpressionStatus(_Node.Target, _Node.OriginalRigStatus);

			//});
			//_Button.text = "1.Generate Joint Asset";
			//_Button.style.marginRight = 320;
			//_Node.titleContainer.Insert(1, _Button);
			////---Generate Set Button

			////---Add Label
			//Label _Label = new Label();
			//_Label.text = "Target GamrObject";
			//_Label.style.marginLeft = 25;
			//_Label.style.marginTop = 10;
			//_Node.inputContainer.Add(_Label);
			////---Add Label
			ObjectField _ObjField;
			//------Genetate Holder for the Face Model GameObject
			_ObjField = new ObjectField
			{
				objectType = typeof(GameObject),
				allowSceneObjects = true,
			};
			//_ObjField.style.marginLeft = 25;
			//_ObjField.style.width = new Length(220, LengthUnit.Pixel);
			//_ObjField.RegisterValueChangedCallback(ValueTuple =>
			//{
			//	_Node.Target = ValueTuple.newValue as GameObject;
			//});
			////_ObjField.SetEnabled(false);
			//_Node.inputContainer.Add(_ObjField);
			_Node.TokenForTarget = _ObjField;
			//Holders.Add(_ObjField);
			////------Genetate Holder for the Face Model GameObject


			////------Genetate Token for OriginalRigStatus
			//_ObjField = new ObjectField
			//{
			//	objectType = typeof(VIVEOriginalExpressionStatus),
			//	allowSceneObjects = false,
			//};
			//_ObjField.style.marginLeft = 25;
			//_ObjField.style.width = new Length(220, LengthUnit.Pixel);
			//_ObjField.allowSceneObjects = true;
			//_ObjField.RegisterValueChangedCallback(ValueTuple =>
			//{
			//	_Node.OriginalRigStatus = ValueTuple.newValue as VIVEOriginalExpressionStatus;
			//});
			_Node.TokenForOriginalRigStatus = _ObjField;
			//_Node.inputContainer.Add(_ObjField);
			//Holders.Add(_ObjField);


			////---Add Label Face
			//Label _LabelFace = new Label();
			//_LabelFace.text = "Target Face Object";
			//_LabelFace.style.marginLeft = 25;
			//_LabelFace.style.marginTop = 10;
			//_Node.inputContainer.Add(_LabelFace);
			////------Genetate Holder for the Face Model GameObject
			//_ObjField = new ObjectField
			//{
			//	objectType = typeof(GameObject),
			//	allowSceneObjects = true,
			//};
			//_ObjField.style.marginLeft = 25;
			//_ObjField.style.width = new Length(220, LengthUnit.Pixel);
			//_ObjField.RegisterValueChangedCallback(ValueTuple =>
			//{
			//	_Node.TargetFace = ValueTuple.newValue as GameObject;
			//});
			////_ObjField.SetEnabled(false);
			//_Node.inputContainer.Add(_ObjField);
			_Node.TokenForTargetFace = _ObjField;
			//Holders.Add(_ObjField);
			////------Genetate Holder for the Face Model GameObject


			_GeneratePorts();

			_Node.outputContainer.style.alignContent = Align.Center;
			_Node.tooltip = "Hint for the Original State Node";
			_Node.RefreshExpandedState();
			_Node.RefreshPorts();
			Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero * 500);
			_Node.SetPosition(new Rect(_TempVec.x, _TempVec.y, DefaultNodeSize.x, DefaultNodeSize.y));
			_Node.style.width = new Length(400, LengthUnit.Pixel);
			return _Node;

			void _GeneratePorts()
			{
				Port _Port;
				Image _Img = new Image();
				for (int i = 0; i < (int)InputDeviceEye.Expressions.MAX; i++)
				{
					//_Port = GeneratePort(node0, Direction.Output, _Capacity: Port.Capacity.Multi, _PortName: ((InputDeviceEye.Expressions)i).ToString());
					_Port = _Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
					_Port.portName = ((InputDeviceEye.Expressions)i).ToString();
					_Port.allowMultiDrag = true;
					_Node.outputContainer.Add(_Port);
					_Node.OutPutPorts[i] = _Port;
					_Img = new Image();
					var imgPath = FindHintPic(i, isEye: true);
					if (!string.IsNullOrEmpty(imgPath))
						_Img.image = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
					_Node.outputContainer.Add(_Img);
                    _Node.outputContainer.style.alignItems = Align.FlexEnd;
                    ListofOriginalStateData.Add(new OriginalStatePortData(_Port, _Img));
				}
				for (int i = 0; i < (int)InputDeviceLip.Expressions.Max; i++)
				{
					//_Port = GeneratePort(node0, Direction.Output, _Capacity: Port.Capacity.Multi, _PortName: ((InputDeviceLip.Expressions)i).ToString());
					_Port = _Node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
					_Port.portName = ((InputDeviceLip.Expressions)i).ToString();
					_Port.allowMultiDrag = true;
					_Node.outputContainer.Add(_Port);
					_Node.OutPutPorts[(int)InputDeviceEye.Expressions.MAX + i] = _Port;
					_Img = new Image();
					var imgPath = FindHintPic(i, isEye: false);
					if (!string.IsNullOrEmpty(imgPath))
						_Img.image = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
					_Node.outputContainer.Add(_Img);
                    _Node.outputContainer.style.alignItems = Align.FlexEnd;
                    ListofOriginalStateData.Add(new OriginalStatePortData(_Port, _Img));
				}
			}
		}

		public VIVEFacialTrackingGraphNode GenerateOriginalStateNode()
        {
			//OriginalStateNode _Node = new OriginalStateNode
			ExpressionStateNodeOriginal _Node = new ExpressionStateNodeOriginal
			{
                title = "Original Expression State",
                GUID = Guid.NewGuid().ToString(),
                ExtryPoint = true,
            };
            //OriginalNode = _Node;

			_Node.RegisterCallback<MouseDownEvent>(evt =>
			{
				//SetTargetToOriginalStatus();
			});
			Button _Button;
			//---Generate Switch Button
			//_Button = new Button(clickEvent: () => { SwitchUsingMode(); });
			//_Button.text = "Switch Mode";
			//_Node.titleButtonContainer.Add(_Button);
			//---Generate Switch Button

#if false
			//---Generate Set Button
			_Button = new Button(clickEvent: () =>
			{
				if (_Node.Target == null)
				{
					Debug.LogError("There is no target!");
					return;
				}
				else if (_Node.OriginalRigStatus == null)
				{
					_Node.OriginalRigStatus = CreateAsset<VIVEOriginalExpressionStatus>(_AssetName: "FT_mapping_blendshape");//OriginalExpressionStatus
																															 //Debug.Log(_Node.OriginalRigStatus);
					_Node.TokenForOriginalRigStatus.value = _Node.OriginalRigStatus;
				}
				SetOriginalExpressionStatus(_Node.Target, _Node.OriginalRigStatus);
				//create custom expression(each blend shape) asset file
				//for (int i = 0; i < _Node.OriginalRigStatus.JointInfos.Count; i++)
				{
					////if (GameObject.Find(_Node.OriginalRigStatus.JointInfos[i].Name).GetComponent<SkinnedMeshRenderer>() != null)
					//if (_Node.OriginalRigStatus.JointInfos[i].Name.ToString().Equals(_Node.TargetFace.name))
					//GenerateCustomExpressionStatus(_Node.TargetFace/*_Node.OriginalRigStatus.JointInfos[i].Name*/, i, _Node.OriginalRigStatus.JointInfos);
				}
			});
			_Button.text = "1.Generate";
			_Button.style.marginRight = 320;
			_Node.titleContainer.Insert(1, _Button);
			//---Generate Set Button
#endif

			//---Add Label
			Label _Label = new Label();
            _Label.text = "Target GamrObject";
            _Label.style.marginLeft = 25;
            _Label.style.marginTop = 10;
            _Node.inputContainer.Add(_Label);
			//---Add Label
			ObjectField _ObjField;
			//------Genetate Holder for the Face Model GameObject
			_ObjField = new ObjectField
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
            };
            _ObjField.style.marginLeft = 25;
            _ObjField.style.width = new Length(220, LengthUnit.Pixel);
            _ObjField.RegisterValueChangedCallback(ValueTuple =>
            {
                _Node.Target = ValueTuple.newValue as GameObject;
            });
            //_ObjField.SetEnabled(false);
            _Node.inputContainer.Add(_ObjField);
            _Node.TokenForTarget = _ObjField;
            Holders.Add(_ObjField);


            //------Genetate Token for OriginalRigStatus
            _ObjField = new ObjectField
            {
                objectType = typeof(VIVEOriginalExpressionStatus),
                allowSceneObjects = false,
            };
            _ObjField.style.marginLeft = 25;
            _ObjField.style.width = new Length(220, LengthUnit.Pixel);
            _ObjField.allowSceneObjects = true;
            _ObjField.RegisterValueChangedCallback(ValueTuple =>
            {
                //_Node.OriginalRigStatus = ValueTuple.newValue as VIVEOriginalExpressionStatus;
            });
            _Node.TokenForOriginalRigStatus = _ObjField;
            _Node.inputContainer.Add(_ObjField);
            Holders.Add(_ObjField);
			//------Genetate Token for OriginalRigStatus
			//------Genetate Holder for the Face Model BlendShap
			//_ObjField = new ObjectField
			//{
			//    objectType = typeof(SkinnedMeshRenderer),
			//    allowSceneObjects = false,
			//};
			//_ObjField.SetEnabled(false);
			//_ObjField.style.marginLeft = 25;
			//_ObjField.style.marginTop = 5;
			//_ObjField.style.width = new Length(150, LengthUnit.Pixel);
			//_ObjField.allowSceneObjects = true;
			//_Node.inputContainer.Add(_ObjField);
			//Holders.Add(_ObjField);
			//------Genetate Holder for the Face Model BlendShap

			//---Add Label Face
			Label _LabelFace = new Label();
			_LabelFace.text = "Target Face Object";
			_LabelFace.style.marginLeft = 25;
			_LabelFace.style.marginTop = 10;
			_Node.inputContainer.Add(_LabelFace);
			//------Genetate Holder for the Face Model GameObject
			_ObjField = new ObjectField
			{
				objectType = typeof(GameObject),
				allowSceneObjects = true,
			};
			_ObjField.style.marginLeft = 25;
			_ObjField.style.width = new Length(220, LengthUnit.Pixel);
			_ObjField.RegisterValueChangedCallback(ValueTuple =>
			{
				_Node.TargetFace = ValueTuple.newValue as GameObject;
			});
			//_ObjField.SetEnabled(false);
			_Node.inputContainer.Add(_ObjField);
			_Node.TokenForTargetFace = _ObjField;
			Holders.Add(_ObjField);


			Port _Port;
            //---Generate Output Ports for diferent States
            _GeneratePorts();
			//_Node.outputContainer.style.width = 500;
			//---Generate Output Ports for diferent States
            _Node.outputContainer.style.alignContent = Align.Center;

			_Node.tooltip = "Hint for the Original State Node";
            _Node.RefreshExpandedState();
            _Node.RefreshPorts();
            Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero * 500);
            _Node.SetPosition(new Rect(_TempVec.x, _TempVec.y, DefaultNodeSize.x, DefaultNodeSize.y));

            _Node.style.width = new Length(600, LengthUnit.Pixel);
            return _Node;

			void _GeneratePorts()
            {
                //string _FilePath = Application.dataPath.Replace("Assests", "");
                Image _Img = new Image();
                for (int i = 0; i < (int)InputDeviceEye.Expressions.MAX; i++)
                {
                    _Port = GeneratePort(_Node, Direction.Output, _Capacity: Port.Capacity.Multi, _PortName: ((InputDeviceEye.Expressions)i).ToString());
                    _Port.allowMultiDrag = true;
                    _Node.outputContainer.Add(_Port);
                    _Node.OutPutPorts[i] = _Port;
                    _Img = new Image();
					var imgPath = FindHintPic(i, isEye: true);
					if (!string.IsNullOrEmpty(imgPath))
						_Img.image = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
                    _Node.outputContainer.Add(_Img);
                    ListofOriginalStateData.Add(new OriginalStatePortData(_Port, _Img));
                }
                for (int i = 0; i < (int)InputDeviceLip.Expressions.Max; i++)
                {
                    _Port = GeneratePort(_Node, Direction.Output, _Capacity: Port.Capacity.Multi, _PortName: ((InputDeviceLip.Expressions)i).ToString());
                    _Port.allowMultiDrag = true;
                    _Node.outputContainer.Add(_Port);
                    _Node.OutPutPorts[(int)InputDeviceEye.Expressions.MAX + i] = _Port;
                    _Img = new Image();
					var imgPath = FindHintPic(i, isEye: false);
					if (!string.IsNullOrEmpty(imgPath))
						_Img.image = AssetDatabase.LoadAssetAtPath<Texture2D>(imgPath);
					_Node.outputContainer.Add(_Img);
                    ListofOriginalStateData.Add(new OriginalStatePortData(_Port, _Img));
                }
            }


		}

		string FindHintPic(int i, bool isEye)
		{
			//_Img.image = LoadPNG(_FilePath + "/Packages/com.htc.upm.wave.openxr.toolkit/Editor/FacialExpressionMaker/VIVEFacialTrackingGraphHintPics/EYE_" + i + ".png");
			//_Img.image = Resources.Load<Texture2D>("VIVEFacialTrackingGraphHintPics/EYE_" + i );

			string name = (isEye ? "EYE_" : "LIP_") + i;
			string[] guids = AssetDatabase.FindAssets(name + " t:texture");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				// filter the exact name
				if (path.Contains("GraphHintPicsNew/" + name + "."))
				{
					return path;
				}
			}
			return "";
		}

        List<OriginalStatePortData> ListofOriginalStateData = new List<OriginalStatePortData>();

        static Texture2D LoadPNG(string _Path)
        {
            Texture2D _Tex = null;
            byte[] _FileData;

            if (File.Exists(_Path))
            {
                _FileData = File.ReadAllBytes(_Path);
                _Tex = new Texture2D(2, 2);
                _Tex.LoadImage(_FileData);
            }
            return _Tex;
        }


		public void SetOriginalExpressionStatus(GameObject _Target, VIVEOriginalExpressionStatus _Status)
        {
            _Status.JointInfos = new List<JointInfo>();
            _AddJointsInfo(_Target.transform);
            //Debug.Log("SetOriginalExpressionStatus(6) target:" + _Target.name + ", VIVEOriginalExpressionStatus: " + _Status.name);
			void _AddJointsInfo(Transform _Joint)
            {
                for (int i = 0; i < _Joint.childCount; i++)
                {
					//Debug.Log("SetOriginalExpressionStatus _AddJointsInfo(6) not Dont idx:" + i + ", gameObjName: "+_Joint.gameObject.name);
                    Transform _TempTrans = _Joint.GetChild(i);
                    _AddJointsInfo(_TempTrans);
                }
				if (_Joint.tag != "Dont")
                {
					//Debug.Log("SetOriginalExpressionStatus _AddJointsInfo(6) not Dont gameObjName: "+_Joint.gameObject.name+", Location: "+ _Joint.localRotation.x +", "+ _Joint.localRotation.y+", "+ _Joint.localRotation.z);
					_Status.JointInfos.Add(new JointInfo(_Joint.gameObject.name, _Joint.localPosition, _Joint.localRotation.eulerAngles, _Joint.localScale, _Joint.GetComponent<SkinnedMeshRenderer>()));
                }
            }
            EditorUtility.SetDirty(_Status);
            AssetDatabase.SaveAssets();
        }

        void PressSetBtnEvt()
        {

        }


#if false
		void SetTargetToOriginalStatus()
        {
			//Debug.Log("SetTargetToOriginalStatus(6) SetBlendShapeWeight() 0");
            List<Transform> _ListOfTrans = new List<Transform>();
            if (OriginalNode.OriginalRigStatus == null)
            {
                return;
            }
            if (OriginalNode.Target == null)
            {
                Debug.LogWarning("There is not targetting GameObject!");
                return;
            }

            GameObject _Target = OriginalNode.Target;
            _AddTrans(_Target.transform);

            if (_ListOfTrans.Count != OriginalNode.OriginalRigStatus.JointInfos.Count)
            {
                Debug.LogWarning("It seems the target has been modified since last saving.");
                return;
            }

            for (int i = 0; i < _ListOfTrans.Count; i++)
            {
                _ListOfTrans[i].localPosition = OriginalNode.OriginalRigStatus.JointInfos[i].LocalPos;
                _ListOfTrans[i].localRotation = Quaternion.Euler(OriginalNode.OriginalRigStatus.JointInfos[i].LocalRotation.x, OriginalNode.OriginalRigStatus.JointInfos[i].LocalRotation.y, OriginalNode.OriginalRigStatus.JointInfos[i].LocalRotation.z);
                _ListOfTrans[i].localScale = OriginalNode.OriginalRigStatus.JointInfos[i].LocalScale;
                for (int j = 0; j < OriginalNode.OriginalRigStatus.JointInfos[i].BlendShapes.Count; j++)
                {
                    //Debug.Log("SetTargetToOriginalStatus(6) SetBlendShapeWeight() 1");
                    _ListOfTrans[i].GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(j, OriginalNode.OriginalRigStatus.JointInfos[i].BlendShapes[j]);
                }
            }

            void _AddTrans(Transform _Joint)
            {
                for (int i = 0; i < _Joint.childCount; i++)
                {
                    Transform _Trans = _Joint.GetChild(i);
                    _AddTrans(_Trans);
                }
                if (_Joint.tag != "Dont")
                {
                    _ListOfTrans.Add(_Joint);
                }
            }
        }
#endif

        T CreateAsset<T>(string _AssetName = "new Asset", string _Path = "VIVEFacialExpressionMakerAsset") where T : ScriptableObject
        {
			//ScriptableObject _Asset = ScriptableObject.CreateInstance<ScriptableObject>();
			T _Asset = ScriptableObject.CreateInstance<T>();

			// show save file dialog and let user set the file name and path
			string absoluteFilePath = EditorUtility.SaveFilePanel(
				"Save Asset", //Dialog Title
				"Assets", //Parent folder name
				"VIVEFacialExpressionBlendShape.asset", //Defatult Asset file name
				"asset"
			);
			// save data to asset file
			if (!string.IsNullOrEmpty(absoluteFilePath))
			{
				// change absoluteFilePath tom mapping "Assets" folder path
				string relativePath = absoluteFilePath.Replace(Application.dataPath, "Assets");

				AssetDatabase.CreateAsset(_Asset, relativePath);
				EditorUtility.SetDirty(_Asset);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			    EditorUtility.FocusProjectWindow();
			}
			Selection.activeObject = _Asset;
			return _Asset;
        }

        void RemovePreviousEdges(Port _Port, Edge _CurrentEdge)
        {
            foreach (Edge _Edge in _Port.connections.ToList())
            {
                if (_Edge != _CurrentEdge)
                {
                    _Port.Disconnect(_Edge);
                }
            }
        }

        void SwitchUsingMode()
        {
            int _Index = (int)VIVEFacialTrackingGraph.UsingMode;
            _Index++;
            _Index %= ((int)DataMode.Max);
            VIVEFacialTrackingGraph.UsingMode = (DataMode)_Index;
            OriginalNode.title = "Original State (" + VIVEFacialTrackingGraph.UsingMode.ToString() + ")";

            switch (VIVEFacialTrackingGraph.UsingMode)
            {
                case DataMode.Rig:
                    Holders[0].SetEnabled(true);
                    Holders[1].SetEnabled(false);
                    break;
                case DataMode.BlendShape:
                    Holders[0].SetEnabled(false);
                    Holders[1].SetEnabled(true);
                    break;
                case DataMode.Both:
                    Holders[0].SetEnabled(true);
                    Holders[1].SetEnabled(true);
                    break;
            }
        }

		public ExpressionStateNode GenerateExpressionStateNode(string _Title = "Expression State", int id = 0, List<float> _weight = null/*float _weight=0.0f*/)
		{
			//using in function LoadFacialTrackingController()
			int i = 0, j = 0;
			ExpressionStateNode _Node = new ExpressionStateNode
			{
				title = _Title,
				GUID = _Title,// Guid.NewGuid().ToString(),
				ExtryPoint = true,
			};
			_Node.NodeName = _Title;
			_Node.BlendShapeId = id;
			//_Node.RegisterCallback<MouseDownEvent>(evt =>{ Debug.Log("GenerateExpressionStateNode(670) Node.RegisterCallback name:" + _Node.NodeName); });
			CustomNode = _Node;

			//---Generate Add Button
			Button _Button = new Button(clickEvent: () =>
			{
				Debug.Log("GenerateExpressionStateNode() button Add element: " + i);
				Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
				_Port.allowMultiDrag = true;
				_Port.style.height = 30;
				_Node.InPorts.Add(_Port);
				_Node.inputContainer.Add(_Port);

				//------Generate Label
				Label _Label = new Label();
				_Label.name = _Port.portName;
				_Label.text = "Weight:100";
				//_Node.BSWeight.Insert(i, 0.0f);
				_Node.NameLabelSet.Add(_Label);
				_Label.RegisterValueChangedCallback(ValueTuple => { //get label text as node name need 1 first blank
				});
				_Node.outputContainer.Add(_Label);
				//------Generate Label

				//----Slider for float value
				float val = 100f;
				var slider = new Slider(0, 100, SliderDirection.Horizontal);
				slider.value = 100;
				slider.RegisterValueChangedCallback(evt =>
				{
					slider.value = Mathf.RoundToInt(evt.newValue);
					val = Mathf.RoundToInt(evt.newValue);
                    _Label.text = "Weight:" + val;

					//var result = from r in _Node.BSWeight where r.BSName == ("Element"+i) select r;
					//result.First().Weight = val;
					//Debug.Log("GenerateExpressionStateNode() slider CB:" + result.First().Weight);
				});
				//_Node.BSWeight.Add(val);
				_Node.outputContainer.Add(slider);
				//----Slider for float value
				_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i,_Node.position));
				j = j + 2; i++;
			});
			_Button.text = "Add";
			//_Node.EditBtn = _Button;
			_Node.titleContainer.Add(_Button);
			//---Generate Add Button

			//---Generate Delete Button
			Button _ButtonDel = new Button(clickEvent: () =>
			{
				_Node.inputContainer.RemoveAt(i - 1);
				_Node.outputContainer.RemoveAt(j - 1);
				_Node.outputContainer.RemoveAt(j - 2);
				_Node.BSWeight.RemoveAt(i - 1);
				j = j - 2;
				i--;
			});
			_ButtonDel.text = "Del";
			//_Node.EditBtn2 = _ButtonDel;
			_Node.titleContainer.Add(_ButtonDel);
			//---Generate Delete Button


			Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero * 500);
			//_TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero);
			//Debug.Log("Window Pos: " + (VIVEFacialTrackingGraph.FTWindow.position.position + Vector2.right * Screen.width) + ", x:" + _TempVec.x + ",y:" + _TempVec.y + ",size x:" + DefaultNodeSize.x + ", size y:" + DefaultNodeSize.y);
			//if (id > 48)
			//	_Node.SetPosition(new Rect(_TempVec.x + 750 + 400, _TempVec.y - 10 + (id - 48) * 40, DefaultNodeSize.x, DefaultNodeSize.y));
			//else
			//	_Node.SetPosition(new Rect(_TempVec.x + 750, _TempVec.y - 10 + id * 40, DefaultNodeSize.x, DefaultNodeSize.y));

			_Node.SetPosition(new Rect(_TempVec.x+1200, _TempVec.y+120+id*2, DefaultNodeSize.x, DefaultNodeSize.y));
			//_Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTGraphView.WindowRect.x/2, VIVEFacialTrackingGraph.FTGraphView.WindowRect.y/2, DefaultNodeSize.x, DefaultNodeSize.y));
			//_Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTGraphView.WindowRect.x, VIVEFacialTrackingGraph.FTGraphView.WindowRect.y, DefaultNodeSize.x, DefaultNodeSize.y));


			if (_weight != null)
			{
				for (int k = 0; k < _weight.Count; k++)
				{
			_Node.inputContainer.style.width = 25;
			//---Generate Ports for diferent States
			Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
			_Port.allowMultiDrag = true;
			_Port.style.height = 30;
					_Port.name = "Element" + i;
			_Node.InPorts.Add(_Port);
			_Node.inputContainer.Add(_Port);
			//---Generate Ports for diferent States

			//------Generate Label
			Label _Label = new Label();
			_Label.name = _Port.portName;
					_Label.text = "Weight:" + _weight[k].ToString();
			//_Node.BSWeight.Insert(i, 0.0f);
			_Node.NameLabelSet.Add(_Label);
					_Label.RegisterValueChangedCallback(ValueTuple =>
					{ //get label text as node name need 1 first blank
			});
			//_Element.Add(_Label);
			_Node.outputContainer.Add(_Label);
			//------Generate Label

			//----Slider for float value
			float val = 100f;
			var slider = new Slider(0, 100, SliderDirection.Horizontal);
			slider.value = _weight[k];
			slider.RegisterValueChangedCallback(evt =>
			{
                slider.value = Mathf.RoundToInt(evt.newValue);
                val = Mathf.RoundToInt(evt.newValue);
                _Label.text = "Weight:" + val;

				//var result = from r in _Node.BSWeight where r.BSName == "Element1" select r;
				//result.First().Weight = val;
				//Debug.Log("GenerateExpressionStateNode() slider CB:" + _Node.NameLabelSet[0].name + "Val:" + _Node.NameLabelSet[0].text);
			});
			//_Node.BSWeight.Add(val);
			_Node.outputContainer.Add(slider);
			//----Slider for float value
			/*JointGapInfoBS bs =*/
					_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i, _Node.position));
			j = j + 2; i++;
				}
			}

			_Node.RefreshExpandedState();
			_Node.RefreshPorts();
			_Node.style.width = new Length(240, LengthUnit.Pixel);

			//ExpStateNodeData _RigNodeData = new ExpStateNodeData(_Node, _Label, _Field);
			//ListofRigStateNodeData.Add(_RigNodeData);
			return _Node;
		}
		public ExpressionStateNode GenerateExpressionStateNode(Rect _position, string _Title = "Expression State", int id = 0, List<TrackNodeInfoBS> _weight = null/*float _weight=0.0f*/)
		{
			//using in function LoadFacialTrackingController()
			int i = 0, j = 0;
			ExpressionStateNode _Node = new ExpressionStateNode
			{
				title = _Title,
				GUID = _Title,// Guid.NewGuid().ToString(),
				ExtryPoint = true,
			};
			_Node.NodeName = _Title;
			_Node.BlendShapeId = id;
			//_Node.RegisterCallback<MouseDownEvent>(evt =>{ Debug.Log("GenerateExpressionStateNode(670) Node.RegisterCallback name:" + _Node.NodeName); });
			CustomNode = _Node;

			//---Generate Add Button
			Button _Button = new Button(clickEvent: () =>
			{
				Debug.Log("GenerateExpressionStateNode() button Add element: " + i);
				Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
				_Port.allowMultiDrag = true;
				_Port.style.height = 30;
				_Node.InPorts.Add(_Port);
				_Node.inputContainer.Add(_Port);

				//------Generate Label
				Label _Label = new Label();
				_Label.name = _Port.portName;
				_Label.text = "Weight:100";
				//_Node.BSWeight.Insert(i, 0.0f);
				_Node.NameLabelSet.Add(_Label);
				_Label.RegisterValueChangedCallback(ValueTuple => { //get label text as node name need 1 first blank
				});
				_Node.outputContainer.Add(_Label);
				//------Generate Label

				//----Slider for float value
				float val = 100f;
				var slider = new Slider(0, 100, SliderDirection.Horizontal);
				slider.value = 100;
				slider.RegisterValueChangedCallback(evt =>
				{
                    slider.value = Mathf.RoundToInt(evt.newValue);
                    val = Mathf.RoundToInt(evt.newValue);
                    _Label.text = "Weight:" + val;

					//var result = from r in _Node.BSWeight where r.BSName == ("Element"+i) select r;
					//result.First().Weight = val;
					//Debug.Log("GenerateExpressionStateNode() slider CB:" + result.First().Weight);
				});
				//_Node.BSWeight.Add(val);
				_Node.outputContainer.Add(slider);
				//----Slider for float value
				_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i, _Node.position));
				j = j + 2; i++;
			});
			_Button.text = "Add";
			//_Node.EditBtn = _Button;
			_Node.titleContainer.Add(_Button);
			//---Generate Add Button

			//---Generate Delete Button
			Button _ButtonDel = new Button(clickEvent: () =>
			{
				_Node.inputContainer.RemoveAt(i - 1);
				_Node.outputContainer.RemoveAt(j - 1);
				_Node.outputContainer.RemoveAt(j - 2);
				_Node.BSWeight.RemoveAt(i - 1);
				j = j - 2;
				i--;
			});
			_ButtonDel.text = "Del";
			//_Node.EditBtn2 = _ButtonDel;
			_Node.titleContainer.Add(_ButtonDel);
			//---Generate Delete Button


			_Node.SetPosition(new Rect(_position.x, _position.y, _position.width, _position.height));


			if (_weight != null)
			{
				for (int k = 0; k < _weight.Count; k++)
				{
					_Node.inputContainer.style.width = 25;
					//---Generate Ports for diferent States
					Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
					_Port.allowMultiDrag = true;
					_Port.style.height = 30;
					_Port.name = "Element" + i;
					_Node.InPorts.Add(_Port);
					_Node.inputContainer.Add(_Port);
					//---Generate Ports for diferent States

					//------Generate Label
					Label _Label = new Label();
					_Label.name = _Port.portName;
					_Label.text = "Weight:" + _weight[k].Weight.ToString();
					//_Node.BSWeight.Insert(i, 0.0f);
					_Node.NameLabelSet.Add(_Label);
					_Label.RegisterValueChangedCallback(ValueTuple =>
					{ //get label text as node name need 1 first blank
					});
					//_Element.Add(_Label);
					_Node.outputContainer.Add(_Label);
					//------Generate Label

					//----Slider for float value
					float val = 100f;
					var slider = new Slider(0, 100, SliderDirection.Horizontal);
					slider.value = _weight[k].Weight;
					slider.RegisterValueChangedCallback(evt =>
					{
                        slider.value = Mathf.RoundToInt(evt.newValue);
                        val = Mathf.RoundToInt(evt.newValue);
                        _Label.text = "Weight:" + val;

						//var result = from r in _Node.BSWeight where r.BSName == "Element1" select r;
						//result.First().Weight = val;
						//Debug.Log("GenerateExpressionStateNode() slider CB:" + _Node.NameLabelSet[0].name + "Val:" + _Node.NameLabelSet[0].text);
					});
					//_Node.BSWeight.Add(val);
					_Node.outputContainer.Add(slider);
					//----Slider for float value
					/*JointGapInfoBS bs =*/
					_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i, _Node.position));
					j = j + 2; i++;
				}
			}

			_Node.RefreshExpandedState();
			_Node.RefreshPorts();
			_Node.style.width = new Length(240, LengthUnit.Pixel);

			//ExpStateNodeData _RigNodeData = new ExpStateNodeData(_Node, _Label, _Field);
			//ListofRigStateNodeData.Add(_RigNodeData);
			return _Node;
		}
		public VIVEFacialTrackingGraphNode GenerateExpressionStateNodeOne(string _Title = "NewExpression", int id = 0, float _weight = 100f)
		{
			int i = 0, j = 0;
			ExpressionStateNode _Node = new ExpressionStateNode ()
			{
				title = _Title,
				GUID = Guid.NewGuid().ToString(),
				ExtryPoint = true,
			};
			_Node.NodeName = _Title;
			_Node.title = _Title;
			_Node.BlendShapeId = id;
			CustomNode = _Node;
			//Debug.Log("GenerateExpressionStateNodeOne(667) SetTargetToExpStatus() node name(0):" + _Node.name);

			//---Generate Naming Field
			VisualElement _Element = new VisualElement();
			_Element.style.height = 24;
			_Element.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.7f));
			_Element.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
			_Element.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
			_Element.style.borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
			_Element.style.borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
			_Node.ElementAt(0).Insert(1, _Element);
			//------Generate Text Field(ExpressionStatus name)
			TextField _Field = new TextField();
			_Field.RegisterValueChangedCallback(ValueTuple => {
				_Node.NodeName = ValueTuple.newValue.Substring(0, ValueTuple.newValue.Length);
				_Node.title = _Node.name;
				//Debug.Log("GenerateExpressionStateNodeOne() update textfield for New name:" + _Node.NodeName + "=");
			});
			//_Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			_Element.Add(_Field);
			//------Generate Text Field(ExpressionStatus name)
			//---Generate Save one node Button
			Button _ButtonSave = new Button(clickEvent: () =>{ _Node.title = _Node.NodeName; });
			_ButtonSave.text = "SetName";
			_Node.titleButtonContainer.Add(_ButtonSave);
			//---Generate Save one node Button
			//---Generate Naming Field

			Button _Button = new Button(clickEvent: () =>
			{
				Debug.Log("GenerateExpressionStateNodeOne() button Add element: " + i);
				Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
				_Port.allowMultiDrag = true;
				_Port.style.height = 30;
				_Node.InPorts.Add(_Port);
				_Node.inputContainer.Add(_Port);

				//------Generate Label
				Label _Label = new Label();
				_Label.name = _Port.portName;
				_Label.text = "Weight:100";
				//_Node.BSWeight.Insert(i, 0.0f);
				_Node.NameLabelSet.Add(_Label);
				_Label.RegisterValueChangedCallback(ValueTuple => { //get label text as node name need 1 first blank
				});
				_Node.outputContainer.Add(_Label);
				//------Generate Label

				//----Slider for float value
				float val = 100f;
				var slider = new Slider(0, 100, SliderDirection.Horizontal);
				slider.value = 100;
				slider.RegisterValueChangedCallback(evt =>
				{
                    slider.value = Mathf.RoundToInt(evt.newValue);
                    val = Mathf.RoundToInt(evt.newValue);
                    _Label.text = "Weight:" + val;

					//var result = from r in _Node.BSWeight where r.BSName == ("Element"+i) select r;
					//result.First().Weight = val;
					//Debug.Log("GenerateExpressionStateNode() slider CB:" + result.First().Weight);
				});
				//_Node.BSWeight.Add(val);
				_Node.outputContainer.Add(slider);
				//----Slider for float value
				_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i,_Node.position));
				j = j + 2; i++;
			});
			_Button.text = "Add";
			//_Node.EditBtn = _Button;
			_Node.titleContainer.Add(_Button);
			//---Generate Add Button



			//---Generate Delete Button
			Button _ButtonDel = new Button(clickEvent: () =>
			{
				_Node.inputContainer.RemoveAt(i - 1);
				_Node.outputContainer.RemoveAt(j - 1);
				_Node.outputContainer.RemoveAt(j - 2);
				_Node.BSWeight.RemoveAt(i - 1);
				j = j - 2;
				i--;
			});
			_ButtonDel.text = "Del";
			//_Node.EditBtn2 = _ButtonDel;
			_Node.titleContainer.Add(_ButtonDel);
			//---Generate Delete Button

			_Node.inputContainer.style.width = 25;
			//---Generate Ports for diferent States
			Port _Port = GeneratePort(_Node, Direction.Input, _Capacity: Port.Capacity.Multi, _PortName: "Element" + i);
			_Port.allowMultiDrag = true;
			_Port.style.height = 30;
			_Node.InPorts.Add(_Port);
			_Node.inputContainer.Add(_Port);
			//---Generate Ports for diferent States


			//------Generate Label
			Label _Label = new Label();
			_Label.name = _Port.portName;
			_Label.text = "Weight:" + _weight.ToString();
			//_Node.BSWeight.Insert(i, 0.0f);
			_Node.NameLabelSet.Add(_Label);
			_Label.RegisterValueChangedCallback(ValueTuple => { //get label text as node name need 1 first blank
			});
			//_Element.Add(_Label);
			_Node.outputContainer.Add(_Label);
			//------Generate Label

			//----Slider for float value
			float val = 100f;
			var slider = new Slider(0, 100, SliderDirection.Horizontal);
			slider.value = _weight;
			slider.RegisterValueChangedCallback(evt =>
			{
                slider.value = Mathf.RoundToInt(evt.newValue);
                val = Mathf.RoundToInt(evt.newValue);
                _Label.text = "Weight:" + val;

				//var result = from r in _Node.BSWeight where r.BSName == "Element1" select r;
				//result.First().Weight = val;
				//Debug.Log("GenerateExpressionStateNodeOne() slider CB:" + _Node.NameLabelSet[0].name + "Val:" + _Node.NameLabelSet[0].text);
			});
			//_Node.BSWeight.Add(val);
			_Node.outputContainer.Add(slider);
			//----Slider for float value

			Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(VIVEFacialTrackingGraph.FTWindow.position.size / 4);
			//_TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero);
			//Debug.Log("Window Pos: " + (VIVEFacialTrackingGraph.FTWindow.position.position + Vector2.right * Screen.width));
			_Node.SetPosition(new Rect(_TempVec.x, _TempVec.y, DefaultNodeSize.x, DefaultNodeSize.y));
			//_Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTGraphView.WindowRect.x, VIVEFacialTrackingGraph.FTGraphView.WindowRect.y, DefaultNodeSize.x, DefaultNodeSize.y));


			/*JointGapInfoBS bs =*/
			_Node.BSWeight.Add(new JointGapInfoBS("Element" + i, 100f, id, "Element" + i, _Node.position));
			j = j + 2; i++;

			_Node.RefreshExpandedState();
			_Node.RefreshPorts();

			//ExpStateNodeData _RigNodeData = new ExpStateNodeData(_Node, _Label, _Field);
			//ListofRigStateNodeData.Add(_RigNodeData);
			return _Node;
		}


		List<ExpStateNodeData> ListofRigStateNodeData = new List<ExpStateNodeData>();
        List<BlendShapeStateNodeData> ListofBlendShapeStateNodeData = new List<BlendShapeStateNodeData>();
        public ExpressionStateNode GenerateExpressionStateNode(string _Title = "Expression State", int _Index = 0)
        {
            ExpressionStateNode _Node = new ExpressionStateNode
            {
                title = _Title,
                GUID = Guid.NewGuid().ToString(),
                ExtryPoint = true,
            };

			//Debug.Log("GenerateExpressionStateNode(667) SetTargetToExpStatus() node name(0):"+ _Node.name);


            //---Generate Naming Field
            //------Generate Background
            VisualElement _Element = new VisualElement();
            _Element.style.height = 20;
            _Element.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.7f));
            _Element.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderTopWidth = 2;
            _Element.style.borderBottomWidth = 2;
            _Element.style.borderLeftWidth = 5;
            _Element.style.borderRightWidth = 5;
            _Node.ElementAt(0).Insert(1, _Element);
            //------Generate Background

            //------Generate Label
            Label _Label = new Label();
            _Label.text = "New Expression";
            _Node.NodeName = "New Expression";
            _Node.NameLabel = _Label;
            _Label.RegisterValueChangedCallback(ValueTuple =>
            {
				//get label text as node name need 1 first blank
				_Node.NodeName = ValueTuple.newValue.Substring(1, ValueTuple.newValue.Length-1); //start from 0 will get redundant 1 blank
				Debug.Log("GenerateExpressionStateNode() update lable for ExpressionStatus name:"+ _Node.NodeName+"=");
			});
            _Element.Add(_Label);
			//Debug.Log("GenerateExpressionStateNode(701) SetTargetToExpStatus() node name(1): " + _Node.name);
			//------Generate Label

			//------Generate Text Field(ExpressionStatus name)
			TextField _Field = new TextField();
			_Element.Add(_Field);
			_Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			//------Generate Text Field(ExpressionStatus name)


			//---Generate Set Button
			Button _Button = new Button(clickEvent: () =>
            {
                if (OriginalNode.Target == null)
                {
                    Debug.LogError("There is no target!");
                    return;
                }

                //if (_Node.Is_Editting)
                {
                    if (_Node.ExpStatus == null)
                    {
						_Node.ExpStatus = CreateAsset<VIVEExpressionStatus>(_AssetName: _Node.NodeName + "ExpressionStatus", _Path: "CustomFacialExpressionAsset");
                        _Node.TokenForRigStatus.value = _Node.ExpStatus;
						Debug.Log("GenerateExpressionStateNode() Add Expression Editing Button: "+ _Node.ExpStatus.name+", node name:"+ _Node.NodeName);
                    }
                    //SetExpressionStatus(OriginalNode.Target, _Node.ExpStatus);
                    //_Node.Is_Editting = false;
                    //_Node.EditBtn.text = "Edit";
                    UnhighLightElement(_Node);
                }
                /*else
                {
                    bool _Is_Editting_others = false;
#if UNITY_2020
                    List<Node> _AllNodes = VIVEFacialTrackingGraph.FTGraphView.nodes.ToList();
                    for (int i = 0; i < _AllNodes.Count; i++)
                    {
                        if (_AllNodes[i].GetType() == typeof(ExpressionStateNode) && _AllNodes[i] != _Node)
                        {
                            if ((_AllNodes[i] as ExpressionStateNode).Is_Editting)
                            {
                                _Is_Editting_others = true;
                                Debug.LogWarning("You are current editting another node!");
                            }
                        }
                    }
#else
                    foreach (Node _node in VIVEFacialTrackingGraph.FTGraphView.nodes)
                    {
                        if (_node.GetType() == typeof(ExpressionStateNode) && _node != _Node)
                        {
                            if((_node as ExpressionStateNode).Is_Editting)
                            {
                                _Is_Editting_others = true;
                                Debug.LogWarning("You are current editting another node!");
                            }
                        }
                    }
#endif

                    if (!_Is_Editting_others)
                    {
                        _Node.Is_Editting = true;
                        _Node.EditBtn.text = "Editting";
                        HighLightElement(_Node);
                        SetTargetToExpStatus(_Node.ExpStatus);
						Debug.Log("GenerateExpressionStateNode(Btn Edit) call SetTargetToExpStatus() node name(2): " + _Node.name);
					}
                }*/

			});
            _Button.text = "SetExpression";
            _Node.EditBtn = _Button;
            _Node.titleContainer.Insert(1, _Button);
			//---Generate Set Button
			//---Generate Naming Field


			//---Generate Ports for diferent States
			Port _Port = GeneratePort(_Node, Direction.Input, _PortName: "");
            _Node.InputPort = _Port;
            _Node.inputContainer.Add(_Port);
			//---Generate Ports for diferent States

			//------Genetate Token for RigStatus
			ObjectField _ObjField = new ObjectField
            {
                objectType = typeof(VIVEExpressionStatus),
                allowSceneObjects = false,
            };
            _ObjField.style.marginLeft = 25;
            _ObjField.style.width = new Length(160, LengthUnit.Pixel);
            _ObjField.allowSceneObjects = true;
            _ObjField.RegisterValueChangedCallback(ValueTuple =>
            {
				_Node.ExpStatus = ValueTuple.newValue as VIVEExpressionStatus;
				_Label.text = " " + _Node.ExpStatus.name.ToString().Replace("ExpressionStatus", "");
				_Node.NodeName = _Node.ExpStatus.name.ToString().Replace("ExpressionStatus", "");
				Debug.Log("GenerateExpressionStateNode() update CustomVIVEExpressionStatusAsset: "+ _Node.ExpStatus.name.ToString().Replace("ExpressionStatus", "")+"=");
			});
			//Debug.Log("GenerateExpressionStateNode(800) Genetate Token for RigStatus(3): Custom VIVEExpressionStatus");
            _Node.TokenForRigStatus = _ObjField;
            _Node.outputContainer.Add(_ObjField);
            Holders.Add(_ObjField);
            //------Genetate Token for RigStatus

            _Node.tooltip = "Hint for the (Expression) State Node";
            _Node.RefreshExpandedState();
            _Node.RefreshPorts();
            Vector2 _TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(VIVEFacialTrackingGraph.FTWindow.position.size/4);
            //_TempVec = VIVEFacialTrackingGraph.FTGraphView.contentViewContainer.WorldToLocal(Vector2.zero);
            //Debug.Log("Window Pos: " + (VIVEFacialTrackingGraph.FTWindow.position.position + Vector2.right * Screen.width));
            _Node.SetPosition(new Rect(_TempVec.x, _TempVec.y, DefaultNodeSize.x, DefaultNodeSize.y));
            //_Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTGraphView.WindowRect.x, VIVEFacialTrackingGraph.FTGraphView.WindowRect.y, DefaultNodeSize.x, DefaultNodeSize.y));

            ExpStateNodeData _RigNodeData = new ExpStateNodeData(_Node, _Label, _Field);
            ListofRigStateNodeData.Add(_RigNodeData);
            return _Node;
        }
        /*
        public BlendShapeStateNode GenerateBlendShapeStateNode(string _Title = "(Expression) State", int _Index = 0)
        {
            BlendShapeStateNode _Node = new BlendShapeStateNode
            {
                title = _Title,
                GUID = Guid.NewGuid().ToString(),
                ExtryPoint = true,
            };

            //---Generate Naming Field
            //------Generate Background
            VisualElement _Element = new VisualElement();
            _Element.style.height = 20;
            _Element.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.7f));
            _Element.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
            _Element.style.borderTopWidth = 2;
            _Element.style.borderBottomWidth = 2;
            _Element.style.borderLeftWidth = 5;
            _Element.style.borderRightWidth = 5;
            _Node.ElementAt(0).Insert(1, _Element);
            //------Generate Background

            //------Generate Label
            Label _Label = new Label();
            _Label.text = " New Expression";
            _Element.Add(_Label);
            //------Generate Label
            //------Generate Text Field
            TextField _Field = new TextField();
            _Element.Add(_Field);
            _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            //------Generate Text Field
            //---Generate Set Button
            Button _Button = new Button(clickEvent: () =>
            {

            });
            _Button.text = "Set";
            _Node.titleContainer.Insert(1, _Button);
            //---Generate Set Button
            //---Generate Naming Field

            //---Generate Ports for diferent States
            Port _Port = GeneratePort(_Node, Direction.Input, _PortName: "");
            _Node.inputContainer.Add(_Port);
            //---Generate Ports for diferent States
            _Node.tooltip = "Hint for the (Expression) State Node";
            _Node.RefreshExpandedState();
            _Node.RefreshPorts();
            _Node.SetPosition(new Rect(VIVEFacialTrackingGraph.FTWindow.position.width / 10, VIVEFacialTrackingGraph.FTWindow.position.height / 8, DefaultNodeSize.x, DefaultNodeSize.y));

            BlendShapeStateNodeData _RigNodeData = new BlendShapeStateNodeData(_Node, _Label, _Field);
            ListofBlendShapeStateNodeData.Add(_RigNodeData);
            return _Node;
        }
        */
        Color HighLightColor = new Color(132.0f / 256.0f, 228.0f / 256.0f, 231.0f / 256.0f, 1);
        void HighLightElement(VisualElement _Element)
        {
            _Element.style.borderLeftWidth = 3;
            _Element.style.borderRightWidth = 3;
            _Element.style.borderTopWidth = 3;
            _Element.style.borderBottomWidth = 3;
            _Element.style.borderTopLeftRadius = 8;
            _Element.style.borderTopRightRadius = 8;
            _Element.style.borderBottomRightRadius = 8;
            _Element.style.borderBottomLeftRadius = 8;
            _Element.style.borderLeftColor = new StyleColor(HighLightColor);
            _Element.style.borderRightColor = new StyleColor(HighLightColor);
            _Element.style.borderTopColor = new StyleColor(HighLightColor);
            _Element.style.borderBottomColor = new StyleColor(HighLightColor);
        }

        void UnhighLightElement(VisualElement _Element)
        {
            _Element.style.borderLeftWidth = 0;
            _Element.style.borderRightWidth = 0;
            _Element.style.borderTopWidth = 0;
            _Element.style.borderBottomWidth = 0;
        }

        //Rule of Ports
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
			//Debug.Log("GetCompatiblePorts() start:"+ startPort.portName);

			List <Port> _Ports = new List<Port>();
#if UNITY_2020_3_OR_NEWER
			List<Port> _AllPorts = ports.ToList();
            foreach (Port _Port in _AllPorts)
            {
                if (startPort == _Port || startPort.node == _Port.node || startPort.direction == _Port.direction) //Port In Out put basic rules
                {
                    continue;
                    //Debug.Log("GetCompatiblePorts() pass foreach" );
                }
                _Ports.Add(_Port);
                continue;
                //---Check if the port doesn't connect to multiple nodes with the same type
                if (startPort.node.GetType() == typeof(OriginalStateNode))
                {
                    if (_Port.node.GetType() == typeof(ExpressionStateNode))
                    {
                        if (!startPort.connections.ToList().Exists(_x => _x.input.node.GetType() == typeof(ExpressionStateNode)))
                        {
                            _Ports.Add(_Port);
                        }
                    }
                }
                else
                {
                    if (!_Port.connections.ToList().Exists(_x => _x.input.node.GetType() == typeof(ExpressionStateNode)))
                    {
                        _Ports.Add(_Port);
                    }
                }
                //---Check if the port doesn't connect to multiple nodes with the same type
            }
#else
            foreach (Port _Port in ports)
            {
                if (startPort == _Port || startPort.node == _Port.node || startPort.direction == _Port.direction) //Port In Out put basic rules
                {
                    continue;
                }
                //_Ports.Add(_Port);
                //continue;
                //---Check if the port doesn't connect to multiple nodes with the same type
                if (startPort.node.GetType() == typeof(OriginalStateNode))
                {
                    if (_Port.node.GetType() == typeof(ExpressionStateNode))
                    {
                        if (!startPort.connections.ToList().Exists(_x => _x.input.node.GetType() == typeof(ExpressionStateNode)))
                        {
                            _Ports.Add(_Port);
                        }
                    }
                }
                else
                {
                    if (!_Port.connections.ToList().Exists(_x => _x.input.node.GetType() == typeof(ExpressionStateNode)))
                    {
                        _Ports.Add(_Port);
                    }
                }
                //---Check if the port doesn't connect to multiple nodes with the same type

            }
#endif
            return _Ports;
        }

        Port GeneratePort(VIVEFacialTrackingGraphNode _Node, Direction _PortDir, Port.Capacity _Capacity = Port.Capacity.Multi, string _PortName = "")
        {
            Port _Port = _Node.InstantiatePort(Orientation.Horizontal, _PortDir, _Capacity, typeof(float));
            //Port _Port = VIVEFacialTrackingGraphPort.Generate(Orientation.Horizontal, _PortDir, _Capacity);
            _Port.portName = _PortName;
            return _Port;
        }

		void OnKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.Delete)
			{
				//DeleteSelection();
				Debug.Log("OnKeyDown() DeleteSelection:" );
			}
		}

		class ExpStateNodeData
        {
            Node LinkingNode;
            Label Name;
            TextField NameField;
            public bool IsEditting = false;
            public ExpStateNodeData(Node _Node, Label _Name, TextField _Field)
            {
                LinkingNode = _Node;
                Name = _Name;
                NameField = _Field;

                _Name.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (Time.realtimeSinceStartup - LabelSastClick <= 0.25f && !IsRenamingTitle)
                    {
                        _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                        _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        _Field.value = _Name.text.Substring(1);
                        _Field.SelectAll();
                        _Field.Focus();
                        IsRenamingTitle = true;
                    }
                    LabelSastClick = Time.realtimeSinceStartup;
                });
                _Field.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.KeypadEnter)
                    {
                        IsRenamingTitle = false;
                        Name.text = " " + _Field.text;
                        _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    }
                });
                _Field.RegisterCallback<FocusOutEvent>(evt =>
                {
                    IsRenamingTitle = false;
                    Name.text = " " + _Field.text;
                    _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                });
            }

            float LabelSastClick = 0;
            bool IsRenamingTitle = false;
            void RigExpressionStateClickEvt()
            {
                if (Time.realtimeSinceStartup - LabelSastClick <= 0.25f && !IsRenamingTitle)
                {
                    //---Generate TextField
                    TextField _Field = new TextField();
                    _Field.style.width = 150;
                    _Field.value = LinkingNode.title;
                    _Field.SelectAll();
                    LinkingNode.titleContainer.Add(_Field);
                    _Field.Focus();

                    IsRenamingTitle = true;
                    //---Generate TextField
                }
                LabelSastClick = Time.realtimeSinceStartup;
            }
        }

        class BlendShapeStateNodeData
        {
            Node LinkingNode;
            Label Name;
            TextField NameField;
            public BlendShapeStateNodeData(Node _Node, Label _Name, TextField _Field)
            {
                LinkingNode = _Node;
                Name = _Name;
                NameField = _Field;

                _Name.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (Time.realtimeSinceStartup - LabelSastClick <= 0.25f && !IsRenamingTitle)
                    {
                        _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                        _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        _Field.value = _Name.text.Substring(1);
                        _Field.SelectAll();
                        _Field.Focus();
                        IsRenamingTitle = true;
                    }
                    LabelSastClick = Time.realtimeSinceStartup;
                });
                _Field.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.KeypadEnter)
                    {
                        IsRenamingTitle = false;
                        Name.text = " " + _Field.text;
                        _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                        _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    }
                });
                _Field.RegisterCallback<FocusOutEvent>(evt =>
                {
                    IsRenamingTitle = false;
                    Name.text = " " + _Field.text;
                    _Name.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    _Field.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                });
            }

            float LabelSastClick = 0;
            bool IsRenamingTitle = false;
            void RigExpressionStateClickEvt()
            {
                if (Time.realtimeSinceStartup - LabelSastClick <= 0.25f && !IsRenamingTitle)
                {
                    //---Generate TextField
                    TextField _Field = new TextField();
                    _Field.style.width = 150;
                    _Field.value = LinkingNode.title;
                    _Field.SelectAll();
                    LinkingNode.titleContainer.Add(_Field);
                    _Field.Focus();

                    IsRenamingTitle = true;
                    //---Generate TextField
                }
                LabelSastClick = Time.realtimeSinceStartup;
            }
        }

        class OriginalStatePortData
        {
            public Port LinkingPort { get; set; }
            public Image LinkingImage { get; set; }

            public OriginalStatePortData(Port _Port, Image _Img)
            {
                LinkingPort = _Port;
                LinkingImage = _Img;
                LinkingImage.style.width = 0;
                LinkingImage.style.height = 0;
                LinkingImage.style.marginLeft = 100;
				//LinkingImage.style.transformOrigin = new TransformOrigin(Length.Percent(100), Length.Percent(50), 0);

				if (LinkingImage.image != null)
				{
					LinkingPort.RegisterCallback<MouseEnterEvent>(evt =>
						{
						//---Generate Hint Pic
							LinkingImage.style.height = LinkingImage.image.height * 0.35f;
							LinkingImage.style.width = LinkingImage.image.width * 0.35f;
						//---Generate Hint Pic
						});
					_Port.RegisterCallback<MouseLeaveEvent>(evt =>
						{
							//---Remove Hint Pic
							LinkingImage.style.width = 0;
							LinkingImage.style.height = 0;
							//LinkingImage.style.marginLeft = 0;
							//---Remove Hint Pic
						});
				}
            }
        }

   //     private void AddChoicePort(VIVEFacialTrackingGraphNode _Node, string _OverriddenName = "")
   //     {
			//Debug.Log("AddChoicePort(): ");
			//Port _Port = GeneratePort(_Node, Direction.Output);
   //         int _PortCount = _Node.outputContainer.Query(name: "connector").ToList().Count;
   //         _Port.portName = $"Choice {_PortCount}";

   //         string _ChoicePortName = string.IsNullOrEmpty(_OverriddenName) ? $"Choice {_OverriddenName + 1}" : _OverriddenName;

   //         ObjectField _ObjField = new ObjectField
   //         {
   //             objectType = typeof(GameObject),
   //             allowSceneObjects = false,
   //             value = FacialState
   //         };

   //         _ObjField.RegisterValueChangedCallback(ValueTuple =>
   //         {
   //             FacialState = ValueTuple.newValue as GameObject;
   //         });
   //         _ObjField.style.width = new Length(150, LengthUnit.Pixel);
   //         _ObjField.allowSceneObjects = true;


   //         _Port.contentContainer.Add(new Label(" "));
   //         _Port.contentContainer.Add(_ObjField);
   //         Button _DeletButton = new Button(clickEvent: () => RemovePort(_Node, _Port))
   //         {
   //             text = "X"
   //         };

   //         _Port.contentContainer.Add(_DeletButton);
   //         _Port.portName = _ChoicePortName;
   //         _Node.outputContainer.Add(_Port);
   //         _Node.RefreshPorts();
   //         _Node.RefreshExpandedState();
   //     }
   //     private void RemovePort(Node _Node, Port _Port)
   //     {
			//Debug.Log("RemovePort(): ");
			//var targetEdge = edges.ToList()
   //             .Where(x => x.output.portName == _Port.portName && x.output.node == _Port.node);
   //         if (targetEdge.Any())
   //         {
   //             var edge = targetEdge.First();
   //             edge.input.Disconnect(edge);
   //             RemoveElement(targetEdge.First());
   //         }

   //         _Node.outputContainer.Remove(_Port);
   //         _Node.RefreshPorts();
   //         _Node.RefreshExpandedState();
   //     }
    }
#endif
}
#endif
