using UnityEngine;
using System.Collections;

public enum TrialStates {
    Instructions,
    SpecificTrial,
    Interval,
    DimLights,
    Questionnaire,
    End,
}

public enum TrialEvents {
    Waved,
    SpecificTrialFinished,
    RoomOff,
    QuestionsFinished,
}


public class TrialController : ICStateMachine<TrialStates, TrialEvents>

{
    // Reference to parent classes
    public ExperimentController experimentController;

    // Specific trials classes
    public ImplicitOwnershipTrial ownershipTrialController;
    public ImplicitAgencyTrial agencyTrialController;
    public ElementsAgencyTrial specificTrialController;
    public QuestionnaireController questionnaireController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;
    public GameObject feedback;

    public Light[] roomLights;
    public MaterialChanger[] roomWalls;
    private bool lightsOff = false;
    private float intensity_initial;

    // Parameters of the current trial
    public int hand;
    public float offset;
    public float noiseLevel;
    public float lNoise;
    public bool changeGender;
    public bool genderChanged;
    public bool ignoreUpdate;
    public int noiseType;
    public float delayWave;

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
    public bool knifeOnReal;

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

        intensity_initial = roomLights[0].intensity;
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

            case TrialStates.SpecificTrial:
                if (ev == TrialEvents.Waved)
                    specificTrialController.ChangeState(ElementsAgencyStates.Interval);
                if (ev == TrialEvents.SpecificTrialFinished)
                    ChangeState(TrialStates.DimLights);
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.DimLights:
                if (ev == TrialEvents.RoomOff)
                    ChangeState(TrialStates.Questionnaire);
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
                    ChangeState(TrialStates.SpecificTrial);
                break;

            case TrialStates.SpecificTrial:
                break;

            case TrialStates.Interval:
                if (GetTimeInState() > 2.0f) {
                    ChangeState(TrialStates.DimLights);
                }
                break;

            case TrialStates.DimLights:
                if (GetTimeInState() > 0.1f && !lightsOff)
                    DimLights();
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

            case TrialStates.SpecificTrial:
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

                    case ExperimentType.ElementsAgencyTrial:
                        specificTrialController.StartMachine();
                        WriteLog("Elements Agency Experiment State Machine started");
                        break;
                }
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.DimLights:
                handSwitcher.showRightHand = false;
                   
                break;

            case TrialStates.Questionnaire:
                questionnaireController.StartMachine();
                questionnaireController.HandleEvent(QuestionnaireEvents.StartQuestionnaire);
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

            case TrialStates.SpecificTrial:
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
            while (l.intensity > 0)
                l.intensity = l.intensity - 0.0001f;
        TurnOffRoom();
    }


    private void TurnOffRoom()
    {
        foreach (MaterialChanger i in roomWalls)
            i.activeMaterial = 1;
        table.SetActive(false);
        testLights.SetActive(false);
        feedback.SetActive(false);
        lightsOff = true;
        HandleEvent(TrialEvents.RoomOff);
    }



    private void resetRoomLight()
    {
        foreach (MaterialChanger i in roomWalls)
            i.activeMaterial = 0;
        foreach (Light l in roomLights)
            l.intensity = intensity_initial;
        table.SetActive(true);
        feedback.SetActive(true);
        lightsOff = false;
    }
}

