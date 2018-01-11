using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum OwnershipTrialEvents
{
    TaskFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum OwnershipTrialStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // Reaching-like task
    Threat,                     // Threat
    End,                         // End of the trial
};



public class ImplicitOwnershipTrial : ICStateMachine<OwnershipTrialStates, OwnershipTrialEvents> {

    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public Threat threatController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;

    // Parameters of the current trial
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
        threatController.threatOffset = knifeOffset;
        threatController.handOffset = new Vector3(0, 0, trialController.offset);
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
                if (ev == OwnershipTrialEvents.TaskFinished)
                    ChangeState(OwnershipTrialStates.End);
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

            case OwnershipTrialStates.End:
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

            case OwnershipTrialStates.End:
                trialController.HandleEvent(TrialEvents.TaskFinished);
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

            case OwnershipTrialStates.End:
                break;
        }
    }
}
