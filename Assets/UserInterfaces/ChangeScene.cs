using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
	public void LoadScene(string sceneName)
	{
		Debug.Log("Loading scene: " + sceneName);
		SceneManager.LoadScene(sceneName);
	}
}