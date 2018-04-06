using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum ElementsAgencyEvents
{
    WaveFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum ElementsAgencyStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // One event of reaching-like task
    Interval,                   // In between measures and task
    Threat,                     // Knife
    End,                        // End of the trial
};


public enum Noise
{
    Control,                    
    ImpairedMovement,
    ImpairedOutcome,
    BothImpaired,
}

public class ElementsAgencyTrial : ICStateMachine<ElementsAgencyStates, ElementsAgencyEvents>
{
    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public Threat threatController;
    
    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public Noise noiseType;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;

    public void Start()
    {
    }

    protected override void OnStart()
    {
        switch (trialController.noiseType) {
            case 0:
                noiseType = Noise.Control;
                waveController.delayWave = 0.0f;
                break;

            case 1:
                noiseType = Noise.ImpairedMovement;
                waveController.delayWave = 0.0f;
                break;

            case 2:
                noiseType = Noise.ImpairedOutcome;
                waveController.delayWave = 0.5f;
                break;

            case 3:
                noiseType = Noise.BothImpaired;
                waveController.delayWave = 0.5f;
                break;

        }
    }


    public void HandleEvent(ElementsAgencyEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case ElementsAgencyStates.Idle:
                break;

            case ElementsAgencyStates.ExperimentWave:
                if (ev == ElementsAgencyEvents.WaveFinished)
                    ChangeState(ElementsAgencyStates.Interval);
                break;

            case ElementsAgencyStates.Threat:
                if (ev == ElementsAgencyEvents.ThreatDone)
                    ChangeState(ElementsAgencyStates.End);
                break;

            case ElementsAgencyStates.End:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case ElementsAgencyStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(ElementsAgencyStates.ExperimentWave);
                break;


            case ElementsAgencyStates.Interval:
                if (GetTimeInState() > 2.0f)
                    testLights.SetActive(true);
                if (trialController.currentWave < trialController.wavesRequired)
                    ChangeState(ElementsAgencyStates.ExperimentWave);
                if (trialController.currentWave == trialController.wavesRequired)
                    ChangeState(ElementsAgencyStates.Threat);
                break;

            case ElementsAgencyStates.ExperimentWave:
                if (GetTimeInState() > 0.5f)
                    waveController.StartMachine();
                break;

            case ElementsAgencyStates.Threat:
                if (GetTimeInState() > 0.5f)
                    threatController.StartMachine();
                threatController.Stopped += (sender, e) => { HandleEvent(ElementsAgencyEvents.ThreatDone); };
                break;

            case ElementsAgencyStates.End:
                break;
        }
    }


    protected override void OnEnter(ElementsAgencyStates oldState)
    {

        switch (GetState())
        {
            case ElementsAgencyStates.Idle:
                handSwitcher.showRightHand = true;
                break;


            case ElementsAgencyStates.ExperimentWave:
                break;

            case ElementsAgencyStates.Threat:
                threatController.threat.SetActive(true);
                break;
                
            case ElementsAgencyStates.End:
                trialController.HandleEvent(TrialEvents.SpecificTrialFinished);
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(ElementsAgencyStates newState)
    {
        switch (GetState())
        {
            case ElementsAgencyStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case ElementsAgencyStates.ExperimentWave:
                waveController.StopMachine();
                if (trialController.wavesRequired == trialController.currentWave)
                    testLights.SetActive(false);
                break;

            case ElementsAgencyStates.Threat:
                threatController.threat.SetActive(false);
                break;

            case ElementsAgencyStates.End:
                break;
        }
    }
}
