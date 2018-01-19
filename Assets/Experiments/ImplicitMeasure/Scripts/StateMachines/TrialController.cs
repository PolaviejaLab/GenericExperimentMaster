using UnityEngine;
using System.Collections;

public enum TrialStates {
    Instructions,
    Task,
    Interval,
    DimLights,
    Questionnaire,
    End,
}

public enum TrialEvents {
    TaskFinished,
    LightsDimmed,
    QuestionsFinished,
}


public class TrialController : ICStateMachine<TrialStates, TrialEvents>

{
    // Reference to the experiment controller
    public ExperimentController experimentController;
    public ImplicitOwnershipTrial ownershipTrialController;
    public ImplicitAgencyTrial agencyTrialController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;

    public Light[] roomLights;
    public MaterialChanger[] roomWalls;
    private bool lightsDimmed = false;
    private bool lightsOff = false;

    // Parameters of the current trial
    public int hand;
    public float offset;
    public float noiseLevel;
    public float lNoise;
    public bool changeGender;
    public bool genderChanged;
    public bool ignoreUpdate;

    // wave recording variables
    public int wavesRequired;
    public int totWaves;
    public int currentWave;
    public int correctWaves;
    public int incorrectWaves;
    public int lateWaves;

    public float collisionProbability;

    // Parameters of the current trial threat
    public bool knifePresent;
    public bool randomizeThreatWave;
    public Vector3 knifeOffset;

    // var to determine trial type
    public ExperimentType experimentType;


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

    }


    public void HandleEvent(TrialEvents ev)
    {
        WriteLog("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case TrialStates.Instructions:
                break;

            case TrialStates.Task:
                if (ev == TrialEvents.TaskFinished) {
                    ChangeState(TrialStates.Interval);
                }
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.DimLights:
                if (ev == TrialEvents.LightsDimmed && lightsDimmed)
                {
                    foreach (MaterialChanger i in roomWalls)
                    {
                        i.activeMaterial = 1;
                    }
                    lightsOff = true;
                }
                break;

            case TrialStates.Questionnaire:
                if (ev == TrialEvents.QuestionsFinished)
                    ChangeState(TrialStates.End);
                break;

            case TrialStates.End:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case TrialStates.Instructions:
                if (GetTimeInState() > 2.0f)
                    ChangeState(TrialStates.Task);
                break;

            case TrialStates.Task:
                break;

            case TrialStates.Interval:
                if (GetTimeInState() > 2.0f) {
                    ChangeState(TrialStates.DimLights);
                }
                break;

            case TrialStates.DimLights:
                if (lightsOff)
                    ChangeState(TrialStates.Questionnaire);
                break;

            case TrialStates.Questionnaire:
                if (Input.GetKeyDown(KeyCode.W))
                    HandleEvent(TrialEvents.QuestionsFinished);
                break;

            case TrialStates.End:
                break;
        }
    }


    protected override void OnEnter(TrialStates oldState)
    {

        switch (GetState())
        {
            case TrialStates.Instructions:
                handSwitcher.showRightHand = true;
                testLights.SetActive(true);
                break;

            case TrialStates.Task:
                switch (experimentType)
                {
                    case ExperimentType.ImplicitAgencyTest:
                        agencyTrialController.StartMachine();
                        WriteLog("Implicit Agency Trial State Machine Started");
                        break;

                    case ExperimentType.ImplicitOwnershipTest:
                        ownershipTrialController.StartMachine();
                        WriteLog("Implicit Ownership Trial State Machine Started");
                        break;
                }
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.DimLights:
                break;

            case TrialStates.Questionnaire:
                handSwitcher.showRightHand = false;
                testLights.SetActive(false);
                break;

            case TrialStates.End:
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(TrialStates newState)
    {
        switch (GetState())
        {
            case TrialStates.Instructions:
                handSwitcher.showLeftHand = false;
                break;

            case TrialStates.Task:
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.DimLights:
                break;

            case TrialStates.Questionnaire:
                resetRoomLight();
                break;

            case TrialStates.End:
                break;
        }
    }

    private void DimLights()
    {
        foreach (Light l in roomLights)
        {
            l.intensity = 0;
        }
        lightsDimmed = true;
        HandleEvent(TrialEvents.LightsDimmed);
    }

    private void resetRoomLight()
    {
        foreach (MaterialChanger i in roomWalls)
        {
            i.activeMaterial = 0;
        }
        lightsDimmed = false;
        lightsOff = false;
    }
}
