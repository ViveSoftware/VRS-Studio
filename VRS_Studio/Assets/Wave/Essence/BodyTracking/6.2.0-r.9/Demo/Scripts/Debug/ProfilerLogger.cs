// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.IO;
using System.Text;
using UnityEngine;

namespace Wave.Essence.BodyTracking.Demo
{
	public class ProfilerLogger : MonoBehaviour
	{
		const string LOG_TAG = "Wave.Essence.BodyTracking.Demo.ProfilerLogger ";
		private StringBuilder m_sb = null;
		internal StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder sb)
		{
			sb.Insert(0, LOG_TAG);
			Debug.Log(sb);
		}

		private string logFilePath;

		private void Awake()
		{
			logFilePath = Path.Combine(Application.persistentDataPath, "profiler_log");
			sb.Clear().Append("Profiler log file path: ").Append(logFilePath); DEBUG(sb);

			UnityEngine.Profiling.Profiler.logFile = logFilePath;
			UnityEngine.Profiling.Profiler.enableBinaryLog = true;
			UnityEngine.Profiling.Profiler.enabled = true;
		}

		public void ExitGame()
		{
			UnityEngine.Profiling.Profiler.enableBinaryLog = false;
			UnityEngine.Profiling.Profiler.enabled = false;

			/*string rawFilePath = logFilePath + ".raw";
			string destinationPath = "/sdcard/profiler_log.raw";
			File.Move(rawFilePath, destinationPath);
			sb.Clear().Append("Profiler log file saved to: ").Append(destinationPath); DEBUG(sb);*/

			Application.Quit();
		}
	}
}
