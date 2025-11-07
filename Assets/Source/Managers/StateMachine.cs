using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public static StateMachine Instance { get; private set; }
    private List<State> _states;
    private State _currentState;


    // Here we will need methods about 
    // - ChangeState (with a parameter of type State)
    // - GetCurrentState


    void Awake()
    {
        {
        if (Instance == null)
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
