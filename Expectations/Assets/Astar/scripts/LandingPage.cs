using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LandingPage : MonoBehaviour 
{
    public Button btnScene1;
    public Button btnScene2;
    public Button btnScene3;
    public Button btnScene4;

	// Use this for initialization
	void Start () 
    {
       // GameObject.DontDestroyOnLoad(this.gameObject);

        btnScene1.OnButtonUp += delegate(Button sender)
        {
               SceneManager.LoadScene("astar_random_maze");
        };

        btnScene2.OnButtonUp += delegate(Button sender)
        {
                SceneManager.LoadScene("astar_simple");
        };

        btnScene3.OnButtonUp += delegate(Button sender)
        {
                SceneManager.LoadScene("astar_two_levels");
        };

        // fullscreen needs button down!
        btnScene4.OnButtonDown += delegate(Button sender)
        {
                Screen.fullScreen = Screen.fullScreen ^true;
        };
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
