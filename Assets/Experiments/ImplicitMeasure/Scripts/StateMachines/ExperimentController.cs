﻿/**
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

using Leap;
using Leap.Unity;


public enum ExperimentStates
{
    Idle,
    Start,
    Trial,
    Delay,   
    End,
};

public enum ExperimentEvents
{
    ProtocolLoaded,
    TrialFinished,
    NextTrial,
    ExperimentFinished,
};

public enum ExperimentType
{
    ImplicitOwnership,          // Trial to test GSR to a threat to the virtual hand
    ImplicitAgency,             // Trial to test sensorymotor adaptation
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

    public TrialController trialController;
    public WaveController waveController;
    public ImplicitMeasure measureController;
    public Threat threatController;

    public HandController handController;
    public HandSwitcher handSwitcher;

    public getGender subjectCode;
    public getGender expInfo;

    public TableLights tableLights;

    private ICTrialList trialList;
    public int randomProtocol;
    public string protocolFile;

    private string outputDirectory;
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
                    ChangeState(ExperimentStates.Trial);
                break;


            case ExperimentStates.Trial:
                if (ev == ExperimentEvents.TrialFinished)
                {
                    SaveTrialResult();
                    ChangeState(ExperimentStates.Delay);
                }
                break;

            case ExperimentStates.Delay:
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

        switch (GetState())
        {
            case ExperimentStates.Idle:
                if (Input.GetKeyDown(KeyCode.Tab))
                    ChangeState(ExperimentStates.Start);
                else if (Input.GetKeyDown(KeyCode.Escape))
                    ChangeState(ExperimentStates.End);
                break;

            case ExperimentStates.Trial:
                break;

            case ExperimentStates.Delay:
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
                string[] dir = Directory.GetDirectories("Results");

                trialCounter = 0;

                for (int i = 0; i < dir.Length; i++)
                {
                    Debug.Log("subject code is " + subjectCode.subjectCode);
                    outputDirectory = "Results/ElementsAgency/" + subjectCode.subjectCode;
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
                {
                    WriteLog("Hand model is female");
                }
                else if (handSwitcher.useMale)
                {
                    WriteLog("Hand model is male");
                }

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
                if (trialList.HasMore())
                {
                    trialCounter++;
                    StartTrial();
                }
                else {
                    ChangeState(ExperimentStates.Idle);
                }
                break;

            case ExperimentStates.Delay:
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
        if (trial["GapStatus"] == "Inactive")
        {
            trialController.hand = 0;
        }
        else if (trial["GapStatus"] == "Active")
        {
            trialController.hand = 1;
        }
        else {
            WriteLog("Invalid GapSize in protocol");
            trialController.hand = -1;
        }
        WriteLog("Gap: " + trial["GapStatus"]);

        // Get offset
        float offset;
        try
        {
            float.TryParse(trial["HandOffset"], out offset);
            trialController.offset = offset / 100.0f;
        }
        catch (Exception e)
        {
            throw new Exception("Could not parse HandOffset in ProtocolFile");
        }

        WriteLog("HandOffset: " + offset);

        // Determine the number of waves per each trial
        int wavesRequired;
        int.TryParse(trial["WavesRequired"], out wavesRequired);
        waveController.wavesRequired = wavesRequired;

        // Determine noise type
        if (trial.ContainsKey("NoiseType"))
        {
            int.TryParse(trial["NoiseType"], out noiseType);
            WriteLog("Noise type " + noiseType);
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
            waveController.collisionProbability = collisionProbability;
            WriteLog("Collision probability: " + collisionProbability);
        }
        else {
            waveController.collisionProbability = 1.0f;
        }

        // Knife
        if (trial.ContainsKey("KnifePresent"))
        {
            if (trial["KnifePresent"].ToLower() == "true")
                waveController.knifePresent = true;
            else if (trial["KnifePresent"].ToLower() == "false")
                waveController.knifePresent = false;
            else
                throw new Exception("Invalid value in trial list for field KnifePresent");

            WriteLog("Knife Present" + waveController.knifePresent);
        }
        else {
            waveController.knifePresent = false;
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

        // Knife
        if (trial.ContainsKey("KnifeOnReal")) {
            if (trial["KnifeOnReal"].ToLower() == "true")
                threatController.knifeOnReal = true;
            else if (trial["KnifeOnReal"].ToLower() == "false")
                threatController.knifeOnReal = false;
            else
                throw new Exception("Invalid value in trial list for field KnifeOnReal");

            WriteLog("Knife on Real" + threatController.knifeOnReal);
        }

        // Temporal position of knife
        if (trial.ContainsKey("KnifeRandom")) {
            if (trial["KnifeRandom"].ToLower() == "false")
                waveController.randomizeThreatWave = false;
            else if (trial["KnifeRandom"].ToLower() == "true")
                waveController.randomizeThreatWave = true;
            else
                throw new Exception("Invalid value in trial list for field KnifeRandom");

            WriteLog("Random Wave for Threat: " + waveController.randomizeThreatWave);
        }

        // Gender Change
        if (trial.ContainsKey("ChangeGender")) {
            if (trial["ChangeGender"].ToLower() == "true") 
                trialController.changeGender = true;            
            else if (trial["ChangeGender"].ToLower() == "false") 
                trialController.changeGender = false;

        }

        switch (experimentType)
        {
            case ExperimentType.ImplicitAgency:
                trialController.experimentType = ExperimentType.ImplicitAgency;
                break;

            case ExperimentType.ImplicitOwnership:
                trialController.experimentType = ExperimentType.ImplicitOwnership;
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

        // Turn table lights on
        tableLights.isOn = true;
        handSwitcher.ignoreUpdatesRight = false;

        if (trial.ContainsKey("IgnoreUpdate"))
        {
            if (trial["IgnoreUpdate"].ToLower() == "false")
            {
                trialController.initialState = TrialStates.AccomodationTime;
                trialController.StartMachine();
            }
            else if (trial["IgnoreUpdate"].ToLower() == "true")
            {
                // inactiveTrialController.initialState = InactiveTrialStates.AccomodationTime;
                // inactiveTrialController.StartMachine(); 
            }
            else {
                throw new Exception("Invalid value for IgnoreUpdate");
            }
            WriteLog("Right hand still " + handSwitcher.ignoreUpdatesRight);
        }
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

        if (waveController.knifePresent == true)
            writer.Write("Threat active, ");
        else if (waveController.knifePresent == false)
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

        if (threatController.knifeOnReal)
            writer.Write("Knife on the real hand, ");
        else if (!threatController.knifeOnReal)
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
        writer.Write(waveController.waveThreat);
        writer.Write(", ");
        writer.WriteLine();

        writer.Close();
    }

    private void SaveExperimentResult()
    {
        handController.StopRecording();
    }
}