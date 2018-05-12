using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestionnaireController : MonoBehaviour {

    public GameObject screen;
    public Text text;

    string[] names = new string[] {
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
    

    // Use this for initialization
    void Start () {
        totalLength = names.Length;
        int[] arrayNum = new int[totalLength];
        for (int n = 0; n <= totalLength-1; n++)
        {
            arrayNum[n] = n;
        }
        RandomizeArray(arrayNum);
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void RandomizeArray(int[] arrayInt) {
        totalLength = names.Length;
        while (totalLength >= totalLength-1)
        {
            int ind_ = Random.Range(0, totalLength-1);
            int selectedNumber = arrayInt[ind_];
            arrayInt = RemoveNumber(arrayInt, ind_);
            totalLength--;
            Debug.Log("Selected Lady: " + names[selectedNumber]);
            QuestionParticipant(names[selectedNumber]);
        }
}

    public void QuestionParticipant(string outputText) {
        screen.SetActive(true);
        text.text = outputText;
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