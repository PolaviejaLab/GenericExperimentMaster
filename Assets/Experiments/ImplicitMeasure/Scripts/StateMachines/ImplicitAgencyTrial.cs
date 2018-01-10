using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum AgencyTrialEvents
{
    WavingFinished,
    MeasureDone,
};


/**
 * States of the Trial statemachine
 */
public enum AgencyTrialStates
{
    Idle,                      // Get used to the environment
    PreMeasure,                 // Implicit Measure (before task)
    ExperimentWave,             // Reaching-like task
    Delay,                      // In between measures and task
    PostMeasure,                // Implicit measure (after task)
    TrialFinished,              // End of the trial
};

public class ImplicitAgencyTrial : ICStateMachine<AgencyTrialStates, AgencyTrialEvents>
{
    // Reference to the experiment controller
    public ExperimentController experimentController;
    public WaveController waveController;
    public ImplicitMeasure measureController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;
    public GameObject measure;

    // Parameters of the current trial
    public int hand;
    public float offset;
    public float noiseLevel;
    public float lNoise;

    // wave recording variables
    public int totWaves;
    public int correctWaves;
    public int incorrectWaves;
    public int lateWaves;


    public void Start()
    {
    }

    protected override void OnStart()
    {
        // Set trial parameters
        offsetSwitcher.initialOffset = offset;
        handSwitcher.selected = hand;
        handSwitcher.noiseLevelLeft = noiseLevel;
        handSwitcher.noiseLevelRight = noiseLevel;
        handSwitcher.lambdaLeft = lNoise;
        handSwitcher.lambdaRight = lNoise;

        testLights.SetActive(false);
}


    public void HandleEvent(AgencyTrialEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case AgencyTrialStates.Idle:
                break;

            case AgencyTrialStates.PreMeasure:
                if (ev == AgencyTrialEvents.MeasureDone)
                    ChangeState(AgencyTrialStates.Delay);
                break;

            case AgencyTrialStates.ExperimentWave:
                if (ev == AgencyTrialEvents.WavingFinished)
                    ChangeState(AgencyTrialStates.PostMeasure);
                break;

            case AgencyTrialStates.PostMeasure:
                if (ev == AgencyTrialEvents.MeasureDone)
                    ChangeState(AgencyTrialStates.TrialFinished);
                break;

            case AgencyTrialStates.TrialFinished:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case AgencyTrialStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(AgencyTrialStates.PreMeasure);
                break;

            case AgencyTrialStates.PreMeasure:
                if (GetTimeInState() > 1.0f)
                    measureController.StartMachine();
                break;

            case AgencyTrialStates.Delay:
                if (GetTimeInState() > 2.0f)
                    testLights.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Q))
                    ChangeState(AgencyTrialStates.ExperimentWave);
                break;

            case AgencyTrialStates.ExperimentWave:
                break;

            case AgencyTrialStates.PostMeasure:
                if (GetTimeInState() > 1.5f)
                    measureController.StartMachine();
                break;

            case AgencyTrialStates.TrialFinished:
                break;
        }
    }


    protected override void OnEnter(AgencyTrialStates oldState)
    {

        switch (GetState())
        {
            case AgencyTrialStates.Idle:
                handSwitcher.showRightHand = true;
                break;

            case AgencyTrialStates.PreMeasure:
                measure.SetActive(true);
                break;

            case AgencyTrialStates.ExperimentWave:
                waveController.StartMachine();
                break;

            case AgencyTrialStates.PostMeasure:
                measure.SetActive(true);
                break;

            case AgencyTrialStates.TrialFinished:
                experimentController.HandleEvent(ExperimentEvents.TrialFinished);
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(AgencyTrialStates newState)
    {
        switch (GetState())
        {
            case AgencyTrialStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case AgencyTrialStates.PreMeasure:
                measure.SetActive(false);
                break;

            case AgencyTrialStates.ExperimentWave:
                testLights.SetActive(false);
                totWaves = waveController.waveCounter;
                correctWaves = waveController.correctWaves;
                incorrectWaves = waveController.incorrectWaves;
                lateWaves = waveController.lateWaves;
                waveController.StopMachine();
                break;

            case AgencyTrialStates.PostMeasure:
                break;

            case AgencyTrialStates.TrialFinished:
                break;
        }
    }
}
