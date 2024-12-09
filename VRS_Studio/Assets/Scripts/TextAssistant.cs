using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextAssistant : MonoBehaviour
{
    public GameObject textRoot;
    public Text text;

    private static TextAssistant instance;
    public static TextAssistant Instance { get { return instance; } private set { instance = value; } }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        var scene = SceneManager.GetActiveScene();

        if (scene.name == "v130_demo") textRoot.SetActive(false);
        //else if (scene.name == "VRSS_Environment") textRoot.SetActive(true);
    }

    public void SetText(string strText)
    {
        text.text = strText;
    }

    public void SetActive(bool active)
    {
        textRoot.SetActive(active);
    }
}
