using UnityEngine;
using System.Collections;

public enum MeasureEvents
{
    Wave_Started, 
    Wave_Finished,
    TimeoutFinished,
};

public enum MeasureStates
{
    Idle,
    Instructions,
    WaitingToStart,
    Measuring, 
    Delay,
    Finished,
};


public class ImplicitMeasure : ICStateMachine<MeasureStates, MeasureEvents>
{
    public ImplicitAgencyTrial trialController;
    public OffsetSwitcher offsetSwitcher;
    public HandSwitcher handSwitcher;

    public GameObject measureObject;
    
    public MaterialChanger startLight;
    public MaterialChanger finishLight;

    public GameObject startCollision;
    public GameObject finishCollision;

    public bool startLightOn;
    public bool finishLightOn;

    public float updatedDisplacement = 0;
    public int framesDisplaced;

    public int totalIterations = 3;
    public int currentIteration;

    public GameObject instructionCanvas;

    new public void Start()
    {
        framesDisplaced = 0;
    }


    protected override void OnStart()
    {
        Debug.Log("Measure Machine Started");
        currentIteration = 1;
    }


    public void HandleEvent(MeasureEvents ev)
    {
        if (!IsStarted())
            return;

        Debug.Log("Event " + ev.ToString());

        switch (GetState())
        {
            case MeasureStates.Idle:
                break;

            case MeasureStates.WaitingToStart:
                if (ev == MeasureEvents.Wave_Started && startLightOn) {
                    ChangeState(MeasureStates.Measuring);
                }
                break;

            case MeasureStates.Measuring:
                if (ev == MeasureEvents.Wave_Finished && finishLightOn) {
                    if (currentIteration < totalIterations)
                        ChangeState(MeasureStates.Delay);
                    else if (currentIteration == totalIterations)
                        ChangeState(MeasureStates.Finished);
                }
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case MeasureStates.Instructions:
                if (GetTimeInState() > 4.0f)
                    ChangeState(MeasureStates.Idle);
                break;
            case MeasureStates.Idle:
                if (GetTimeInState() > 0.5f)
                {
                    ChangeState(MeasureStates.WaitingToStart);
                    offsetSwitcher.noDisplacement();
                    if (!handSwitcher.showRightHand)
                        handSwitcher.showRightHand = true;
                }
                break;

            case MeasureStates.Measuring:
                offsetSwitcher.displaceHand(updatedDisplacement);
                updatedDisplacement += 0.000025f;
                framesDisplaced += 1;
                if (Input.GetKeyDown(KeyCode.Space))
                    HandleEvent(MeasureEvents.Wave_Finished);
                break;

            case MeasureStates.Delay:
                if (GetTimeInState() > 2.0f) {
                    ChangeState(MeasureStates.Idle);
                }
                break;

            case MeasureStates.Finished:
                if (GetTimeInState() > 0.5f) {
                    trialController.HandleEvent(AgencyTrialEvents.MeasureDone);
                    measureObject.SetActive(false);
                    this.StopMachine();
                }           
                break;
        }
    }


    protected override void OnEnter(MeasureStates oldState)
    {
        switch (GetState())
        {
            case MeasureStates.Instructions:
                instructionCanvas.SetActive(true);
                break;

            case MeasureStates.Idle:
                WriteLog("Implicit Measure Started");
                break;

            case MeasureStates.WaitingToStart:
                TurnOnStartLight();
                TurnOnFinishLight();
                break;

            case MeasureStates.Measuring:
                WriteLog("Iteration measure " + currentIteration + " started");           
                break;

            case MeasureStates.Delay:
                handSwitcher.showRightHand = false;
                currentIteration += 1;
                updatedDisplacement = 0;
                framesDisplaced = 0;
                break;

            case MeasureStates.Finished:
                WriteLog("Implicit Measure Finished"); 
                break;
        }
    }


    protected override void OnExit(MeasureStates newState)
    {
        switch (GetState())
        {
            case MeasureStates.Instructions:
                instructionCanvas.SetActive(false);
                break;
            case MeasureStates.Idle:
                break;

            case MeasureStates.WaitingToStart:
                TurnOffStartLight();
                break;

            case MeasureStates.Measuring:
                TurnOffFinishLight();
                WriteLog("Updated Displacement " + updatedDisplacement);
                WriteLog("Fames Displaced " + framesDisplaced);
                WriteLog("Measurement Iteration " + currentIteration + " finished");
                break;

            case MeasureStates.Finished:
                offsetSwitcher.noDisplacement();
                break;
        }
    }

    public void TurnOnStartLight() {
        startLight.activeMaterial = 1;      
        startLightOn = true;        
    }

    public void TurnOffStartLight() {
        startLight.activeMaterial = 0;
        startLightOn = false;
    }

    public void TurnOnFinishLight() {
        finishLight.activeMaterial = 1;
        finishLightOn = true;
    }

    public void TurnOffFinishLight() {
        finishLight.activeMaterial = 0;
        finishLightOn = false;
    }
}