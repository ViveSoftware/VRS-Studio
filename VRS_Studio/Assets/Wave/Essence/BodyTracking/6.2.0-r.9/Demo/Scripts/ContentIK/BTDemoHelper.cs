// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;

namespace Wave.Essence.BodyTracking.Demo
{
	public static class BTDemoHelper
	{
		public enum TrackingMode : UInt32
		{
			Arm = BodyTrackingMode.ARMIK,
			UpperBody = BodyTrackingMode.UPPERBODYIK,
			FullBody = BodyTrackingMode.FULLBODYIK,
			UpperBodyAndLeg = BodyTrackingMode.UPPERIKANDLEGFK,
		}

		public static string Name(this TrackingMode mode)
		{
			if (mode == TrackingMode.Arm) { return "Arm"; }
			if (mode == TrackingMode.FullBody) { return "FullBody"; }
			if (mode == TrackingMode.UpperBody) { return "UpperBody"; }
			if (mode == TrackingMode.UpperBodyAndLeg) { return "UpperLeg"; }

			return "";
		}
	}
}
