using UnityEngine;

namespace Wave.Essence.Spectator
{
	/// <summary>
	/// Name: ISpectatorCameraSetting.cs
	/// Role: Contract
	/// Responsibility: Define the setting attribute of the spectator camera.
	/// </summary>
	public interface ISpectatorCameraSetting
	{
		#region Property

		LayerMask LayerMask { get; set; }
		bool IsSmoothCameraMovement { get; set; }
		int SmoothCameraMovementSpeed { get; set; }
		bool IsFrustumShowed { get; set; }
		float VerticalFov { get; set; }
		SpectatorCameraHelper.SpectatorCameraPanoramaResolution PanoramaResolution { get; set; }
		TextureProcessHelper.PictureOutputFormat PanoramaOutputFormat { get; set; }
		TextureProcessHelper.PanoramaType PanoramaOutputType { get; set; }
		SpectatorCameraHelper.FrustumLineCount FrustumLineCount { get; set; }
		SpectatorCameraHelper.FrustumCenterLineCount FrustumCenterLineCount { get; set; }
		float FrustumLineWidth { get; set; }
		float FrustumCenterLineWidth { get; set; }
		Color FrustumLineColor { get; set; }
		Color FrustumCenterLineColor { get; set; }

		#endregion

		#region Function

		void ResetSetting();
		void ExportSetting2JsonFile(in SpectatorCameraHelper.AttributeFileLocation attributeFileLocation);
		void LoadSettingFromJsonFile(in string jsonFilePath);
		void LoadSettingFromJsonFile(
			in string sceneName,
			in string gameObjectName,
			in SpectatorCameraHelper.AttributeFileLocation attributeFileLocation);

		void ApplyData(in SpectatorCameraHelper.SpectatorCameraAttribute data);

		#endregion
	}
}
