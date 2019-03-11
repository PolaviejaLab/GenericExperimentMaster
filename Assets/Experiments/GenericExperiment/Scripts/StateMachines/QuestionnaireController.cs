using UnityEngine;
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
    };

    public int totalLength;
    public int currentStatement = 0;

    public StreamWriter questionnaireResults;
    public KeyCode cKey;




    //   public int number = -1;
    //   public string teststrings;
    //   public int selectedNumber;
    //   public int selectedQuestion;
    //   public int q;

    //   public int[] arrayNum;


    public void Start()
    {
    }

    protected override void OnStart()
    {
        totalLength = statements.Length;
        //       int[] arrayNum = new int[totalLength];
        //       Debug.Log("Questionnaire Length: " + totalLength);
        //       for (int n = 0; n <= totalLength - 1; n++)
        //       {
        //           arrayNum[n] = n + 1;
        //       }
        questionnaireResults = OpenResultsFile();
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
                    foreach (KeyCode cKey in System.Enum.GetValues(typeof(KeyCode)))
                        if (Input.GetKey(cKey))
                        {
                            Debug.Log("miaw key name : " + cKey);
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
                Debug.Log("Question number: " + currentStatement);
                DisplayText();
                HandleEvent(QuestionnaireEvents.QuestionDisplayed);
                break;


            //               if (q < totalLength)
            //               {
            //                   selectedQuestion = GetRandomNumber(arrayNum);
            //                   DisplayText(selectedQuestion);
            //                   screen.SetActive(true);


            case QuestionnaireStates.WaitingForAnswer:
                break;

            case QuestionnaireStates.Delay:
                if (currentStatement < totalLength - 1) {
                    currentStatement++;
                    ChangeState(QuestionnaireStates.ShowQuestion);
                }
                else if (currentStatement == totalLength - 1) {
                    ChangeState(QuestionnaireStates.End);
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
                RecordResponse(cKey);
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


    public void DisplayText()   // (int qNumber)
    {
        text.text = statements[currentStatement].ToString();
    }

    public void RecordResponse(KeyCode response) {
        questionnaireResults.Write((currentStatement + 1).ToString());
        questionnaireResults.Write(", ");
        questionnaireResults.Write(response);
        questionnaireResults.WriteLine();
    }



    //   public int GetRandomNumber(int[] arrayInt)
    //   {
    //       if (totalLength >= 1)
    //       {
    //           int ind_ = UnityEngine.Random.Range(1, totalLength);
    //           selectedNumber = arrayInt[ind_];
    //           Debug.Log("question number " + selectedNumber);
    //           arrayInt = RemoveNumber(arrayInt, ind_);           
    //       }
    //       else {
    //       }
    //       totalLength--;
    //       return selectedNumber;
    //   }

    //   public int[] RemoveNumber(int[] arrayToRemove, int ind_)
    //   {
    //       int length = arrayToRemove.Length;
    //       int count = 0;
    //       int[] arrayNumberLess = new int[length-1];
    //       for (int n = 0; n <= length - 1; n++)
    //       {
    //           if (n != ind_)
    //           {
    //               arrayNumberLess[count]  = arrayToRemove[n];
    //               count++;
    //           }
    //       }
    //       return arrayNumberLess;
    //   }
}
