using UnityEngine;
using System.Collections;


/**
 * States of the Trial statemachine
 */
public enum RepeatThreatStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // One event of reaching-like task
    Interval,                   // In between measures and task
    Threat,                     // Knife
    End,                        // End of the trial
};

public enum RepeatThreatEvents
{
    WaveFinished,
    ThreatDone,
};


public class RepeatThreat : ICStateMachine<RepeatThreatStates, RepeatThreatEvents> {
    
    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public Threat threatController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;


    public void Start()
    {
    }

    protected override void OnStart()
    {
    }


    public void HandleEvent(RepeatThreatEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case RepeatThreatStates.Idle:
                break;

            case RepeatThreatStates.ExperimentWave:
                if (ev == RepeatThreatEvents.WaveFinished)
                    ChangeState(RepeatThreatStates.Interval);
                break;

            case RepeatThreatStates.Threat:
                if (ev == RepeatThreatEvents.ThreatDone)
                    ChangeState(RepeatThreatStates.End);
                break;

            case RepeatThreatStates.End:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case RepeatThreatStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(RepeatThreatStates.ExperimentWave);
                break;


            case RepeatThreatStates.Interval:
                //if (GetTimeInState() > 2.0f)
                //    testLights.SetActive(true);
                //if (trialController.currentWave < trialController.wavesRequired)
                //    ChangeState(RepeatThreatStates.ExperimentWave);
                //if (trialController.currentWave == trialController.wavesRequired)
                //    ChangeState(RepeatThreatStates.Threat);
                break;

            case RepeatThreatStates.ExperimentWave:
                if (GetTimeInState() > 0.5f)
                    waveController.StartMachine();
                break;

            case RepeatThreatStates.Threat:
                if (GetTimeInState() > 0.5f)
                    threatController.StartMachine();
                threatController.Stopped += (sender, e) => { HandleEvent(RepeatThreatEvents.ThreatDone); };
                break;

            case RepeatThreatStates.End:
                break;
        }
    }


    protected override void OnEnter(RepeatThreatStates oldState)
    {

        switch (GetState())
        {
            case RepeatThreatStates.Idle:
                handSwitcher.showRightHand = true;
                break;


            case RepeatThreatStates.ExperimentWave:
                break;

            case RepeatThreatStates.Threat:
                threatController.threat.SetActive(true);
                break;

            case RepeatThreatStates.End:
                trialController.HandleEvent(TrialEvents.SpecificTrialFinished);
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(RepeatThreatStates newState)
    {
        switch (GetState())
        {
            case RepeatThreatStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case RepeatThreatStates.ExperimentWave:
                //waveController.StopMachine();
                //if (trialController.wavesRequired == trialController.currentWave)
                //    testLights.SetActive(false);
                break;

            case RepeatThreatStates.Threat:
                threatController.threat.SetActive(false);
                break;

            case RepeatThreatStates.End:
                break;
        }
    }
}

