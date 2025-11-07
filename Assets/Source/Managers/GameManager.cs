using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    private List<Pipes> _pipes = new();

    // Here we will need some methods about
    // - StartGame
    // - PauseGame
    // - LoseGame
    // - RestartGame
    // - QuitGame
    // - StopTime
    // - ResumeTime
    // - StyleMoment 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
