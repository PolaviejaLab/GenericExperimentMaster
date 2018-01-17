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
    ExperimentWave,             // Waving
    TaskPaused,                 // 
    Threat,                     // Threat
    End,                        // End of the trial
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

    // Parameters of the current trial threat
    public bool knifePresent;
    public bool randomizeThreatWave;
    public int threatWave;
    public Vector3 knifeOffset;

    // wave recording variables
    public int currentWave;
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
        currentWave = 0;
        lateWaves = 0;
        correctWaves = 0;
        incorrectWaves = 0;

        // Set trial parameters
        threatController.threatOffset = knifeOffset;
        threatController.handOffset = new Vector3(0, 0, trialController.offset); // this does not look right 

        FindThreatWaveNumber();
        knifePresent = trialController.knifePresent;
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
                waveController.Stopped += (obj, ev) => HandleEvent(OwnershipTrialEvents.TaskFinished);
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
                totWaves = waveController.currentWave;
                // correctWaves = waveController.correctWaves;
                // incorrectWaves = waveController.incorrectWaves;
                // lateWaves = waveController.lateWaves;
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