using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        if (!tabButtons.Contains(button)) tabButtons.Add(button);

        List<TabButton> sortedTabButtons = tabButtons.OrderBy(tabButtonGO => tabButtonGO.name).ToList();
        tabButtons = sortedTabButtons;
    }
    
    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;

        foreach(TabButton currentTabButton in tabButtons)
        {
            if (currentTabButton != button)
            {
                currentTabButton.RespectiveTab.SetActive(false);
            }
            else
            {
                currentTabButton.RespectiveTab.SetActive(true);
            }
        }
    }

    public void ResetTabs()
    {
        foreach(TabButton button in tabButtons)
        {
            if(selectedTab != null && button == selectedTab)
            {
                continue;
            }
            button.background.sprite = tabIdle;
        }
    }
}
