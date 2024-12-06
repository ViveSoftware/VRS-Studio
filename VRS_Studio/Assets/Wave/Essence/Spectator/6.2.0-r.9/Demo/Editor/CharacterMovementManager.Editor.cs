#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Spectator.Demo
{
	/// <summary>
	/// Name: CharacterMovementManager.Editor.cs
	/// Role: General script only use in Unity Editor
	/// Responsibility: Display the CharacterMovementManager.cs in Unity Inspector
	/// </summary>
	public partial class CharacterMovementManager
	{
		[CustomEditor(typeof(CharacterMovementManager))]
		public class CharacterMovementManagerEditor : UnityEditor.Editor
		{
			public override void OnInspectorGUI()
			{
				if (!(target is CharacterMovementManager))
				{
					// Just return if not "CharacterMovementManager" class
					return;
				}

				EditorHelper.ShowDefaultInspector(serializedObject);
			}
		}
	}
}
#endif
