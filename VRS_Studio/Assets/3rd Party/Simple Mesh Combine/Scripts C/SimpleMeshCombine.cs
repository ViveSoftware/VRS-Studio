/****************************************
	Simple Mesh Combine
	Copyright Unluck Software	
 	www.chemicalbliss.com 																																													
*****************************************/
//Add script to the parent gameObject, then click combine
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
[AddComponentMenu("Simple Mesh Combine")]

public class SimpleMeshCombine : MonoBehaviour {
	public GameObject[] combinedGameOjects;     //Stores gameObjects that has been merged, mesh renderer disabled
	public GameObject combined;                 //Stores the combined mesh gameObject
	public string meshName = "Combined_Meshes"; //Asset name when saving as prefab
	public bool _canGenerateLightmapUV;
	public int vCount;
	public bool generateLightmapUV;
	public float lightmapScale = 1f;

	public GameObject copyTarget;
	public bool destroyOldColliders;
	public bool keepStructure = true;

	public Mesh autoOverwrite;

	public bool setStatic = true;

	public void EnableRenderers(bool e) {
		for (int i = 0; i < combinedGameOjects.Length; i++) {
			if (combinedGameOjects[i] == null) break;
			Renderer renderer = combinedGameOjects[i].GetComponent<Renderer>();
			if (renderer != null) renderer.enabled = e;
		}
	}
	//Returns a meshFilter[] list of all renderer enabled meshfilters(so that it does not merge disabled meshes, useful when there are invisible box colliders)
	public MeshFilter[] FindEnabledMeshes() {
		MeshFilter[] renderers = null;
		int count = 0;
		renderers = transform.GetComponentsInChildren<MeshFilter>();
		//count all the enabled meshrenderers in children		
		for (int i = 0; i < renderers.Length; i++) {
			if ((renderers[i].GetComponent<MeshRenderer>() != null) && renderers[i].GetComponent<MeshRenderer>().enabled)
				count++;
		}
		MeshFilter[] meshfilters = new MeshFilter[count];//creates a new array with the correct length
		count = 0;
		//adds all enabled meshes to the array
		for (int ii = 0; ii < renderers.Length; ii++) {
			if ((renderers[ii].GetComponent<MeshRenderer>() != null) && renderers[ii].GetComponent<MeshRenderer>().enabled) {
				meshfilters[count] = renderers[ii];
				count++;
			}
		}
		return meshfilters;
	}


	public void CombineMeshes() {
		GameObject combo = new GameObject();
		combo.name = "_Combined Mesh [" + name + "]";
		combo.gameObject.AddComponent<MeshFilter>();
		combo.gameObject.AddComponent<MeshRenderer>();
		MeshFilter[] meshFilters = null;
		meshFilters = FindEnabledMeshes();
		ArrayList materials = new ArrayList();
		ArrayList combineInstanceArrays = new ArrayList();
		combinedGameOjects = new GameObject[meshFilters.Length];
		for (int i = 0; i < meshFilters.Length; i++) {
			combinedGameOjects[i] = meshFilters[i].gameObject;
			MeshRenderer meshRenderer = meshFilters[i].GetComponent<MeshRenderer>();
			meshFilters[i].transform.gameObject.GetComponent<Renderer>().enabled = false;
			if (meshFilters[i].sharedMesh == null) {
#if UNITY_EDITOR
				Debug.LogWarning("SimpleMeshCombine : " + meshFilters[i].gameObject + " [Mesh Filter] has no [Mesh], mesh will not be included in combine..");
#endif
				break;
			}
			for (int o = 0; o < meshFilters[i].sharedMesh.subMeshCount; o++) {
				if (meshRenderer == null) {
#if UNITY_EDITOR
					Debug.LogWarning("SimpleMeshCombine : " + meshFilters[i].gameObject + "has a [Mesh Filter] but no [Mesh Renderer], mesh will not be included in combine.");
#endif
					break;
				}
				if (o < meshRenderer.sharedMaterials.Length && o < meshFilters[i].sharedMesh.subMeshCount) {
					int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[o]);
					if (materialArrayIndex == -1) {
						materials.Add(meshRenderer.sharedMaterials[o]);
						materialArrayIndex = materials.Count - 1;
					}
					combineInstanceArrays.Add(new ArrayList());
					CombineInstance combineInstance = new CombineInstance();
					combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
					//Fix for Unity 2017 Bug
				#if UNITY_2017
					if (meshFilters[i].sharedMesh.subMeshCount > 1) combineInstance.mesh = SubMeshFix.GetSubmesh(meshFilters[i].sharedMesh,o);
					else {
						combineInstance.mesh = meshFilters[i].sharedMesh;
					}
				#else
					combineInstance.mesh = meshFilters[i].sharedMesh;
					combineInstance.subMeshIndex = o;
				#endif
					(combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
				}


#if UNITY_EDITOR
				else {
					Debug.LogWarning("Simple Mesh Combine: GameObject [ " + meshRenderer.gameObject.name + " ] is missing a material (Mesh or sub-mesh ignored from combine)");
				}
#endif
			}
#if UNITY_EDITOR
			EditorUtility.DisplayProgressBar("Combining", "", (float)i);
#endif
		}

		Mesh[] meshes = new Mesh[materials.Count];
		CombineInstance[] combineInstances = new CombineInstance[materials.Count];
		for (int m = 0; m < materials.Count; m++) {
			CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
			meshes[m] = new Mesh();
			meshes[m].CombineMeshes(combineInstanceArray, true, true);
			combineInstances[m] = new CombineInstance();
			combineInstances[m].mesh = meshes[m];
			combineInstances[m].subMeshIndex = 0;
		}
		Mesh ms = combo.GetComponent<MeshFilter>().sharedMesh = new Mesh();
		ms.Clear();
		ms.CombineMeshes(combineInstances, false, false);
		combo.GetComponent<MeshFilter>().sharedMesh = ms;
		foreach (Mesh mesh in meshes) {
			mesh.Clear();
			DestroyImmediate(mesh);
		}
		MeshRenderer meshRendererCombine = combo.GetComponent<MeshFilter>().GetComponent<MeshRenderer>();
		if (meshRendererCombine == null) meshRendererCombine = gameObject.AddComponent<MeshRenderer>();
		Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
		meshRendererCombine.materials = materialsArray;
		combined = combo.gameObject;
		EnableRenderers(false);
		combo.transform.parent = transform;
#if UNITY_EDITOR
		if (generateLightmapUV) {
			Unwrapping.GenerateSecondaryUVSet(combo.GetComponent<MeshFilter>().sharedMesh);
			SerializedObject so = new SerializedObject (combo.GetComponent<MeshRenderer>());
			so.FindProperty("m_ScaleInLightmap").floatValue = lightmapScale;
			so.ApplyModifiedProperties();
		}
#endif
		combo.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
		vCount = combo.GetComponent<MeshFilter>().sharedMesh.vertexCount;
		if (vCount > 65536) {
			Debug.LogWarning("Vertex Count: " + vCount + "- Vertex Count too high, please divide mesh combine into more groups. Max 65536 for each mesh");
			_canGenerateLightmapUV = false;
		} else {
			_canGenerateLightmapUV = true;
		}
		if (setStatic) combined.isStatic = true;

#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
#endif
	}

	public int Contains(ArrayList l, Material n) {
		for (int i = 0; i < l.Count; i++) {
			if ((l[i] as Material) == n) {
				return i;
			}
		}
		return -1;
	}
}