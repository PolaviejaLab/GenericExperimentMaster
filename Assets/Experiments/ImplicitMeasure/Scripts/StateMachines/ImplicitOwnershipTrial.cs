using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum OwnershipTrialEvents
{
    WavingFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum OwnershipTrialStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // Reaching-like task
    Delay,                      // In between measures and task
    Threat,                     // Threat
    TrialFinished,              // End of the trial
};



public class ImplicitOwnershipTrial : ICStateMachine<OwnershipTrialStates, OwnershipTrialEvents> {

    // Reference to the experiment controller
    public ExperimentController experimentController;
    public WaveController waveController;
    public Threat threatController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;

    // Parameters of the current trial
    public int hand;
    public float offset;
    public float noiseLevel;
    public float lNoise;
    public Vector3 knifeOffset;

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
        threatController.threatOffset = knifeOffset;
        threatController.handOffset = new Vector3(0, 0, offset);

        testLights.SetActive(false);

    }


    public void HandleEvent(OwnershipTrialEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case OwnershipTrialStates.Idle:
                break;

            case OwnershipTrialStates.ExperimentWave:
                if (ev == OwnershipTrialEvents.WavingFinished)
                    ChangeState(OwnershipTrialStates.TrialFinished);
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case OwnershipTrialStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(OwnershipTrialStates.ExperimentWave);
                break;

            case OwnershipTrialStates.ExperimentWave:
                break;

            case OwnershipTrialStates.TrialFinished:
                break;
        }
    }


    protected override void OnEnter(OwnershipTrialStates oldState)
    {

        switch (GetState())
        {
            case OwnershipTrialStates.Idle:
                handSwitcher.showRightHand = true;
                break;

            case OwnershipTrialStates.ExperimentWave:
                waveController.StartMachine();
                break;

            case OwnershipTrialStates.TrialFinished:
                experimentController.HandleEvent(ExperimentEvents.TrialFinished);
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(OwnershipTrialStates newState)
    {
        switch (GetState())
        {
            case OwnershipTrialStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case OwnershipTrialStates.ExperimentWave:
                testLights.SetActive(false);
                totWaves = waveController.waveCounter;
                correctWaves = waveController.correctWaves;
                incorrectWaves = waveController.incorrectWaves;
                lateWaves = waveController.lateWaves;
                waveController.StopMachine();
                break;

            case OwnershipTrialStates.TrialFinished:
                break;
        }
    }
}
