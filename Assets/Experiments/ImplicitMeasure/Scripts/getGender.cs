﻿using UnityEngine;
using System.Collections;

public class getGender : MonoBehaviour {

    public HandSwitcher handSwitcher;
    public ExperimentController experimentController;
    public TrialController trialController;    

    public string subjectCode;

    public int male_ = 0;
    public int female_ = 0;

    public int expNum;
    public string experimentName;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void getCode(string subjectName)
    {
        subjectCode = subjectName;
        Debug.Log("Subject Code " + subjectCode);
    }

    public void isMale(bool male) {
        handSwitcher.useMale = true;
        Debug.Log("Changed gender to male");
        if (female_ == 0) {
            male_ = 1;  
            Debug.Log("Gender is male");
       } else if (female_ == 1) {
            Debug.Log("Female already selected");
        }
    }

    public void isFemale(bool female) {
        handSwitcher.useMale = false;
        Debug.Log("Changed gender to female");
        if (male_ == 0) {
            female_ = 1;
            Debug.Log("Gender is female");
        } else if (male_ == 1) {
            Debug.Log("Male already selected");
        }
    }

    public void getExperimentNumber(int expNum) {
        Debug.Log("int passed: " + expNum);
        if (expNum == 1)
        {
            experimentController.experimentType = ExperimentType.ImplicitOwnership;
            Debug.Log("Testing implicit measure for sense of ownership");
        }
        else if (expNum == 2) {
            experimentController.experimentType = ExperimentType.ImplicitAgency;
            Debug.Log("Testing implicit measure for sense of agency");
        }
    }

    public void startExperiment() {
        experimentController.ChangeState(ExperimentStates.Start);
        Debug.Log("Experiment has started");
    }
}