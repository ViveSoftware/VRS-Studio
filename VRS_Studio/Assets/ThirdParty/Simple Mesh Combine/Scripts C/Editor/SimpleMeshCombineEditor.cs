/*
	Simple Mesh Combine
	Copyright Unluck Software	
 	www.chemicalbliss.com
*/

using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEditor;

[CustomEditor(typeof(SimpleMeshCombine))]

[System.Serializable]
public class SimpleMeshCombineEditor : Editor {
	public Texture titleTexture;

	public void ExportMesh(MeshFilter meshFilter, string folder, string filename) {
		string path = SaveFile(folder, filename, "obj");
		if (path != null) {
			StreamWriter sw = new StreamWriter(path);
			sw.Write(MeshToString(meshFilter));
			sw.Flush();
			sw.Close();
			AssetDatabase.Refresh();
			Debug.Log("Exported OBJ file to folder: " + path);
		}
	}

	public string MeshToString(MeshFilter meshFilter) {
		Mesh sMesh = meshFilter.sharedMesh;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("g ").Append(meshFilter.name).Append("\n");
		foreach (Vector3 vert in sMesh.vertices) {
			Vector3 tPoint = meshFilter.transform.TransformPoint(vert);
			stringBuilder.Append(String.Format("v {0} {1} {2}\n", -tPoint.x, tPoint.y, tPoint.z));
		}
		stringBuilder.Append("\n");
		foreach (Vector3 norm in sMesh.normals) {
			Vector3 tDir = meshFilter.transform.TransformDirection(norm);
			stringBuilder.Append(String.Format("vn {0} {1} {2}\n", -tDir.x, tDir.y, tDir.z));
		}
		stringBuilder.Append("\n");
		foreach (Vector3 uv in sMesh.uv) {
			stringBuilder.Append(String.Format("vt {0} {1}\n", uv.x, uv.y));
		}
		for (int material = 0; material < sMesh.subMeshCount; material++) {
			stringBuilder.Append("\n");
			int[] tris = sMesh.GetTriangles(material);
			for (int i = 0; i < tris.Length; i += 3) {
				stringBuilder.Append(String.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", tris[i] + 1, tris[i + 1] + 1, tris[i + 2] + 1));
			}
		}
		return stringBuilder.ToString();
	}

	public string SaveFile(string folder, string name, string type) {
		string newPath = "";
		string path = EditorUtility.SaveFilePanel("Select Folder ", folder, name, type);
		if (path.Length > 0) {
			if (path.Contains("" + Application.dataPath)) {
				string s = "" + path + "";
				string d = "" + Application.dataPath + "/";
				string p = "Assets/" + s.Remove(0, d.Length);
				bool cancel = false;
				if (cancel) Debug.Log("Canceled");
				newPath = p;
			} else {
				Debug.LogError("Prefab Save Failed: Can't save outside project: " + path);
			}
		}
		return newPath;
	}


	public override void OnInspectorGUI() {
		var target_cs = (SimpleMeshCombine)target;
		GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
		GUIStyle buttonStyle2 = new GUIStyle(GUI.skin.button);
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.fixedWidth = 150.0f;
		buttonStyle.fixedHeight = 35.0f;
		buttonStyle.fontSize = 15;
		buttonStyle2.fixedWidth = 200.0f;
		buttonStyle2.fixedHeight = 20.0f;
		buttonStyle2.margin = new RectOffset((int)((Screen.width - 200) * .5f), (int)((Screen.width - 200) * .5f), 0, 0);
		buttonStyle.margin = new RectOffset((int)((Screen.width - 150) * .5f), (int)((Screen.width - 150) * .5f), 0, 0);
		GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
		infoStyle.fontSize = 10;
		infoStyle.margin.top = 0;
		infoStyle.margin.bottom = 0;

		if (!Application.isPlaying) {
			GUI.enabled = true;
		} else {
			GUILayout.Label("Editor can't combine in play-mode", infoStyle);
			GUILayout.Label("Use SimpleMeshCombine.CombineMeshes();", infoStyle);
			GUI.enabled = false;
		}
		GUILayout.Space(15.0f);

		if (target_cs.combinedGameOjects == null || target_cs.combinedGameOjects.Length == 0) {
			if (GUILayout.Button("Combine", buttonStyle)) {
				if (target_cs.transform.childCount >= 1) target_cs.CombineMeshes();
			}
		} else {
			if (GUILayout.Button("Release", buttonStyle)) {
				target_cs.EnableRenderers(true);
				if (target_cs.combined != null) DestroyImmediate(target_cs.combined);
				target_cs.combinedGameOjects = null;
				target_cs.vCount = 0;
			}
		}
		GUILayout.Space(5.0f);

		if (target_cs.combined != null) {
			if (!target_cs._canGenerateLightmapUV) {
				GUILayout.Label("Warning: Mesh has too high vertex count", EditorStyles.boldLabel);
				GUI.enabled = false;
			}

			if (target_cs.combined.GetComponent<MeshFilter>().sharedMesh.name != "") {
				GUI.enabled = false;
			} else if (!Application.isPlaying) {
				GUI.enabled = true;
			}

			if (GUILayout.Button("Save Mesh", buttonStyle2)) {

				if (target_cs.autoOverwrite != null) {
					string apath = AssetDatabase.GetAssetPath(target_cs.autoOverwrite);
					if (EditorUtility.DisplayDialog("Replace Asset?",
								   "Are you sure you want to replace mesh asset:\n" + apath
								   , "Yes", "No")) {
						UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(apath, (Type)typeof(object));
						((Mesh)asset).Clear();
						EditorUtility.CopySerialized(target_cs.combined.GetComponent<MeshFilter>().sharedMesh, asset);
						AssetDatabase.SaveAssets();
						Debug.Log("Saved mesh asset: " + apath);
					}
					return;
				}

				string path = SaveFile("Assets/", target_cs.transform.name + " [SMC Asset]", "asset");
				if (path != null) {
					UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					if (asset == null) {
						AssetDatabase.CreateAsset(target_cs.combined.GetComponent<MeshFilter>().sharedMesh, path);
					} else {
						((Mesh)asset).Clear();
						EditorUtility.CopySerialized(target_cs.combined.GetComponent<MeshFilter>().sharedMesh, asset);
						AssetDatabase.SaveAssets();
					}
					target_cs.combined.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					target_cs.autoOverwrite = (Mesh)AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					Debug.Log("Saved mesh asset: " + path);
				}
			}
			GUILayout.BeginHorizontal();
			target_cs.autoOverwrite = (Mesh)EditorGUILayout.ObjectField(target_cs.autoOverwrite, (Type)typeof(Mesh), false);
			if (GUILayout.Button("Clear")) {
				target_cs.autoOverwrite = null;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5.0f);
		}
		if (!Application.isPlaying) {
			GUI.enabled = true;
		}
		if (target_cs.combined != null) {
			if (GUILayout.Button("Export OBJ", buttonStyle2)) {
				if (target_cs.combined != null) {
					ExportMesh(target_cs.combined.GetComponent<MeshFilter>(), "Assets/", target_cs.transform.name + " [SMC Mesh]" + ".obj");
				}
			}
			GUILayout.Space(15.0f);
			string bText = "Create Copy";
			if (target_cs.combined.GetComponent<MeshFilter>().sharedMesh.name == "") {
				bText = bText + " (Saved mesh)";
				GUI.enabled = false;
			} else if (!Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button(bText, buttonStyle2)) {
				GameObject newCopy = new GameObject();
				GameObject newCopy2 = new GameObject();
				newCopy2.transform.parent = newCopy.transform;
				newCopy2.transform.localPosition = target_cs.combined.transform.localPosition;
				newCopy2.transform.localRotation = target_cs.combined.transform.localRotation;
				newCopy.name = target_cs.name + " [SMC Copy]";
				newCopy2.name = "Mesh [SMC]";
				newCopy.transform.position = target_cs.transform.position;
				newCopy.transform.rotation = target_cs.transform.rotation;
				MeshFilter mf = newCopy2.AddComponent<MeshFilter>();
				newCopy2.AddComponent<MeshRenderer>();
				mf.sharedMesh = target_cs.combined.GetComponent<MeshFilter>().sharedMesh;
				target_cs.copyTarget = newCopy;
				CopyMaterials(newCopy2.transform);
				CopyColliders();
				Selection.activeTransform = newCopy.transform;
			}
			GUILayout.Space(5.0f);
			if (target_cs.copyTarget == null) {
				GUI.enabled = false;
			} else if (!Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button("Copy Colliders", buttonStyle2)) {
				CopyColliders();
			}
			GUILayout.Space(5.0f);
			if (GUILayout.Button("Copy Materials", buttonStyle2)) {
				CopyMaterials(target_cs.copyTarget.transform.Find("Mesh [SMC]"));
			}
			GUILayout.Space(15.0f);
			if (!Application.isPlaying) {
				GUI.enabled = true;
			}
			target_cs.destroyOldColliders = EditorGUILayout.Toggle("Destroy old colliders", target_cs.destroyOldColliders);
			target_cs.keepStructure = EditorGUILayout.Toggle("Keep collider structure", target_cs.keepStructure);
			target_cs.copyTarget = (GameObject)EditorGUILayout.ObjectField("Copy to: ", target_cs.copyTarget, typeof(GameObject), true);
		}

		if (target_cs.combined == null) {
			target_cs.generateLightmapUV = EditorGUILayout.Toggle("Create Lightmap UV", target_cs.generateLightmapUV);
			target_cs.lightmapScale = EditorGUILayout.FloatField("Lightmap Scale", target_cs.lightmapScale);
			target_cs.setStatic = EditorGUILayout.Toggle("Static", target_cs.setStatic);

		}



		GUILayout.Space(5.0f);
		EditorGUILayout.BeginVertical("Box");

		if (target_cs.combined != null) {
			GUILayout.Label("Combined vertex count: " + target_cs.vCount + " / 65536" + " (" + UncombinedVertex() + ")", infoStyle);
			GUILayout.Label("Material count: " + target_cs.combined.GetComponent<Renderer>().sharedMaterials.Length, infoStyle);
		} else {
			GUILayout.Label("Combined vertex count: " + target_cs.vCount + " / 65536" + " (" + UncombinedVertex() + ")", infoStyle);

			GUILayout.Label("Material count: -", infoStyle);
		}

		GUI.color = Color.white;
		EditorGUILayout.EndVertical();
		GUIStyle buttonStyle3 = new GUIStyle(GUI.skin.button);
		buttonStyle3.fixedWidth = 11.0f;
		buttonStyle3.fixedHeight = 14.0f;
		buttonStyle3.fontSize = 9;
		buttonStyle3.padding = new RectOffset(-2, 0, 0, 0);
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		if (GUI.changed) {
			EditorUtility.SetDirty(target_cs);
		}
	}

	public int UncombinedVertex(){
		int totalMeshes = 0;
		int verts = 0;
		var target_cs = (SimpleMeshCombine)target;

		for (int i = 0; i < target_cs.transform.childCount; i++) {
			if (target_cs.combined && target_cs.transform.GetChild(i) != target_cs.combined.transform || !target_cs.combined) {
				MeshFilter[] mfs = target_cs.transform.GetChild(i).GetComponentsInChildren<MeshFilter>();
				totalMeshes += mfs.Length;
				for (int j = 0; j < mfs.Length; j++) {
					if (mfs[j].sharedMesh != null) {
						verts += mfs[j].sharedMesh.vertexCount;
					}
				}
			}
			
		}
		return verts;
	}

	public void DestroyComponentsExeptColliders(Transform t){
		var target_cs = (SimpleMeshCombine)target;
        Component[] transforms = t.GetComponentsInChildren(typeof(Transform));
		foreach(Transform trans in transforms){ 
			if(!target_cs.keepStructure && trans.transform.parent != t && trans.transform != t && (trans.GetComponent(typeof(Collider)) != null)){
	        	trans.transform.name = ""+ GetParentStructure(t, trans.transform);
	         	trans.transform.parent = t;      
	        }
		}
		Component[] components = t.GetComponentsInChildren(typeof(Component));
        foreach(Component comp in components){      
	        if( !( comp is Collider) && !( comp is Transform) ){    				
	             DestroyImmediate(comp);  
			}
        }
	}
	
	public string GetParentStructure(Transform root,Transform t){
		Transform ct = t;
		string s = "";
		while(ct !=root ){	
			s = s.Insert(0, ct.name + " - ");	
			ct = ct.parent;
			
		}
		s = s.Remove(s.Length-3, 3);
		return s;
	}
	
	public void DestroyEmptyGameObjects(Transform t){
		Component[] components = t.GetComponentsInChildren(typeof(Transform));
		  foreach(Transform comp in components){
		  	if((comp != null) && (comp.childCount == 0 || !CheckChildrenForColliders(comp))){
		  		Collider col = (Collider)comp.GetComponent(typeof(Collider));
		  		if(col == null){
		  			DestroyImmediate(comp.gameObject);
		  		}
		  	}
		}
	}
	public bool CheckChildrenForColliders(Transform t){		
		Component[] components = t.GetComponentsInChildren(typeof(Collider));
		if(components.Length > 0){
			return true;
		}
		return false;
	}
	
	public void CopyMaterials(Transform t){
		var target_cs = (SimpleMeshCombine)target;
		Renderer r = t.GetComponent<Renderer>();
		r.sharedMaterials = target_cs.combined.transform.GetComponent<Renderer>().sharedMaterials;
	}
	
	public void CopyColliders(){
		var target_cs = (SimpleMeshCombine)target;
        GameObject clone = (GameObject)Instantiate(target_cs.gameObject,  target_cs.copyTarget.transform.position,  target_cs.copyTarget.transform.rotation);
		if(target_cs.destroyOldColliders){
			Transform o = target_cs.copyTarget.transform.Find("Colliders [SMC]");
			if(o != null){
				DestroyImmediate(o.gameObject);
			}
		}				
		clone.transform.name = "Colliders [SMC]";
		clone.transform.parent = target_cs.copyTarget.transform;				    
		DestroyComponentsExeptColliders(clone.transform);
		DestroyEmptyGameObjects(clone.transform);
	}
}