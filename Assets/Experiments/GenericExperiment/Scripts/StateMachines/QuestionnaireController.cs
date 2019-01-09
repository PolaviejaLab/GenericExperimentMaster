using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;


public enum QuestionnaireEvents {
    StartQuestionnaire,
    QuestionAnswered,
    QuestionnaireDone,
    }


public enum QuestionnaireStates {
    Idle,
    QuestionnaireStarted,
    ShowQuestion,
    Delay, 
    End,
}




public class QuestionnaireController : ICStateMachine<QuestionnaireStates, QuestionnaireEvents>
{
    public TrialController trialController;

    public GameObject screen;
    public Text text;

    string[] statements = new string[] {
                "Lady1",
                "Lady2",
                "Lady3",
                "Lady4",
                "Lady5",
                "Lady6",
                "Lady7",
                "Lady8",
                "Lady9",
            };


    public int number = 0;
    public string teststrings;
    public int totalLength;
    public int selectedNumber;
    public int selectedQuestion;
    public int q;

    public int[] arrayNum;

    // Use this for initialization


    public void Start() {
    }

    protected override void OnStart()
    {
        totalLength = statements.Length;
        int[] arrayNum = new int[totalLength];
        Debug.Log("Questionnaire Length: " + totalLength);
        for (int n = 0; n <= totalLength - 1; n++)
        {
            arrayNum[n] = n + 1;
        }
    }

    // Update is called once per frame
    void Update () {
        if (!IsStarted())
            return;

        switch (GetState()) {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.QuestionnaireStarted:
                if (GetTimeInState() > 1.0f)
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;

            case QuestionnaireStates.ShowQuestion:           
                if (Input.GetKey(KeyCode.D))
                    HandleEvent(QuestionnaireEvents.QuestionAnswered);
                // record the response.
                // Wait for the response
                break;

            case QuestionnaireStates.Delay:
                if (GetTimeInState() > 0.5f)
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;
        }
	}


    public void HandleEvent(QuestionnaireEvents ev)
    {
        switch (GetState()) {
            case QuestionnaireStates.Idle:
                if (ev == QuestionnaireEvents.StartQuestionnaire)
                    ChangeState(QuestionnaireStates.QuestionnaireStarted);
                    break;

            case QuestionnaireStates.ShowQuestion:
                if (ev == QuestionnaireEvents.QuestionAnswered)
                    ChangeState(QuestionnaireStates.Delay);
                if (ev == QuestionnaireEvents.QuestionnaireDone)
                    ChangeState(QuestionnaireStates.End);
                break;

            case QuestionnaireStates.Delay:
                // record 
                // change the state in the trials controller. 
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
                if (q < totalLength)
                {
                    selectedQuestion = GetRandomNumber(arrayNum);

                    DisplayText(selectedQuestion);
                    screen.SetActive(true);

                }
                else
                {
                    HandleEvent(QuestionnaireEvents.QuestionnaireDone);
                }
                break;
 
                
     

            case QuestionnaireStates.Delay:
                q++;
                break;

            case QuestionnaireStates.End:
                trialController.HandleEvent(TrialEvents.QuestionsFinished);
                this.StopMachine();
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
                screen.SetActive(false);


                break;

            case QuestionnaireStates.Delay:
                // should record the questionnaire responses in an Array. after everytime it leaves this state. 
                break;
        }

    }

    public int GetRandomNumber(int[] arrayInt)
    {
        if (totalLength >= 1)
        {
            int ind_ = UnityEngine.Random.Range(1, totalLength);
            selectedNumber = arrayInt[ind_];
            Debug.Log("question number " + selectedNumber);
            arrayInt = RemoveNumber(arrayInt, ind_);           
        }
        else {
        }
        totalLength--;
        return selectedNumber;
    }

    public void DisplayText(int qNumber) {
        //text.text = outputText;
        text.text = statements[qNumber].ToString();
    }


    public int[] RemoveNumber(int[] arrayToRemove, int ind_)
    {
        int length = arrayToRemove.Length;
        int count = 0;
        int[] arrayNumberLess = new int[length-1];
        for (int n = 0; n <= length - 1; n++)
        {
            if (n != ind_)
            {
                arrayNumberLess[count]  = arrayToRemove[n];
                count++;
            }
        }
        return arrayNumberLess;
    }
}
