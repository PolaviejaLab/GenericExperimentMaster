using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum OutcomeOwnershipEvents
{
    WaveFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum OutcomeOwnershipStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // One event of reaching-like task
    Interval,                   // In between measures and task
    Threat,                     // Knife
    End,                        // End of the trial
};


public class OutcomeOwnership : ICStateMachine<OutcomeOwnershipStates, OutcomeOwnershipEvents>
{

    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public Threat threatController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public Noise noiseType;

    private bool threatDone;

    // Use this for initialization
    protected override void OnStart() {
        switch (trialController.noiseType)
        {
            case 0:
                noiseType = Noise.Control;
                trialController.delayWave = 0.0f;
                break;

            case 1:
                noiseType = Noise.ImpairedMovement;
                trialController.delayWave = 0.0f;
                break;

            case 2:
                noiseType = Noise.ImpairedOutcome;
                trialController.delayWave = 0.5f;
                break;

            case 3:
                noiseType = Noise.BothImpaired;
                trialController.delayWave = 0.5f;
                break;
        }
        WriteLog("Noise type: " + noiseType);
        WriteLog("Delay collision active: " + trialController.delayWave);

    }
	
	// Update is called once per frame
	void Update () {
	        if (!IsStarted())
            return;

        switch (GetState())
        {
            case OutcomeOwnershipStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(OutcomeOwnershipStates.ExperimentWave);
                break;


            case OutcomeOwnershipStates.Interval:
                if (GetTimeInState() > 0.5f)
                    ChangeState(OutcomeOwnershipStates.Threat);

                break;

            case OutcomeOwnershipStates.ExperimentWave:
                if (GetTimeInState() > 0.5f)
                    waveController.StartMachine();

                if (waveController.taskStarted)
                {
                    waveController.Stopped += (sender, e) => { HandleEvent(OutcomeOwnershipEvents.WaveFinished); };
                    waveController.taskStarted = false;
                }

                break;
                 
                case OutcomeOwnershipStates.Threat:
                if (!threatDone) {
                    threatController.Stopped += (sender, e) => { HandleEvent(OutcomeOwnershipEvents.ThreatDone); };
                    threatDone = true;
                }
                
                break;

            case OutcomeOwnershipStates.End:
                break;
        }
	}


    public void HandleEvent(OutcomeOwnershipEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case OutcomeOwnershipStates.Idle:
                break;

            case OutcomeOwnershipStates.ExperimentWave:
                if (ev == OutcomeOwnershipEvents.WaveFinished)
                    ChangeState(OutcomeOwnershipStates.Interval);
                break;

            case OutcomeOwnershipStates.Threat:
                if (ev == OutcomeOwnershipEvents.ThreatDone)
                    ChangeState(OutcomeOwnershipStates.End);
                break;

            case OutcomeOwnershipStates.End:
                break;
        }
    }

    protected override void OnEnter(OutcomeOwnershipStates oldState)
    {

        switch (GetState())
        {
            case OutcomeOwnershipStates.Idle:
                handSwitcher.showRightHand = true;
                break;

            case OutcomeOwnershipStates.ExperimentWave:
                break;

            case OutcomeOwnershipStates.Threat:
                threatController.StartMachine();
                threatController.HandleEvent(ThreatEvent.ReleaseThreat);
                break;

            case OutcomeOwnershipStates.End:
                trialController.HandleEvent(TrialEvents.SpTrialFinished);
                this.StopMachine();
                break;
        }
    }

    protected override void OnExit(OutcomeOwnershipStates oldState)
    {
        switch (GetState())
        {
            case OutcomeOwnershipStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case OutcomeOwnershipStates.ExperimentWave:
                waveController.StopMachine();
                break;

            case OutcomeOwnershipStates.Threat:
                threatController.threat.SetActive(false);
                break;

            case OutcomeOwnershipStates.End:
                break;
        }
    }
}


