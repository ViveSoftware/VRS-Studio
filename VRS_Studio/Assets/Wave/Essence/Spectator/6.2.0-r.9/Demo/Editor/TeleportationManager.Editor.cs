#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Spectator.Demo
{
	/// <summary>
	/// Name: TeleportationManager.Editor.cs
	/// Role: General script only use in Unity Editor
	/// Responsibility: Display the TeleportationManager.cs in Unity Inspector
	/// </summary>
	public partial class TeleportationManager
	{
		[CustomEditor(typeof(TeleportationManager))]
		public class TeleportationManagerEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				// Just return if not "TeleportationManager" class
				if (!(target is TeleportationManager))
				{
					return;
				}

				EditorHelper.ShowDefaultInspector(serializedObject);
			}
		}
	}
}
#endif
