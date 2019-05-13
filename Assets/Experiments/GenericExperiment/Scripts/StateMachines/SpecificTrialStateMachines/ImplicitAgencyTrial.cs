using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum AgencyTrialEvents
{
    TaskFinished,
    MeasureDone,
};


/**
 * States of the Trial statemachine
 */
public enum AgencyTrialStates
{
    Idle,                       // Get used to the environment
    PreMeasure,                 // Implicit Measure (before task)
    ExperimentWave,             // Reaching-like task
    Interval,                      // In between measures and task
    PostMeasure,                // Implicit measure (after task)
    End,                        // End of the trial
};

public class ImplicitAgencyTrial : ICStateMachine<AgencyTrialStates, AgencyTrialEvents>
{
    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public ImplicitMeasure measureController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;
    public GameObject measure;

    // wave recording variables
    public int totWaves;
    public int correctWaves;
    public int incorrectWaves;
    public int lateWaves;


    protected override void OnStart()
    {
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
                    ChangeState(AgencyTrialStates.Interval);
                break;

            case AgencyTrialStates.ExperimentWave:
                if (ev == AgencyTrialEvents.TaskFinished)
                    ChangeState(AgencyTrialStates.PostMeasure);
                break;

            case AgencyTrialStates.PostMeasure:
                if (ev == AgencyTrialEvents.MeasureDone)
                    ChangeState(AgencyTrialStates.End);
                break;

            case AgencyTrialStates.End:
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

            case AgencyTrialStates.Interval:
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

            case AgencyTrialStates.End:
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

            case AgencyTrialStates.End:
                // experimentController.HandleEvent(ExperimentEvents.TrialFinished);
                trialController.HandleEvent(TrialEvents.SpTrialFinished);
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
                // totWaves = waveController.currentWave;
                // correctWaves = waveController.correctWaves;
                //incorrectWaves = waveController.incorrectWaves;
               // lateWaves = waveController.lateWaves;
                waveController.StopMachine();
                break;

            case AgencyTrialStates.PostMeasure:
                break;

            case AgencyTrialStates.End:
                break;
        }
    }
}
