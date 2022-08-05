using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRSStudio.Build
{
    [Serializable]
    public struct SceneData
    {
        public SceneData(string inName, string inPath)
		{
            name = inName;
            path = inPath;
		}

        public string name;
        public string path;
    }

    [Serializable]
    public class VRSStudioScenes : ScriptableObject
    {
        private static VRSStudioScenes instance = null;
        public static VRSStudioScenes Instance
		{
            get
			{
#if UNITY_EDITOR
                if (instance == null)
                {
                    instance = new VRSStudioScenes();
                    instance.sceneDataList = GetSceneDataList();
                    instance.pathList = GetScenePathList();
                }
#endif
                return instance;
            }
		}

        public List<string> pathList = new List<string>();
        public List<SceneData> sceneDataList = new List<SceneData>();

        public static List<SceneData> GetSceneDataList()
        {
            return new List<SceneData>()
            {
                new SceneData("SceneManager", "Assets/Scenes/SceneManager.unity"),

#if !VRSSTUDIO_INTERNAL
                new SceneData("BaseScene", "Assets/Scenes/BaseScene.unity"),
                new SceneData("RobotAssistant", "Assets/Scenes/RobotAssistant.unity"),
#endif
                new SceneData("Entrance", "Assets/Scenes/Entrance.unity"),
                new SceneData("NavMenu", "Assets/Scenes/NavMenu.unity"),

                new SceneData("TrackingBoundaryGuide", "Assets/Scenes/TrackingBoundaryGuide.unity"),
                new SceneData("ScissorsPaperRock", "Assets/Scenes/ScissorsPaperRock.unity"),
                new SceneData("Typing", "Assets/Scenes/Keyboard.unity"),
                new SceneData("3DObject", "Assets/Scenes/3DObjectManipulation.unity"),
                new SceneData("2DPanel", "Assets/Scenes/2DPanel.unity")
            };
        }

        private static List<string> GetScenePathList()
        {
            List<string> scenePaths = new List<string>();

            foreach (SceneData sceneData in instance.sceneDataList)
			{
                scenePaths.Add(sceneData.path);
            }

            return scenePaths;
        }

        private void Awake()
        {
            Debug.Log("VRSStudioScenes Awake");
            instance = this;
        }
    }
}