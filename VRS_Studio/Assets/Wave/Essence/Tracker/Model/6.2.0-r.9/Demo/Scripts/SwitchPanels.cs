// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC\u2019s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections.Generic;
using UnityEngine;

namespace Wave.Essence.Tracker.Model.Demo
{
	public class SwitchPanels : MonoBehaviour
	{
		enum ShowPanelOption
		{
			ShowAll = 0,
			ShowPanels1 = 1,
			ShowPanels2 = 2,
			HideAll = 3,
		}

		static readonly ShowPanelOption[] s_ShowPanelOptions = new ShowPanelOption[]
		{
			ShowPanelOption.ShowAll,
			ShowPanelOption.ShowPanels1,
			ShowPanelOption.ShowPanels2,
			ShowPanelOption.HideAll,
		};

		public List<GameObject> Panels1 = new List<GameObject>();
		public List<GameObject> Panels2 = new List<GameObject>();

		int optionIndex = (s_ShowPanelOptions.Length - 1);

		public void ShowPanels()
		{
			optionIndex++;
			optionIndex %= s_ShowPanelOptions.Length;
			ShowPanelOption option = s_ShowPanelOptions[optionIndex];

			switch(option)
			{
				case ShowPanelOption.ShowAll:
					{
						for (int i = 0; i < Panels1.Count; i++)
							Panels1[i].SetActive(true);
						for (int i = 0; i < Panels2.Count; i++)
							Panels2[i].SetActive(true);
					}
					break;
				case ShowPanelOption.ShowPanels1:
					{
						for (int i = 0; i < Panels1.Count; i++)
							Panels1[i].SetActive(true);
						for (int i = 0; i < Panels2.Count; i++)
							Panels2[i].SetActive(false);
					}
					break;
				case ShowPanelOption.ShowPanels2:
					{
						for (int i = 0; i < Panels1.Count; i++)
							Panels1[i].SetActive(false);
						for (int i = 0; i < Panels2.Count; i++)
							Panels2[i].SetActive(true);
					}
					break;
				case ShowPanelOption.HideAll:
					{
						for (int i = 0; i < Panels1.Count; i++)
							Panels1[i].SetActive(false);
						for (int i = 0; i < Panels2.Count; i++)
							Panels2[i].SetActive(false);
					}
					break;
				default:
					break;
			}
		}
	}
}
