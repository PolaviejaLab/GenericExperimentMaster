using UnityEngine;
using System.Collections;
using System;

/**
* Events handled by the Wave State Machine
*/
public enum WaveEvents
{
    Wave_0 = 0,
    Wave_1 = 1,
    Wave_Initial,
    DelayDone,
    Waved, 
};

/**
* States of the Wave State Machine
*/
public enum WaveStates
{
    Idle,
    Initial,
    Target,
    Feedback,
    Interval,
    End,
};

public enum LightResults
{
    Correct,
    Incorrect,
    TimeOut,
};


public class WaveController : ICStateMachine<WaveStates, WaveEvents>
{
    // Reference to parent classes
    public TrialController trialController;

    // Initial and subsequent lights
    public MaterialChanger initialLight;
    public MaterialChanger[] lights;
    public MaterialChanger feedbackScreen;

    // public int currentWave;

    // Number of current light
    private int currentLight;

    // State of the lights
    public bool initialLightOn;
    public bool targetLightOn;
    public LightResults lightResults;
    
    // Colliders on/off
    public GameObject collisionLights;
    public GameObject collisionInitial;
    public float collisionProbability;
    public float randomProbability;

    // Questionnaire
    // public GameObject Questionnaire; // > this does nothing and now :P
    public float timeInState;

    // Define Time Outs
    public float timeOut = 3.0f;


    public void Start()
    {
        collisionLights = GameObject.Find("CubeLight");
        collisionInitial = GameObject.Find("CubeInitial");
    }


    protected override void OnStart()
    {
        collisionProbability = trialController.collisionProbability;
    }


    public void HandleEvent(WaveEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case WaveStates.Initial:
                if (ev == WaveEvents.DelayDone && initialLightOn)
                {
                    initialLight.activeMaterial = 1;
                    collisionInitial.SetActive(true);
                }
                else if (ev == WaveEvents.Wave_Initial)
                {
                    WriteLog("Initial Target Waved");
                    collisionInitial.SetActive(false);

                    ChangeState(WaveStates.Target);
                }
                break;

            case WaveStates.Target:
                if (ev == WaveEvents.DelayDone && targetLightOn)
                {
                    // Turn on random target light
                    currentLight = UnityEngine.Random.Range(0, lights.Length);

                    WriteLog("Light: " + currentLight);

                    lights[currentLight].activeMaterial = 1;
                    collisionLights.SetActive(true);
                    
                }
                else if ((int)ev == currentLight && randomProbability <= collisionProbability)
                {
                    WriteLog("Probability for Wave " + trialController.currentWave + ": " + randomProbability);
                    WriteLog("Waved correctly");
                    trialController.correctWaves++;

                    lightResults = LightResults.Correct;
                    ChangeState(WaveStates.Feedback);
                }
                else if ((int)ev == currentLight && randomProbability > collisionProbability) {
                    WriteLog("Probability for Wave " + trialController.currentWave + ": " + randomProbability);

                    WriteLog("Not waved");
                }
                else if ((int)ev != currentLight && ev != WaveEvents.Wave_Initial) {
                    WriteLog("Waved incorrectly");
                    trialController.incorrectWaves++;

                    lightResults = LightResults.Incorrect;
                    ChangeState(WaveStates.Feedback);
                }
                break;

            case WaveStates.Interval:
                break;

            case WaveStates.End:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case WaveStates.Initial:
                if (GetTimeInState() > 0.5f && !initialLightOn) {
                    initialLightOn = true;
                    HandleEvent(WaveEvents.DelayDone);
                }
                break;

            case WaveStates.Target:
                // Wait between the lights turning on and off
                if (GetTimeInState() > 0.5f && !targetLightOn) {
                    targetLightOn = true;
                    HandleEvent(WaveEvents.DelayDone);
                }
                if (GetTimeInState() > timeOut && targetLightOn)
                {
                    WriteLog("Waved Late");
                    trialController.lateWaves++;

                    lightResults = LightResults.TimeOut;
                    ChangeState(WaveStates.Feedback);
                }
                break;

            case WaveStates.Feedback:
                if (GetTimeInState() > 0.5f)
                    GiveFeedback();
                if (GetTimeInState() > 3.0f)
                    ChangeState(WaveStates.End);
                break;

            case WaveStates.Interval:
                if (GetTimeInState() > 0.5f)
                    ChangeState(WaveStates.End);

              break;

            case WaveStates.End:
                break;
        }
    }


    protected override void OnEnter(WaveStates oldState)
    {
        switch (GetState())
        {
            case WaveStates.Initial:
                break;

            case WaveStates.Target:
                trialController.currentWave++;
                break;

            case WaveStates.Interval:
                feedbackScreen.activeMaterial = 0;
                break;

            case WaveStates.Feedback:
                break;


            case WaveStates.End:
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(WaveStates newState)
    {
        switch (GetState())
        {
            case WaveStates.Initial:
                TurnOffInitial();
                break;

            case WaveStates.Target:
                TurnOffTarget();
                timeInState = GetTimeInState();
                WriteLog("Time in wave: " + trialController.currentWave + " was " + timeInState);
                break;

            case WaveStates.Interval:
                break;

            case WaveStates.End:
                break;
        }
    }

    public void TurnOnInitial() {
        initialLight.activeMaterial = 1;
        collisionInitial.SetActive(true);
        initialLightOn = true;
    }
    
    public void TurnOffInitial() {
        initialLight.activeMaterial = 0;
        collisionInitial.SetActive(false);
        initialLightOn = false;
    }

    public void TurnOnTarget() {
        collisionLights.SetActive(true);
        lights[currentLight].activeMaterial = 1;
        targetLightOn = true;
    }

    public void TurnOffTarget() {
        collisionLights.SetActive(false);
        lights[currentLight].activeMaterial = 0;
        targetLightOn = false;
    }

    public void GiveFeedback() {
        if (lightResults == LightResults.Correct)
            feedbackScreen.activeMaterial = 1;
        if (lightResults == LightResults.Incorrect)
            feedbackScreen.activeMaterial = 2;
        if (lightResults == LightResults.TimeOut)
            feedbackScreen.activeMaterial = 2;
    }
}
