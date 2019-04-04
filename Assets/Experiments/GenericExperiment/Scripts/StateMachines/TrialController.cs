using UnityEngine;
using System.Collections;

public enum TrialStates {
    Idle,
    SpecificTrial,
    // DimLights,
    Questionnaire,
    Interval,
    End,
}

public enum TrialEvents {
    TaskFinished,
    SpTrialFinished,
    // RoomOff,
    QuestionsFinished,
}

public enum Noise
{
    Control,                    // No Action Noise and No Outcome noise
    ImpairedMovement,           // Action noise -- eg. experiment 1
    ImpairedOutcome,            // Outcome noise -- 
    BothImpaired,               // Both noise 
}


public class TrialController : ICStateMachine<TrialStates, TrialEvents>

{
    // Reference to parent classes
    public ExperimentController experimentController;

    // Specific trials classes
    //public ImplicitOwnershipTrial ownershipTrialController;
    //public ImplicitAgencyTrial agencyTrialController;
    public VisuomotorAgency visuomotorTrialController ;
    public OutcomeOwnership outcomeOwnershipTrialController;


    // Reference to child classes
    public QuestionnaireController questionnaireController;

    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public OffsetSwitcher offsetSwitcher;

    public GameObject testLights;
    public GameObject room;
    public GameObject table;
    public GameObject feedback;



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
    }


    public void HandleEvent(TrialEvents ev)
    {
        WriteLog("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case TrialStates.Idle:
                break;

            case TrialStates.SpecificTrial:
                if (ev == TrialEvents.SpTrialFinished)
                    ChangeState(TrialStates.Questionnaire);
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.Questionnaire:
                if (ev == TrialEvents.QuestionsFinished) {
                    questionnaireController.StopMachine();
                    ChangeState(TrialStates.End);
                }
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
            case TrialStates.Idle:
                if (GetTimeInState() > 2.0f)
                    ChangeState(TrialStates.SpecificTrial);
                break;

            case TrialStates.SpecificTrial:
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.Questionnaire:
                if (GetTimeInState() > 1.5f)
                    questionnaireController.DimLights();
                break;

            case TrialStates.End:
                break;
        }
    }


    protected override void OnEnter(TrialStates oldState)
    {
        switch (GetState())
        {
            case TrialStates.Idle:
                handSwitcher.showRightHand = true;
                testLights.SetActive(true);
                break;

            case TrialStates.SpecificTrial:
                switch (experimentType)
                {
                    //case ExperimentType.ImplicitAgencyTest:
                    //    agencyTrialController.StartMachine();
                    //    WriteLog("Implicit Agency Trial State Machine Started");
                    //    break;

                    //case ExperimentType.ImplicitOwnershipTest:
                    //    ownershipTrialController.StartMachine();
                    //    WriteLog("Implicit Ownership Trial State Machine Started");
                    //    break;

                    case ExperimentType.VisuomotorInformation:
                        visuomotorTrialController.StartMachine();
                        WriteLog("Visuomotor Information Trial started");
                        break;

                    case ExperimentType.OutcomeOwnership:
                        outcomeOwnershipTrialController.StartMachine();
                        WriteLog("Outcome Ownership Trial started");
                        break;
                }
                break;

            case TrialStates.Interval:
                break;



            case TrialStates.Questionnaire:
                handSwitcher.showRightHand = false;
                questionnaireController.StartMachine();
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
            case TrialStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case TrialStates.SpecificTrial:
                break;

            case TrialStates.Interval:
                break;

            case TrialStates.Questionnaire:
                break;

            case TrialStates.End:
                break;
        }
    }





}

