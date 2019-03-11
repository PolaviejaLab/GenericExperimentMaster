﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;


public enum QuestionnaireEvents {
    StartQuestionnaire,
    QuestionDisplayed,
    QuestionAnswered,
    QuestionnaireDone,
    }


public enum QuestionnaireStates {
    Idle,
    QuestionnaireStarted,
    ShowQuestion,
    WaitingForAnswer,
    Delay, 
    End,
}


public class QuestionnaireController : ICStateMachine<QuestionnaireStates, QuestionnaireEvents>
{
    public TrialController trialController;

    public GameObject screen;
    public Text text;

    string[] statements = new string[] {
        "This is the text I want to show in the screen",
        "I love Lady",
        "I need a third statement",
        "And maybe a fourth"
    };

    public int totalLength;
    public int currentStatement = 0;
    public int[] arrayNum;

    public StreamWriter questionnaireResults;
    public KeyCode cKey;
    public KeyCode responseLikert;


    public void Start()
    {
    }

    protected override void OnStart()
    {
        questionnaireResults = OpenResultsFile();
        totalLength = statements.Length;
        Debug.Log("Total questions: " + totalLength.ToString());
        arrayNum = CreateNumericArray();
    }

    void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.QuestionnaireStarted:
                if (GetTimeInState() > 1.0f)
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;

            case QuestionnaireStates.ShowQuestion:
                break;

            case QuestionnaireStates.WaitingForAnswer:
                if (Input.anyKeyDown)
                    foreach (KeyCode cKey in Enum.GetValues(typeof(KeyCode)))
                        if (Input.GetKey(cKey))
                        {
                            Debug.Log("Question Answer: " + cKey);
                            responseLikert = cKey;
                            HandleEvent(QuestionnaireEvents.QuestionAnswered);
                        }
                break;

            case QuestionnaireStates.Delay:
                if (GetTimeInState() > 0.5f)
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;
        }
    }

    public void HandleEvent(QuestionnaireEvents ev)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                if (ev == QuestionnaireEvents.StartQuestionnaire)
                    ChangeState(QuestionnaireStates.QuestionnaireStarted);
                break;

            case QuestionnaireStates.ShowQuestion:
                if (ev == QuestionnaireEvents.QuestionDisplayed)
                    ChangeState(QuestionnaireStates.WaitingForAnswer);
                break;

            case QuestionnaireStates.WaitingForAnswer:
                if (ev == QuestionnaireEvents.QuestionAnswered)
                    ChangeState(QuestionnaireStates.Delay);
                break;

            case QuestionnaireStates.Delay:
                break;

            case QuestionnaireStates.End:
                trialController.HandleEvent(TrialEvents.QuestionsFinished);
                this.StopMachine();
                break;
        }
    }

    protected override void OnEnter(QuestionnaireStates oldState)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.QuestionnaireStarted:
                break;

            case QuestionnaireStates.ShowQuestion:
                screen.SetActive(true);
                currentStatement = GetRandomNumber(arrayNum);
                Debug.Log("Question number: " + currentStatement);
                DisplayText();
                break;

            case QuestionnaireStates.WaitingForAnswer:

                break;

            case QuestionnaireStates.Delay:
                arrayNum = RemoveNumber(arrayNum, currentStatement);
                if (arrayNum == null)
                {
                    ChangeState(QuestionnaireStates.End);
                }
                else { 
                    
                    ChangeState(QuestionnaireStates.ShowQuestion);
                }
                break;

            case QuestionnaireStates.End:
                questionnaireResults.Close();
                HandleEvent(QuestionnaireEvents.QuestionnaireDone);
                break;
        }

    }

    protected override void OnExit(QuestionnaireStates newState)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.QuestionnaireStarted:
                break;

            case QuestionnaireStates.ShowQuestion:

                break;

            case QuestionnaireStates.WaitingForAnswer:
                screen.SetActive(false);
                RecordResponse(responseLikert);
                break;

            case QuestionnaireStates.Delay:
                break;
        }

    }

    private string GetResultsFilename()
    {
        return trialController.experimentController.outputDirectory + "\\" + "Responses Trial " + trialController.experimentController.trialCounter + ".csv";
    }

    public StreamWriter OpenResultsFile()
    {
        Debug.Log("Document opened: " + GetResultsFilename().ToString());
        return questionnaireResults = new StreamWriter(GetResultsFilename(), true);
    }


    public int[] CreateNumericArray() {
        int[] arrayNum = new int[totalLength];
        for (int n = 0; n <= totalLength - 1; n++)
            arrayNum[n] = n;
        Debug.Log("Numeric array created");
        return arrayNum;
    }

    public void DisplayText()
    {
        text.text = statements[currentStatement].ToString();
        HandleEvent(QuestionnaireEvents.QuestionDisplayed);
    }

    public void RecordResponse(KeyCode response) {
        questionnaireResults.Write((currentStatement + 1).ToString());
        questionnaireResults.Write(", ");
        questionnaireResults.Write(response);
        questionnaireResults.WriteLine();
    }



    public int GetRandomNumber(int[] arrayInt)
    {
        int ind_ = UnityEngine.Random.Range(0, arrayInt.Length - 1);
        currentStatement = arrayInt[ind_];
        return currentStatement;
    }


    public int[] RemoveNumber(int[] arrayToRemove, int ind_)
    {
        int length = arrayToRemove.Length;
        int count = 0;
        int[] arrayLess = new int[length - 1];
        if (arrayLess.Length == 0)
        {
            return null; 
        }
        else
        {
            for (int n = 0; n < length; n++)
            {
                if (arrayToRemove[n] != ind_)
                {
                    int aa = count;
                    arrayLess[count] = arrayToRemove[n];
                    count++;
                }
            }
            return arrayLess;
        }
    } 
}
