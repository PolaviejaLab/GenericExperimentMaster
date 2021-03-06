﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
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
    Delay,
    Target,
    Feedback,
    Interval,
    End,
};

public enum LightResults
{
    Correct,
    Incorrect,
    Late,
};


public class WaveController : ICStateMachine<WaveStates, WaveEvents>
{
    // Reference to parent classes
    public TrialController trialController;

    // boolean
    public bool taskStarted = false;

    // Initial and subsequent lights
    public MaterialChanger initialLight;
    public MaterialChanger[] lights;
    public MaterialChanger feedbackScreen;

    // Number of current light
    private int currentLight;

    // Wave counter
    public int currentWave;

    // State of the lights
    public bool initialLightOn;
    public bool targetLightOn;
    public LightResults lightResults;
    
    // Colliders on/off
    public GameObject collisionLights;
    public GameObject collisionInitial;
    public float collisionProbability;
    public float randomProbability;
    public bool targetColliderOn;

    private bool feedbackOn;

    // Define Time Outs
    public float waveTimeOut = 3.0f;
    public float collisionDelay;
    public float timeInState;

    public GameObject counters; 
    public Text incorrectCounter;
    public Text correctCounter;

    public void Start() {
        collisionLights = GameObject.Find("CubeLight");
        collisionInitial = GameObject.Find("CubeInitial");
    }

    protected override void OnStart()
    {
        collisionProbability = trialController.collisionProbability;
        collisionDelay = trialController.delayWave;
        
        currentWave = 0;
        trialController.incorrectWaves = 0;
        trialController.correctWaves = 0;
        trialController.lateWaves = 0;

        taskStarted = true;
        counters.SetActive(true);
        correctCounter.text = trialController.correctWaves.ToString();
        incorrectCounter.text = trialController.incorrectWaves.ToString();
    }


    public void HandleEvent(WaveEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case WaveStates.Initial:
                if (ev == WaveEvents.Wave_Initial)
                {
                    WriteLog("Initial Target Waved");
                    ChangeState(WaveStates.Delay);
                }
                break;

            case WaveStates.Target:
                if ((int)ev == currentLight && randomProbability <= collisionProbability)
                {
                    WriteLog("Probability for Wave " + currentWave + ": " + randomProbability);
                    trialController.correctWaves++;

                    lightResults = LightResults.Correct;
                    ChangeState(WaveStates.Feedback);
                }
                else if ((int)ev == currentLight && randomProbability > collisionProbability) {
                    WriteLog("Probability for Wave " + currentWave + ": " + randomProbability);
                }
                else if ((int)ev != currentLight) {
                    lightResults = LightResults.Incorrect;
                    trialController.incorrectWaves++;
                    ChangeState(WaveStates.Feedback);
                }
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
            case WaveStates.Idle:
                if (GetTimeInState() > 0.25f) {
                    ChangeState(WaveStates.Initial);
                }
                break;

            case WaveStates.Initial:
                if (GetTimeInState() > 0.25f && !initialLightOn) {
                    TurnOnInitial();
                }
                break;

            case WaveStates.Delay:
                if (GetTimeInState() > 0.5f && !targetLightOn)
                {
                    ChangeState(WaveStates.Target);
                }
                break;

            case WaveStates.Target:
                // Wait between the lights turning on and off
                if (GetTimeInState() > collisionDelay && targetLightOn && !targetColliderOn)
                {
                    collisionLights.SetActive(true);
                    targetColliderOn = true;
                }
                if (GetTimeInState() > waveTimeOut && targetLightOn)
                {
                    trialController.lateWaves++;

                    lightResults = LightResults.Late;
                    ChangeState(WaveStates.Feedback);
                }
                break;

            case WaveStates.Feedback:
                if (GetTimeInState() > 0.25f && !feedbackOn)
                    GiveFeedback();
                if (GetTimeInState() > 1.5f && feedbackOn)
                    ChangeState(WaveStates.Interval);
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

            case WaveStates.Delay:
                break;

            case WaveStates.Target:
                // Turn on random target light
                currentLight = UnityEngine.Random.Range(0, lights.Length);
                WriteLog("Light: " + currentLight);
                TurnOnTarget();
                break;

            case WaveStates.Feedback:
                break;

            case WaveStates.Interval:
                if (currentWave < (trialController.wavesRequired))
                    ChangeState(WaveStates.Initial);
                if (currentWave == (trialController.wavesRequired))
                    ChangeState(WaveStates.End);
                break;

            case WaveStates.End:
                counters.SetActive(false);
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
                timeInState = GetTimeInState();
                TurnOffTarget();
                WriteLog("Time in wave: " + currentWave + " was " + timeInState);
                break;

            case WaveStates.Feedback:
                feedbackScreen.activeMaterial = 0;
                feedbackOn = false;
                currentWave++;
                correctCounter.text = trialController.correctWaves.ToString();
                incorrectCounter.text = (trialController.incorrectWaves + trialController.lateWaves).ToString();               
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
        targetColliderOn = false;
    }

    public void GiveFeedback() {
        feedbackOn = true;
        if (lightResults == LightResults.Correct)
        {
            WriteLog("Waved correctly");
            feedbackScreen.activeMaterial = 1;
        }
        if (lightResults == LightResults.Incorrect)
        {
            WriteLog("Waved incorrectly");
            feedbackScreen.activeMaterial = 2;
        }
        if (lightResults == LightResults.Late)
        {
            WriteLog("Waved Late");
            feedbackScreen.activeMaterial = 2;

        }

    }
}