#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Spectator.Demo
{
	/// <summary>
	/// Name: UIManager.Editor.cs
	/// Role: General script only use in Unity Editor
	/// Responsibility: Display the UIManager.cs in Unity Inspector
	/// </summary>
	public partial class UIManager
	{
		[CustomEditor(typeof(UIManager))]
		public class UIManagerEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				// just return if not "SpectatorCameraController" class
				if (!(target is UIManager))
				{
					return;
				}

				EditorHelper.ShowDefaultInspector(serializedObject);
			}
		}
	}
}

#endif
