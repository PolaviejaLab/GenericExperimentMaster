/**
 * Main state machine controlling the flow of the experiment.
 *  This state machine is responsible for loading trial descriptions,
 *  running the appropriate trials in order and saving the results
 *  back to the data file.
 *
 * Logic pertaining to the flow of a trial should be placed in the
 *  trial state machine.
 */
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using Leap;
using Leap.Unity;


public enum ExperimentStates
{
    Idle,
    Start,
    Interval,
    Trial,
    End,
};

public enum ExperimentEvents
{
    ProtocolLoaded,
    TrialFinished,
    NextTrial,
};

public enum ExperimentType
{ 
    // Ensure that the Folder containing the Protocol and Results folder have the same name (caption sensitive)
    VisuomotorInformation,                  // Manipulate both action and outcome visual information
    OutcomeOwnership,                       // Manipulate outcome information and visual information of the appearance of the hand

    //    ImplicitOwnershipTest,             // Trial to test GSR to a threat to the virtual hand
    //    ImplicitAgencyTest,                // Trial to test sensorymotor adaptation
    //    Outcome1,                          // Experiment elements of agency
    //    Outcome2,                          // Seconds part of the elements of agency
    //    ThreatExperimentDiscontinuity,     // Repeat the experiment of discontinuity with the threat.
}


public class ExperimentController : ICStateMachine<ExperimentStates, ExperimentEvents>
{
    /**
     * Link to the Trial state machine which is responsible for the flow of a single trial.
     */

    /**
    * Links to state machines used by the Trial state machine (TrialController)
    * we need those here in order to load variables from the protocol file into those
    * state machines.
    *
    * FIXME: This is undesirable behaviour as it breaks the hierarchical organization.
    * Instead, we should tell the Trial state machine which should set variables on the
    * child machines.
    */

    // Reference to child classes
    public TrialController trialController;
    public WaveController waveController;
    
    public HandController handController;
    public HandSwitcher handSwitcher;

    public getInformation subjectCode;
    public getInformation expInfo;

    private ICTrialList trialList;
    public int randomProtocol;
    public string protocolFile;

    public string outputDirectory;
    private string participantName;

    public int trialCounter;
    private float nextTrialTimeOut = 1.5f;

    public ExperimentType experimentType;

    public int noiseType;

    public void Start()
    {
        // When the trial controller is stopped, invoke an event
        trialController.Stopped +=
            (sender, e) =>
                { HandleEvent(ExperimentEvents.TrialFinished); };

        this.StartMachine();
    }


    public void HandleEvent(ExperimentEvents ev)
    {
        WriteLog("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case ExperimentStates.Idle:
                break;

            case ExperimentStates.Start:
                if (ev == ExperimentEvents.ProtocolLoaded)
                    ChangeState(ExperimentStates.Interval);
                break;


            case ExperimentStates.Trial:
                if (ev == ExperimentEvents.TrialFinished)
                {
                    SaveTrialResult();
                    ChangeState(ExperimentStates.Interval);
                }
                break;

            case ExperimentStates.Interval:
                if (ev == ExperimentEvents.NextTrial) {
                    ChangeState(ExperimentStates.Trial);
                }
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case ExperimentStates.Idle:
                // Change the gender of the hand
                if (Input.GetKeyDown(KeyCode.F))
                {
                    handSwitcher.useMale = false;
                    WriteLog("Gender changed to female");
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    handSwitcher.useMale = true;
                    WriteLog("Gender changed to male");
                }
                break;

            case ExperimentStates.Trial:
                break;

            case ExperimentStates.Interval:
                if (GetTimeInState() > nextTrialTimeOut)
                    HandleEvent(ExperimentEvents.NextTrial);
                break;
        }
    }


    protected override void OnEnter(ExperimentStates oldState)
    {

        switch (GetState())
        {
            case ExperimentStates.Idle:
                break;

            case ExperimentStates.Start:
                WriteLog("Experiment has started");

                string[] dir = Directory.GetDirectories("Results");

                trialCounter = 0;

                for (int i = 0; i < dir.Length; i++)
                {
                    Debug.Log("subject code is " + subjectCode.subjectCode);
                    outputDirectory = "Results/" + expInfo.experimentName.ToString() + "/" + subjectCode.subjectCode;
                    Debug.Log("Output Directory " + outputDirectory);
                    if (!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                        break;
                    }
                    else {
                        Debug.Log("Subject code already exists");
                        this.StopMachine();
                    }
                }

                logger.OpenLog(GetLogFilename());

                // Record participant number to log-file
                WriteLog("Participant: " + subjectCode.subjectCode);

                if (!handSwitcher.useMale)
                    WriteLog("Hand model is female");
                else if (handSwitcher.useMale)
                    WriteLog("Hand model is male");

                string[] dirProtocol = Directory.GetFiles("Protocol/" + expInfo.experimentName.ToString() + "/");
                randomProtocol = UnityEngine.Random.Range(0, dirProtocol.Length);
                protocolFile = dirProtocol[randomProtocol];

                // Load protocol
                Debug.Log("Loading protocol: " + protocolFile);
                WriteLog("Protocol file " + protocolFile);
                trialList = new ICTrialList(protocolFile);
                WriteLog("Loaded " + trialList.Count() + " trials");

                HandleEvent(ExperimentEvents.ProtocolLoaded);

                break;

            case ExperimentStates.Trial:
                StartTrial();
                break;

            case ExperimentStates.Interval:
                if (trialList.HasMore())
                    trialCounter++;

                else
                    ChangeState(ExperimentStates.End);
                break;

            case ExperimentStates.End:
                StopMachine();
                WriteLog("Experiment Finished");
                break;
        }
    }


    protected override void OnExit(ExperimentStates newState)
    {
        switch (GetState())
        {
            case ExperimentStates.Idle:
                break;

            case ExperimentStates.Start:
                break;

            case ExperimentStates.Trial:
                break;

            case ExperimentStates.End:
                break;
        }
    }


    /**
     * Parse trial details and pass them to the trial controller
     */
    private void PrepareTrial(Dictionary<string, string> trial, TrialController trialController)
    {
        // Determine which hand to use for given gapsize
        if (trial["GapStatus"] == "Inactive") {
            trialController.hand = 0;
        }
        else if (trial["GapStatus"] == "Active") {
            trialController.hand = 1;
        }
        else {
            WriteLog("Invalid GapSize in protocol");
            trialController.hand = -1;
        }
        WriteLog("Gap: " + trial["GapStatus"]);

        // Get Hand offset
        float offset;
        try {
            float.TryParse(trial["HandOffset"], out offset);
            trialController.offset = offset / 100.0f;
        }
        catch (Exception e) {
            throw new Exception("Could not parse HandOffset in ProtocolFile");
        }

        WriteLog("HandOffset: " + offset);

        // Determine the number of waves per each trial
        int wavesRequired;
        int.TryParse(trial["WavesRequired"], out wavesRequired);
        trialController.wavesRequired = wavesRequired;

        // Determine noise type
        if (trial.ContainsKey("NoiseType"))
        {
            int.TryParse(trial["NoiseType"], out noiseType);
            WriteLog("Noise type " + noiseType);
            trialController.noiseType = noiseType;
        }

        // Noise level
        if (trial.ContainsKey("NoiseLevel"))
        {
            float noiseLevel;
            float.TryParse(trial["NoiseLevel"], out noiseLevel);
            trialController.noiseLevel = noiseLevel;
            WriteLog("NoiseLevel: " + noiseLevel);
        }
        else {
            trialController.noiseLevel = 0.0f;
        }

        if (trial.ContainsKey("NoiseLambda"))
        {
            float noiseLambda;
            float.TryParse(trial["NoiseLambda"], out noiseLambda);
            trialController.lNoise = noiseLambda;
            WriteLog("Lambda: " + noiseLambda);
        }
        else {
            trialController.lNoise = 0.0f;
        }


        // Determine collision probability
        if (trial.ContainsKey("CollisionProbability"))
        {
            float collisionProbability;
            float.TryParse(trial["CollisionProbability"], out collisionProbability);
            trialController.collisionProbability = collisionProbability;
            WriteLog("Collision probability: " + collisionProbability);
        }
        else {
            trialController.collisionProbability = 1.0f;
        }



        // Knife
        if (trial.ContainsKey("KnifePresent"))
        {
            if (trial["KnifePresent"].ToLower() == "true")
                trialController.knifePresent = true;
            else if (trial["KnifePresent"].ToLower() == "false")
                trialController.knifePresent = false;
            else
                throw new Exception("Invalid value in trial list for field KnifePresent");

            WriteLog("Knife Present" + trialController.knifePresent);
        }
        else {
            trialController.knifePresent = false;
        }


        // Knife Offset
        if (trial.ContainsKey("KnifeOffset"))
        {
            float knifeOffsetx; float knifeOffsety; float knifeOffsetz;
            float.TryParse(trial["OffsetX"], out knifeOffsetx);
            float.TryParse(trial["OffsetY"], out knifeOffsety);
            float.TryParse(trial["OffsetZ"], out knifeOffsetz);

            Vector3 knifeVector = new Vector3(knifeOffsetx, knifeOffsety, knifeOffsetz);

            trialController.knifeOffset = knifeVector;

            WriteLog("Knife Offset: " + knifeVector);
        }

        // Temporal position of knife
        if (trial.ContainsKey("KnifeRandom"))
        {
            if (trial["KnifeRandom"].ToLower() == "false")
                trialController.randomizeThreatWave = false;
            else if (trial["KnifeRandom"].ToLower() == "true")
                trialController.randomizeThreatWave = true;
            else
                throw new Exception("Invalid value in trial list for field KnifeRandom");

            WriteLog("Random Wave for Threat: " + trialController.randomizeThreatWave);
        }
        else {
            trialController.randomizeThreatWave = false;
        }

        if (trial.ContainsKey("KnifeOnReal"))
        {
            if (trial["KnifeOnReal"].ToLower() == "true")
                trialController.knifeOnReal = true;
            else if (trial["KnifeOnReal"].ToLower() == "false")
                trialController.knifeOnReal = false;
        }
        else {
            trialController.knifeOnReal = false;
        }


        // Gender Change
        if (trial.ContainsKey("ChangeGender")) {
            if (trial["ChangeGender"].ToLower() == "true") 
                trialController.changeGender = true;            
            else if (trial["ChangeGender"].ToLower() == "false") 
                trialController.changeGender = false;
        }


        if (trial.ContainsKey("IgnoreUpdate"))
        {
            if (trial["IgnoreUpdate"].ToLower() == "false") {
                trialController.ignoreUpdate = false;
            }
            else if (trial["IgnoreUpdate"].ToLower() == "true") {
                trialController.ignoreUpdate = true;
            }
            else
                throw new Exception("Invalid value for IgnoreUpdates");
        }

        switch (experimentType)
        {
            //case ExperimentType.ImplicitAgencyTest:
            //    trialController.experimentType = ExperimentType.ImplicitAgencyTest;
            //    break;

            //case ExperimentType.ImplicitOwnershipTest:
            //    trialController.experimentType = ExperimentType.ImplicitOwnershipTest;
            //    break;

            case ExperimentType.VisuomotorInformation:
                trialController.experimentType = ExperimentType.VisuomotorInformation;
                break;

            case ExperimentType.OutcomeOwnership:
                trialController.experimentType = ExperimentType.OutcomeOwnership;
                break;
        }
        
    }


    /**
	 * Start the next trial
	 */
    private void StartTrial()
    {
        // Do not start if already running
        if (trialController.IsStarted())
            return;

        WriteLog("Starting trial");

        // Get next trial from list and setup trialController
        Dictionary<string, string> trial = trialList.Pop();
        PrepareTrial(trial, trialController);

        handController.StartRecording(GetLEAPFilename(trialCounter));

        handSwitcher.ignoreUpdatesRight = false;

        trialController.StartMachine();
    }


    private string GetLEAPFilename(int trial)
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + " Trial " + trial + ".csv";
    }


    private string GetLogFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".log";
    }


    private string GetResultsFilename()
    {
        return outputDirectory + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm ") + participantName + ".csv";
    }


    /**
	 * Appends the result of the previous trial to the datafile
	 */
    private void SaveTrialResult()
    {
        StreamWriter writer = new StreamWriter(GetResultsFilename(), true);

        // Append result of trial to data file
        writer.Write(trialCounter);
        writer.Write(", ");

        if (trialController.hand == 0)
            writer.Write("Continuous Limb, ");
        else if (trialController.hand == 1)
            writer.Write("Discontinous Limb, ");
        else
            writer.Write("Gap unknown, ");

        if (trialController.knifePresent == true)
            writer.Write("Threat active, ");
        else if (trialController.knifePresent == false)
            writer.Write("Threat inactive, ");
        else
            writer.Write("Threat unknown, ");


        if (noiseType == 0)
            writer.Write("Hand noise inactive, ");
        else if (noiseType == 1)
            writer.Write("Bodiliy noise active, ");
        else if (noiseType == 2)
            writer.Write("Outcome noise active, ");
        else if (noiseType == 3)
            writer.Write("Bodily and outcome noise active, ");
        else
            writer.Write("no noise information available, ");

        if (trialController.knifeOnReal)
            writer.Write("Knife on the real hand, ");
        else if (!trialController.knifeOnReal)
            writer.Write("Knife on the virtual hand, ");

        if (handSwitcher.useMale)
        {
            writer.Write("Male model, ");
        }
        else if (!handSwitcher.useMale)
        {
            writer.Write("Female model, ");
        }


        writer.Write(trialController.offset);
        writer.Write(", ");
        writer.Write(waveController.collisionProbability);
        writer.Write(", ");
        writer.Write(trialController.totWaves);
        writer.Write(", ");
        writer.Write(trialController.correctWaves);
        writer.Write(", ");
        writer.Write(trialController.incorrectWaves);
        writer.Write(", ");
        writer.Write(trialController.lateWaves);
        writer.Write(", ");
        // writer.Write(trialController.threatWave); // This will also need to be recorded. 
        // writer.Write(", ");
        writer.WriteLine();

        writer.Close();
    }

    private void SaveExperimentResult()
    {
        handController.StopRecording();
    }
}
