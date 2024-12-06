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
using System.Linq;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Wave.Essence.FacialExpression.Maker
{
#if UNITY_2020_3_OR_NEWER
	public class VIVEFacialTrackingGraph : EditorWindow
    {
        public static VIVEFacialExpressionController CurrentController;
		public static VIVEFacialExpressionConfig CurrentControllerNew;
		public static VIVEFacialTrackingGraphView FTGraphView;
		//public static VIVEFacialTrackingGraphView FTGraphViewCustom;
		//public static CustomStateNodeMapping[] CustomContainers;
		public static List<CustomStateNodeMapping> CustomContainers;
		public static ObjectField TokenForController;
        public static EditorWindow FTWindow;
		public static DataMode UsingMode = DataMode.Rig;
		public static GameObject TargetAvatar;
		public static GameObject TargetFacial;
		static List<Edge> AllEdges;//= FTGraphView.edges.ToList();


		private ScrollView scrollView;
		private GUIStyle labelStyle;
		private static GUIContent editorContent = new GUIContent("FacialExpressionMakeEditor");
		private Vector2 scrollPosition = Vector2.zero;
		private static List<Node> generatedNodesTmp = new List<Node>();
		private static string mappingconfigFile = null;

		public static void ShowWindow()
		{
			var scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			scrollView.style.width = 250;
			scrollView.style.height = 400;
			scrollView.Add(new Label("List of checkboxes:"));
			for (int i = 0; i < 100; ++i)
			{
				var toggle = new UnityEngine.UIElements.Toggle()
				{ text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." };
				scrollView.Add(toggle);
			}

			EditorWindow.GetWindow(typeof(VIVEFacialTrackingGraph));
		}

        public static void OpenWindow()
        {
			GUILayout.Label(editorContent, GUILayout.MaxWidth(500));
			EditorGUILayout.LabelField(editorContent, GUILayout.MaxWidth(500));
			//Debug.Log("OpenWindow");
			if (FTWindow == null)
            {
                VIVEFacialTrackingGraph _Window = GetWindow<VIVEFacialTrackingGraph>();
				_Window.titleContent = editorContent; //new GUIContent("FacialExpressionMakerEditor");

				_Window.scrollView = new ScrollView();
				_Window.scrollView.verticalScroller.style.flexBasis = new StyleLength(0f);
				_Window.scrollView.style.width = 500;
				_Window.scrollView.style.height = 300;
				FTWindow = _Window;
				CurrentControllerNew = null;
                Debug.Log("Null FT CurrentController null!?");

			}
		}

		public void CreateGUI()
        {
			//Debug.Log("VIVEFacialTrackingGraph CreateGUI");
			// Each editor window contains a root VisualElement object
			VisualElement _Root = rootVisualElement;

			// VisualElements objects can contain other VisualElement following a tree hierarchy.
			//VisualElement _Label = new Label("Hello World! From C#");
			//_Root.Add(_Label);

		}

		void OnGUI()
		{
			GUILayout.Label(editorContent, labelStyle);
			// create ScrollView and config size
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(200), GUILayout.Height(300));
			// end ScrollView area
			EditorGUILayout.EndScrollView();

			GetTmpAllNodes();
			if (Event.current.type == EventType.KeyDown)
			{
				Debug.Log("Received keyboard input: " + Event.current.keyCode);
				Event.current.Use();
			}

		}

		public void onDisable()
		{
			//Debug.Log("VIVEFacialTrackingGraph onDisable()");
		}
		private void OnEnable()
        {
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.fixedWidth = 500;
			GUILayout.Label(editorContent, GUILayout.MaxWidth(500));
			EditorGUILayout.LabelField(editorContent, GUILayout.MaxWidth(500));
			//Debug.Log("VIVEFacialTrackingGraph OnEnable");
			if (FTWindow == null)
            {
                VIVEFacialTrackingGraph _Window = GetWindow<VIVEFacialTrackingGraph>();
				_Window.titleContent = editorContent; //new GUIContent("FacialExpressionMakerEditor");
				FTWindow = _Window;
                CurrentControllerNew = null;
			}


			ConstructGraphView();
            FTGraphView.AddElement(FTGraphView.GenerateOriginalStateNodeNew());

			//ConstructGraphViewCustom();
			//FTGraphViewCustom.AddElement(FTGraphViewCustom.GenetateCustomStateNode(TargetFacial));

			GenerateToolbar();
			//GenerateBlackBoard();

		}

        VIVEFacialTrackingGraphView ConstructGraphView()
        {
            FTGraphView = new VIVEFacialTrackingGraphView
            {
                name = "FTGraphView"
            };
            FTGraphView.StretchToParentSize();
			rootVisualElement.Add(FTGraphView);
            return FTGraphView;
        }


		void GenerateToolbar()
        {
            Toolbar _Toolbar = new Toolbar();
            Button _Button;

			//--- Generate Blend Expression Button
			//_Button = new Button(clickEvent: () => { FTGraphView.AddElement(FTGraphView.GenerateBlendShapeStateNode("BlendShape Expression")); });
			//_Button.text = "Add BlendShape Expression";
			//_Toolbar.Add(_Button);
			//--- Generate Blend Expression Button

			//--- Generate Load Button
			//_Button = new Button(clickEvent: () =>
			//{
			//    /*Loading*/
			//    if(CurrentController == null)
			//    {
			//        Debug.LogError("No Controller is provided!");
			//    }
			//    LoadFacialTrackingController(CurrentController);
			//});
			//_Button.text = "Load";
			//_Toolbar.Add(_Button);
			//--- Generate Load Button

			//------Genetate Token for Controller
			ObjectField _ObjField = new ObjectField
            {
                objectType = typeof(VIVEFacialExpressionConfig),
            };
            _ObjField.SetEnabled(false);
            //_ObjField.RegisterValueChangedCallback(ValueTuple =>
            //{
            //    if(CurrentController == ValueTuple.newValue as VIVEFacialExpressionController)
            //    {
            //        return;
            //    }
            //    CurrentController = ValueTuple.newValue as VIVEFacialExpressionController;
            //    TokenForController.value = CurrentController;
            //    if (CurrentController != null)
            //    {
            //        LoadFacialTrackingController(CurrentController, null);
            //    }
            //});
            TokenForController = _ObjField;
            _Toolbar.Add(_ObjField);
            //------Genetate Token for Controller

            //--- Generate Rig Expression Button
            _Button = new Button(clickEvent: () => {
                //Debug.Log("Button:Auto Expression" + TargetFacial.name.ToString());
                //FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNode("Expression Status")); //Single ExpressionStatus Node

                GetTmpAllNodes();
                if ((GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>() == null))
                {
                    Debug.Log("Button:Auto Expression null SkinnedMeshRenderer");
                    return;
                }


				if (CurrentControllerNew == null)
				{

					// each one node 51 port value for wave expression, totoal node by TargetFacial blendshapecount
					for (int i = 0; i < GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; i++)
					{
					    var result = from r in generatedNodesTmp where (r.title.ToString() == GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(i).ToString()) select r;

						if (result.Any())
							continue;
						else 
						{
							//GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount
						    VIVEFacialTrackingGraphNode newNode = FTGraphView.GenerateExpressionStateNodeCustomNew(TargetFacial, i);
						    FTGraphView.AddElement(newNode);//FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNode(TargetFacial.name, i));

						    //FTGraphView.ClearSelection();
						    //FTGraphView.AddToSelection(newNode);
						    //generatedNodes.Add(new Node(name= GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(i)));
					    }
				    }
				}
				else {
                    //Debug.Log("Button:Auto add Expression(has config):" + TargetFacial.name.ToString());
                    for (int i = 0; i < GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount; i++)
					{

						var result1 = from r in CurrentControllerNew.GeneratedNodeNew.TrackingExpNodes where (r.OriginalBSNodeName == GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(i).ToString()) select r;
						var result2 = from r in generatedNodesTmp where (r.title.ToString() == GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(i).ToString()) select r;

						if (result2.Any())
							continue;
						//else if (result1.Any())
						//	continue;
						else
							FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNode(GameObject.Find(TargetFacial.name.ToString()).GetComponent<SkinnedMeshRenderer>().sharedMesh.GetBlendShapeName(i).ToString(), i, new List<float>() {0.0f}));
					}
				}


			});
            _Button.text = "Auto add blendShapes";
            _Toolbar.Add(_Button);
			//--- Generate Rig Expression Button


			////--- Generate Add one Expression Button
			//_Button = new Button(clickEvent: () =>
			//{
			//	Debug.Log("Button:Add Expression" + TargetFacial.name.ToString());
			//	FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNodeOne("NewExpression", 0, 0.0f)); //Single ExpressionStatus Node

			//	//FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNode("Expression Status", 0));
			//});
			//_Button.text = "3.Add Expression";
			//_Toolbar.Add(_Button);
			////--- Generate Add one Expression Button


			//--- Generate Save Button
			_Button = new Button(clickEvent: () =>{
				GetTmpAllNodes();
				//SaveAvatarBlendShape();
				if (CurrentControllerNew == null)
				{
					//FTGraphView.OriginalNode.OriginalRigStatus = CreateAssetAuto<VIVEOriginalExpressionStatus>(_AssetName: "VIVEFacialExpressionBlendShape");//OriginalExpressionStatus
                    ////Debug.Log(FTGraphView.OriginalNode.OriginalRigStatus);
                    //FTGraphView.OriginalNode.TokenForOriginalRigStatus.value = FTGraphView.OriginalNode.OriginalRigStatus;

				}
				//if (FTGraphView.OriginalNode.OriginalRigStatus != null)
				{
					//FTGraphView.SetOriginalExpressionStatus(TargetAvatar, FTGraphView.OriginalNode.OriginalRigStatus);

					/*PrintConnectedPortsInfo();*//*Saving*/
					SaveFacialTrackingController();

					VIVEFacialExpressionMakerEditor.SetMappingConfig(CurrentControllerNew);
				}

			});
			_Button.text = "Save mapping config";
            _Toolbar.Add(_Button);
            //--- Generate Save Button

            rootVisualElement.Add(_Toolbar);
		}

		public void GetTmpAllNodes()
		{
			generatedNodesTmp = new List<Node>();
			// get all exist Node
			var nodes = FTGraphView.Query<Node>().ToList();

			foreach (var node in nodes)
			{
				//Debug.Log(i+", Found Node: " + node.title);
				if (node is ExpressionStateNode cNode)
				{
					generatedNodesTmp.Add(node);
				}
			}
			//return generatedNodeNew;
		}
		void PrintConnectedPortsInfo(/*Edge edge*/)
		{
			List<Edge> _AllEdges = FTGraphView.edges.ToList();
			List<LinkEdgeData> _AllEdgeData = new List<LinkEdgeData>();
			for (int i = 0; i < _AllEdges.Count; i++)
			{
				Port outputPort = _AllEdges[i].output as Port;
				Port inputPort = _AllEdges[i].input as Port;
				Node nodeA = outputPort.node as Node;
				ExpressionStateNode nodeB = (inputPort.node as ExpressionStateNode);//Node

				string nodeAName = nodeA.title;//nodeA.title;
				string nodeBName = nodeB.title;//nodeB.title;
				string outputPortName = outputPort.portName;
				string inputPortName = inputPort.portName;

				var result = from r in nodeB.NameLabelSet where r.name == (inputPortName) select r;
				Debug.Log($"Edge connects port(out) '{outputPortName}' of nodeA '{nodeAName}' to port(in) '{inputPortName}' of nodeB '{nodeBName}'"
					+ ", retrieve the inputport value:" + result.First().text);
				_AllEdgeData.Add(new LinkEdgeData(nodeA, nodeB, outputPort, inputPort, inputPortName, Convert.ToSingle(result.First().text.Replace("Weight:", "").Trim()) , nodeB.GetPosition()));
				//for (int dg = 0; dg < nodeB.NameLabelSet.Count; dg++)
				//Debug.Log($"Edge connects port '{nodeBName}' , '{nodeB.NameLabelSet[dg].name}' , '{nodeB.NameLabelSet[dg].text}'");
			}
		}
		public void SaveAvatarBlendShape()
		{
			//if (_Node.Target == null)
			//{
			//	Debug.LogError("There is no target!");
			//	return;
			//}
			//else if (_Node.OriginalRigStatus == null)
			if (CurrentControllerNew == null)
			{
				//FTGraphView.OriginalNode.OriginalRigStatus = CreateAssetAuto<VIVEOriginalExpressionStatus>(_AssetName: "VIVEFacialExpressionBlendShape");//OriginalExpressionStatus
				//Debug.Log(FTGraphView.OriginalNode.OriginalRigStatus);
				//if (FTGraphView.OriginalNode.OriginalRigStatus != null)
				//{
				//	FTGraphView.OriginalNode.TokenForOriginalRigStatus.value = FTGraphView.OriginalNode.OriginalRigStatus;
				//	FTGraphView.SetOriginalExpressionStatus(TargetAvatar, FTGraphView.OriginalNode.OriginalRigStatus);
				//}
			}

		}
		public GeneratedNodeNew GetandSaveAllNodesAndPorts()
		{
			GeneratedNodeNew generatedNodeNew = new GeneratedNodeNew();
			generatedNodeNew.TrackingExpNodes = new List<TrackNodeNew>();
			// get all exist Node
			var nodes = FTGraphView.Query<Node>().ToList();

			foreach (var node in nodes)
			{
				//Debug.Log("Found Node: " + node.title);
				if (node is ExpressionStateNode cNode)
				{
					var connected = false;
					List<TrackNodeInfoBS> InfoSets = new List<TrackNodeInfoBS>();
					//Debug.Log("custom Node");
					
					// get all Node and all Port
					var inputPorts = cNode.inputContainer.Query<Port>().ToList();
					foreach (var port in inputPorts)
					{
						//Debug.Log("custom Node:"+node.title+", Input Port: " + port.portName);

						// check if edge connect to the input port
						var edges = FTGraphView.edges.ToList();
						connected = edges.Any(e => e.input.portName == port.portName && e.input.node.title == port.node.title);
						if (connected)
						{
							//Debug.Log("Input Port is connected.");
						}
					}

					// get all Node outputContainer Label weight value
					var labels = cNode.outputContainer.Query<Label>().ToList();
					//foreach (var label in labels) {
					//	Debug.Log("Label Value: " + label.text);
					//}

					for (int i = 0; i < labels.Count; i++) {
						InfoSets.Add(new TrackNodeInfoBS(Convert.ToSingle(labels[i].text.Replace("Weight:", "").Trim()), inputPorts[i].portName));
					}

					//if (connected)
					    generatedNodeNew.TrackingExpNodes.Add(new TrackNodeNew(cNode.title, InfoSets, cNode.GetPosition(), connected));
					//else
						//generatedNodeNew.TrackingExpNodes.Add(new TrackNodeNew(cNode.title, null, cNode.GetPosition(), connected));
				}


				//if (node is OriginalStateNode oNode) Debug.Log("original Node");

				// 取得 Node 的所有 Port
				//var inputPorts = node.inputContainer.Query<Port>().ToList();
				//var outputPorts = node.outputContainer.Query<Port>().ToList();
				//foreach (var port in inputPorts)
				//{
				//	Debug.Log("Input Port: " + port.portName);
				//	// 判斷是否有 edge 連接此 input port
				//	var edges = FTGraphView.edges.ToList();
				//	var connected = edges.Any(e => e.input.portName == port.portName && e.input.node == port.node);
				//	if (connected)
				//	{
				//		Debug.Log("Input Port is connected.");
				//	}
				//}
				//foreach (var port in outputPorts)
				//{
				//	Debug.Log("Output Port: " + port.portName);
				//}

			}
			return generatedNodeNew;
		}
		void SaveFacialTrackingController()
		{
			//for(int i = 0; i < generatedNodes.Count; i++)
			//Debug.Log("SaveFacialTrackingController():"+ generatedNodes[i].name);

			//Debug.Log("SaveFacialTrackingController(): outputport: " + FTGraphView.OriginalNode.OutPutPorts.Length);
			List <Edge> _AllEdges = FTGraphView.edges.ToList();
			List<LinkEdgeData> _AllEdgeData = new List<LinkEdgeData>();
			for (int i = 0; i < _AllEdges.Count; i++)
			{
				//Debug.Log("SaveFacialTrackingController():"+ _AllEdges.Count);
				Port outputPort = _AllEdges[i].output as Port;
				Port inputPort = _AllEdges[i].input as Port;
				Node nodeA = outputPort.node as Node;
				ExpressionStateNode nodeB = (inputPort.node as ExpressionStateNode);//Node

				string nodeAName = nodeA.title;//nodeA.title;
				string nodeBName = nodeB.title;//nodeB.title;
				string outputPortName = outputPort.portName;
				string inputPortName = inputPort.portName;

				var result = from r in nodeB.NameLabelSet where r.name == (inputPortName) select r;
				//Debug.Log($"Edge connects port(out) '{outputPortName}' of nodeA '{nodeAName}' to port(in) '{inputPortName}' of nodeB '{nodeBName}'"+ ", retrieve the inputport value:" + result.First().text);
				_AllEdgeData.Add(new LinkEdgeData(nodeA, nodeB, outputPort, inputPort, inputPortName, Convert.ToSingle(result.First().text.Replace("Weight:", "").Trim()) , nodeB.GetPosition()));

				//var delresult = from r in tmpNodes where tmpNodes.name == (inputPortName) select r;
			}

			VIVEFacialExpressionConfig _ControllerNew;
			if (CurrentControllerNew == null)
			{
				_ControllerNew = CreateAsset<VIVEFacialExpressionConfig>(_AssetName: "VIVEFacialExpressionConfig");//New Controller
				Debug.Log("SaveFacialTrackingController(): CurrentControllerNew is null");
			}
			else
			{
				_ControllerNew = CurrentControllerNew;
			}
			_ControllerNew.OriginalNodeNew = new OriginalNodeNew();
			_ControllerNew.OriginalNodeNew.Position = FTGraphView.OriginalNode.GetPosition();
			//_ControllerNew.OriginalNodeNew.OriginalRigStatus = FTGraphView.OriginalNode.OriginalRigStatus;
			_ControllerNew.GeneratedNodeNew = GetandSaveAllNodesAndPorts();

			for (int i = 0; i < FTGraphView.OriginalNode.OutPutPorts.Length; i++)
			{
				List<JointGapInfoBS> BSSets = new List<JointGapInfoBS>();
				for (int j = 0; j < _AllEdgeData.Count; j++) {

					//foreach (Edge _Edge in FTGraphView.OriginalNode.OutPutPorts[j].connections.ToList())
					if (FTGraphView.OriginalNode.OutPutPorts[i].portName == _AllEdgeData[j].OutPort.portName)
                    {
						BSSets.Add(new JointGapInfoBS(_AllEdgeData[j].InNode.NodeName, _AllEdgeData[j].weight, _AllEdgeData[j].InNode.BlendShapeId, _AllEdgeData[j].InPortName, _AllEdgeData[j].position));
						//EditorUtility.SetDirty(_ControllerNew.OriginalNodeNew.LinkingExpNodes[j].ExpStatus);
						_ControllerNew.OriginalNodeNew.LinkingExpNodes[i] = new RigNodeNew(FTGraphView.OriginalNode.OutPutPorts[i].portName, BSSets/*, _AllEdgeData[j].InNode.GetPosition()*/);
						//Debug.Log("InNode:"+ _AllEdgeData[j].InNode.NodeName+"Window Pos: " + _AllEdgeData[j].InNode);
					}


				}
			}

			EditorUtility.SetDirty(_ControllerNew);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			if (TokenForController.value != _ControllerNew || CurrentControllerNew != _ControllerNew)
			{
				TokenForController.value = _ControllerNew;
				CurrentControllerNew = _ControllerNew;
			}

		}


		public static void LoadFacialTrackingController(VIVEFacialExpressionConfig _Controller, GameObject _Target, GameObject _TargetFacial) {
			TargetAvatar = _Target;
			//Vector2 DefaultNodeSize = new Vector2(150, 200);
			List<ExpressionStateNode> nodes = new List<ExpressionStateNode>();
			List<ExpressionStateNodeReserve> reserveData = new List<ExpressionStateNodeReserve>();
			FTGraphView.OriginalNode.parent.Remove(FTGraphView.OriginalNode);
			FTGraphView.AddElement(FTGraphView.GenerateOriginalStateNodeNew());
			if (_Controller != null)
			{
				//FTGraphView.OriginalNode.OriginalRigStatus = _Controller.OriginalNodeNew.OriginalRigStatus;
				Debug.Log("LoadFacialTrackingController2():  controller not null");
			}
			else { Debug.Log("LoadFacialTrackingController2():  controller null"); }

			if(_Target == null) { Debug.Log("LoadFacialTrackingController2():  target null"); }
			FTGraphView.OriginalNode.Target = _Target;
			FTGraphView.OriginalNode.TokenForTarget.value = _Target;
			FTGraphView.OriginalNode.TargetFace = _TargetFacial;
			FTGraphView.OriginalNode.TokenForTargetFace.value = _TargetFacial;
			CurrentControllerNew = _Controller;
			//FTGraphView.OriginalNode.Target = _Controller.Target;
			if (_Controller != null)
			{
				//FTGraphView.OriginalNode.TokenForOriginalRigStatus.value = _Controller.OriginalNodeNew.OriginalRigStatus;
				FTGraphView.OriginalNode.SetPosition(_Controller.OriginalNodeNew.Position);
                //FTGraphView.OriginalNode.SetPosition(new Rect(_Controller.OriginalNodeNew.Position.x, _Controller.OriginalNodeNew.Position.y+10, _Controller.OriginalNodeNew.Position.width, _Controller.OriginalNodeNew.Position.height));
			}
			/************* OriginalNode link to each Expression Node(right part) **************/


#if UNITY_2020_3_OR_NEWER
			List<Node> _AllNodes = FTGraphView.nodes.ToList();
			for (int i = 0; i < _AllNodes.Count; i++)
			{
				if (_AllNodes[i] != FTGraphView.OriginalNode)
				{
					_AllNodes[i].parent.Remove(_AllNodes[i]);
				}
			}
#else
            foreach (Node _Node in FTGraphView.nodes)
            {
                if (_Node != FTGraphView.OriginalNode)
                {
                    _Node.parent.Remove(_Node);
                }
            }
#endif
#if UNITY_2020_3_OR_NEWER
			List<Edge> _AllEdges = FTGraphView.edges.ToList();
			for (int i = 0; i < _AllEdges.Count; i++)
			{
				//Debug.Log("LoadFacialTrackingController2() out:" + _AllEdges[i].output.name + "input:" + _AllEdges[i].input.name);

				_AllEdges[i].output.Disconnect(_AllEdges[i]);
				_AllEdges[i].input.Disconnect(_AllEdges[i]);
				_AllEdges[i].parent.Remove(_AllEdges[i]);
			}
#else
            foreach (Edge _Edge in FTGraphView.edges)
            {
                _Edge.output.Disconnect(_Edge);
                _Edge.input.Disconnect(_Edge);
                _Edge.parent.Remove(_Edge);
            }
#endif

			if (_Controller != null)
			{
				for (int i = 0; i < _Controller.OriginalNodeNew.LinkingExpNodes.Length; i++)
				{
					//Debug.Log("LoadFacialTrackingController2() out:" + _Controller.OriginalNodeNew.LinkingExpNodes.Length);
					//Edge _Edge;
					if (_Controller.OriginalNodeNew.LinkingExpNodes[i].NotNull)
					{
						//ExpressionStateNode _Node = FTGraphView.GenerateExpressionStateNode();
						for (int j = 0; j < _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets.Count; j++)
						{
							var result = from r in reserveData where (r.name == _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName) select r;
							if (!result.Any())
							{
								//Debug.Log("no match node name:" + _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName + ", port:" + Convert.ToInt32(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Inport.Replace("Element", "")));
								ExpressionStateNodeReserve data1 = new ExpressionStateNodeReserve();
								data1.name = _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName;
								data1.AddWeight(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Weight, data1.name, Convert.ToInt32(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Inport.Replace("Element", "")));
								data1.id = _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSId;
								data1.position = _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].poistion;
								reserveData.Add(data1);
							}
							else
							{
								result.First().AddWeightByName(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName, _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Weight, Convert.ToInt32(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Inport.Replace("Element", "")));
								//foreach (var d in reserveData) {
								//	d.AddWeightByName(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName, _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Weight);
								//}
							}
						}//end of j
					}
				}//end of i


				//Debug.Log("Printing all data:"); //reload the custom nodes
				for (int m = 0; m < reserveData.Count; m++) //foreach (var d in reserveData)
				{
					var sortedWeightList = reserveData[m].weightList.OrderBy(x => x.Key);
					reserveData[m].weightList = sortedWeightList.ToDictionary(x => x.Key, x => x.Value);
					//for (int n = 0; n < reserveData[m].weightList.Keys.ToList<string>().Count; n++)
					//Debug.Log("collect reserveData:"+ reserveData[m].name+", "+ reserveData[m].id+", List:"+n+" ( "+ reserveData[m].weightList.Values.ToList<float>()[n]+", "+reserveData[m].weightList.Keys.ToList<string>()[n]+")");
					ExpressionStateNode _Node = FTGraphView.GenerateExpressionStateNode(reserveData[m].name, reserveData[m].id, reserveData[m].weightList.Values.ToList<float>());
					_Node.SetPosition(reserveData[m].position);
					//_Node.SetPosition(new Rect(reserveData[m].position.x +  * 120, reserveData[m].position.y, DefaultNodeSize.x, DefaultNodeSize.y));
					_Node.NodeName = reserveData[m].name;
					nodes.Add(_Node);

					FTGraphView.AddElement(_Node);
				}
				// For debug (nodename, portname, weight*)
				//for (int i = 0; i < nodes.Count; i++) //Debug.Log("Collect node name: " + nodes[i].NodeName + ", Title:" + nodes[i].GUID);
				//	for (int j = 0; j < nodes[i].InPorts.Count; j++)
				//		Debug.Log("Collect node name(**): " + nodes[i].NodeName + ", Title:" + nodes[i].GUID + ", portName:" + nodes[i].InPorts[j].name + ", count:" + nodes[i].InPorts.Count);


				for (int i = 0; i < _Controller.OriginalNodeNew.LinkingExpNodes.Length; i++)
				{
					//Debug.Log("LoadFacialTrackingController2Teest() out:" + _Controller.OriginalNodeNew.LinkingExpNodes.Length+",nodes count:"+ nodes.Count);
					if (_Controller.OriginalNodeNew.LinkingExpNodes[i].NotNull)
					{
						for (int j = 0; j < _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets.Count; j++)
						{
							var result = from r in nodes where (r.NodeName == _Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].BSName) select r as ExpressionStateNode;
							if (result.Any())
							{
								//Debug.Log("LoadFacialTrackingController2Teest() match node(Edge):" + result.First().NodeName+ ",inport:"+ Convert.ToInt32(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Inport.Replace("Element", "")));
								Edge _Edge = FTGraphView.OriginalNode.OutPutPorts[i].ConnectTo(result.First().InPorts[Convert.ToInt32(_Controller.OriginalNodeNew.LinkingExpNodes[i].BlendShapeSets[j].Inport.Replace("Element", ""))]);
								FTGraphView.AddElement(_Edge);
							}
							//else Debug.Log("LoadFacialTrackingController2Teest() no match node find:");
						}
					}
				}

				// Generated and after saved and no linking status node
				for (int i = 0; i < _Controller.GeneratedNodeNew.TrackingExpNodes.Count; i++)
				{
					if (!_Controller.GeneratedNodeNew.TrackingExpNodes[i].IsLinking)
						FTGraphView.AddElement(FTGraphView.GenerateExpressionStateNode(_Controller.GeneratedNodeNew.TrackingExpNodes[i].Position, _Controller.GeneratedNodeNew.TrackingExpNodes[i].OriginalBSNodeName, 0, _Controller.GeneratedNodeNew.TrackingExpNodes[i].BlendShapeSets));
				}

			}
		}


		T CreateAssetAuto<T>(string _AssetName = "new FacialExpressionController", string _Path = "VIVEFacialExpressionMakerAsset") where T : ScriptableObject
		{
			//ScriptableObject _Asset = ScriptableObject.CreateInstance<ScriptableObject>();
			T _Asset = ScriptableObject.CreateInstance<T>();

			// show save file dialog and let user set the file name and path
			string absoluteFilePath = EditorUtility.SaveFilePanel(
				"Save Asset", //Dialog Title
				"Assets", //Parent folder name
				"VIVEFacialExpressionConfig.asset", //Default Asset file name
				"asset"
			);

			// save data to asset file
			if (!string.IsNullOrEmpty(absoluteFilePath))
			{
				Debug.Log("CreateAssetAuto():" + absoluteFilePath + ", AppPath:" + Application.dataPath);
				// change absoluteFilePath tom mapping "Assets" folder path
				string relativePath = absoluteFilePath.Replace(Application.dataPath, "Assets");
				mappingconfigFile = relativePath;
				//Debug.Log("CreateAssetAuto() mappingconfigFile:" + mappingconfigFile );

				AssetDatabase.CreateAsset(_Asset, /*relativePath*/relativePath.Replace(".asset", "BlendShape.asset"));
				EditorUtility.SetDirty(_Asset);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
				return null;
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = _Asset;
			return _Asset;
		}
        T CreateAsset<T>(string _AssetName = "new FacialExpressionController", string _Path = "VIVEFacialExpressionMakerAsset") where T : ScriptableObject
        {
            T _Asset = ScriptableObject.CreateInstance<T>();
            //if (!AssetDatabase.IsValidFolder($"Assets/{_Path}"))
            //{
            //	AssetDatabase.CreateFolder("Assets", _Path);
            //}

            // show save file dialog and let user set the file name and path
            string absoluteFilePath = EditorUtility.SaveFilePanel(
                "Save Asset", //Dialog Title
                "Assets", //Parent folder name
                "VIVEFacialExpressionConfig.asset", //Default Asset file name
                "asset"
            );

            //if (!string.IsNullOrEmpty(mappingconfigFile))
            if (!string.IsNullOrEmpty(absoluteFilePath))
            {
                //Debug.Log("CreateAsset() mappingconfigFile:" + mappingconfigFile);
                //AssetDatabase.CreateAsset(_Asset, mappingconfigFile);
                //must to map to filterConfig.yml
                string relativePath = absoluteFilePath.Replace(Application.dataPath, "Assets");
                AssetDatabase.CreateAsset(_Asset, relativePath);
                EditorUtility.SetDirty(_Asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
				return null;
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _Asset;
            //mappingconfigFile = null;
            return _Asset;
        }


		private void OnDisable()
        {
            rootVisualElement.Remove(FTGraphView);
			//rootVisualElement.Remove(FTGraphViewCustom);
		}
    }
#endif
}
#endif
