using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum OwnershipTrialEvents
{
    WaveFinished,
    TaskFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum OwnershipTrialStates
{
    Idle,                      
    ExperimentWave,             // Waving
    Interval,
    Threat,                     // Threat
    End,                        // End of the trial
};



public class ImplicitOwnershipTrial : ICStateMachine<OwnershipTrialStates, OwnershipTrialEvents> {

    // Reference to parent classes
    public TrialController trialController;

    // Reference to child classes
    public WaveController waveController;
    public Threat threatController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    // Furniture GameObjects
    public GameObject testLights;
    public GameObject room;
    public GameObject table;

    // Parameters of the current trial threat
    public bool randomizeThreatWave;
    public int threatWave;
    public Vector3 knifeOffset;
    private bool threatDone;
    
    // wave recording variables
    // public int currentWave;
    public int wavesRequired;
    public int totWaves;
    public int correctWaves;
    public int incorrectWaves;
    public int lateWaves;


    public void Start()
    {
    }

    protected override void OnStart()
    {
        // set wave parameters
        trialController.currentWave = 0;
        trialController.correctWaves = 0;
        trialController.lateWaves = 0;
        trialController.incorrectWaves = 0;

        // Set trial parameters
        threatController.threatOffset = knifeOffset;
        threatController.handOffset = new Vector3(0, 0, trialController.offset); // this does not look right 

        if (trialController.knifePresent)
            FindThreatWaveNumber();
        threatDone = false;
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
                break;

            case OwnershipTrialStates.Interval:
                if (ev == OwnershipTrialEvents.TaskFinished)
                    ChangeState(OwnershipTrialStates.End);
                break;

            case OwnershipTrialStates.Threat:
                break;

            case OwnershipTrialStates.End:
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
                if (GetTimeInState() > 1.0f)
                    ChangeState(OwnershipTrialStates.ExperimentWave);
                break;

            case OwnershipTrialStates.ExperimentWave:               
                break;

            case OwnershipTrialStates.Interval:
                if (trialController.currentWave < trialController.wavesRequired)
                    ChangeState(OwnershipTrialStates.ExperimentWave);
                else if (trialController.currentWave == trialController.wavesRequired && randomizeThreatWave)
                    HandleEvent(OwnershipTrialEvents.TaskFinished);
                else if (trialController.currentWave == threatWave && !threatDone)
                    ChangeState(OwnershipTrialStates.Threat);
                else if (trialController.currentWave == threatWave && threatDone && randomizeThreatWave)
                    ChangeState(OwnershipTrialStates.ExperimentWave);
                else if (trialController.currentWave == threatWave && threatDone && !randomizeThreatWave)
                    ChangeState(OwnershipTrialStates.ExperimentWave);
                break;

            case OwnershipTrialStates.Threat:
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

            case OwnershipTrialStates.Threat:
                threatController.StartMachine();
                threatDone = true;
                break;

            case OwnershipTrialStates.Interval:
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
                break;

            case OwnershipTrialStates.End:
                break;
        }

    }

    private void FindThreatWaveNumber() {
        if (randomizeThreatWave)
        {
            threatWave = UnityEngine.Random.Range(4, wavesRequired - 5);
            WriteLog("Threat wave is: " + threatWave);
        }
        else if (!randomizeThreatWave)
        {
            threatWave = wavesRequired;
            WriteLog("Threat wave is: " + threatWave);
        }
    }
}