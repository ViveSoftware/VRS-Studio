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
using Wave.Essence.FacialExpression.Maker;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(VIVEFacialExpressionAdapter))]
public class VIVEFacialExpressionMakerEditor : Editor
{
    public static SerializedProperty MappingConfigProp = null;
	public static SerializedProperty TargetFaceProp = null;
	SerializedProperty TargetFaceAppendProp;
	SerializedProperty BlendShapeOnlyProp;
	private GUIStyle BtnStyle;
	public GameObject TokenMappingConfig { get; set; }
	//public static SerializedObject serializedObject;

	public static VIVEFacialExpressionConfig tmpConfig = null;
	public static void SetMappingConfig(VIVEFacialExpressionConfig _config)
	{
		tmpConfig = _config;
	}

	void OnEnable()
    {
        //Debug.Log("MakerEditor OnEnable() ");
        MappingConfigProp = serializedObject.FindProperty("m_MappingConfig");
        TargetFaceProp = serializedObject.FindProperty("m_TargetFace");
        TargetFaceAppendProp = serializedObject.FindProperty("m_TargetFaceAppend");
		BlendShapeOnlyProp = serializedObject.FindProperty("blendShapeOnly");
	}
	/*
    private void OnSceneGUI()
    {
        BtnStyle = new GUIStyle("button");
        BtnStyle.fontStyle = FontStyle.Bold;
    }
    */
	public override void OnInspectorGUI()
	{
		//Debug.Log("MakerEditor OnInspectorGUI() ");
		if(tmpConfig != null)
			MappingConfigProp.objectReferenceValue = tmpConfig/*VIVEFacialTrackingGraph.CurrentControllerNew*/ as VIVEFacialExpressionConfig;
		Begin_WrapWord();
		Begin_Bold();
		Begin_BiggerFrontSize();
		//EditorGUILayout.LabelField("Note:");
		End_Bold();
		End_BiggerFrontSize();
		Begin_Italic();

		End_Italic();
		Begin_BoldItalic();
		//EditorGUILayout.LabelField("The VIVE Facial Tracking Maker is a tool which helps you bind any model with the data provided by VIVE Facial Tracking.", GUILayout.MinHeight(48));
		EditorGUILayout.LabelField("The Facial Expression Adapter helps you bind the VIVE Facial Expressions to a face model’s blend shapes.\nAssign the target face object before creating the facial expression config for the face object.", GUILayout.MinHeight(72));
		End_BoldItalic();

		EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(TargetFaceProp);
        EditorGUILayout.PropertyField(MappingConfigProp);
        EditorGUILayout.PropertyField(TargetFaceAppendProp);
		EditorGUILayout.PropertyField(BlendShapeOnlyProp);
		if (EditorGUI.EndChangeCheck()){
			if (MappingConfigProp != null && MappingConfigProp.objectReferenceValue != null)
				Debug.Log("MakerEditor OnInspectorGUI() mapping config changed:"+ MappingConfigProp.objectReferenceValue.ToString());
			tmpConfig = null;
		}
		serializedObject.ApplyModifiedProperties();
		    

		string btnName = "Open Facial Expression Make Editor";
		if (MappingConfigProp != null && MappingConfigProp.objectReferenceValue != null)
		{
			btnName = "Open Facial Expression Make Editor";
		}
		else
		{
			btnName = "Create BlendShape Mapping Config";
		}
		if (TargetFaceProp != null && TargetFaceProp.objectReferenceValue != null)
		{
			//if (GUILayout.Button("Open Controller", BtnStyle, GUILayout.MaxWidth(120), GUILayout.MinHeight(25)))
			if (GUILayout.Button(btnName, ((TargetFaceProp != null) ? GUI.skin.button : GUIStyle.none), GUILayout.MaxWidth(216), GUILayout.MinHeight(25)))
			{
				VIVEFacialExpressionAdapter.Prepareblendshapes();
				VIVEFacialExpressionAdapter.targetFaceStr = TargetFaceProp.name.ToString();
				//GameObject GameObjectProp = GameObject.Find("Face");
#if UNITY_2020_3_OR_NEWER
				VIVEFacialTrackingGraph.TargetFacial = TargetFaceProp.objectReferenceValue as GameObject;
				VIVEFacialTrackingGraph.OpenWindow();
				VIVEFacialTrackingGraph.LoadFacialTrackingController(MappingConfigProp.objectReferenceValue as VIVEFacialExpressionConfig, (target as MonoBehaviour).gameObject, TargetFaceProp.objectReferenceValue as GameObject);
				VIVEFacialTrackingGraph.TokenForController.value = MappingConfigProp.objectReferenceValue;
#endif
			}
		}
		else {
			GUI.enabled = false;
			GUILayout.Button(btnName, GUI.skin.button, GUILayout.MaxWidth(216), GUILayout.MinHeight(25));
		}

		Begin_WrapWord();
        Begin_Bold();
        Begin_BiggerFrontSize();
        //EditorGUILayout.LabelField("Note:");
        End_Bold();
        End_BiggerFrontSize();
        Begin_Italic();
        //EditorGUILayout.LabelField("This component aims for making a GameObject behave like a screen space UI in the MR/VR world.");
        End_Italic();
        Begin_BoldItalic();
        //EditorGUILayout.LabelField("In order for the GameObject to be seen in the game, please search the Camera_for_UILike prefab under in the assets and place it under your main camera as a child.");
        End_BoldItalic();

		//base.OnInspectorGUI();
	}

    void Begin_WrapWord()
    {
        EditorStyles.label.wordWrap = true;
    }

    void End_WrapWord()
    {
        EditorStyles.label.wordWrap = false;
    }

    void Begin_BiggerFrontSize()
    {
        EditorStyles.label.fontSize += 2;
    }

    void End_BiggerFrontSize()
    {
        EditorStyles.label.fontSize -= 2;
    }

    void Begin_Bold()
    {
        EditorStyles.label.fontStyle = FontStyle.Bold;
    }

    void End_Bold()
    {
        EditorStyles.label.fontStyle = FontStyle.Normal;
    }

    void Begin_Italic()
    {
        EditorStyles.label.fontStyle = FontStyle.Italic;
    }

    void End_Italic()
    {
        EditorStyles.label.fontStyle = FontStyle.Normal;
    }

    void Begin_BoldItalic()
    {
        EditorStyles.label.fontStyle = FontStyle.BoldAndItalic;
    }

    void End_BoldItalic()
    {
        EditorStyles.label.fontStyle = FontStyle.Normal;
    }
}
#endif
