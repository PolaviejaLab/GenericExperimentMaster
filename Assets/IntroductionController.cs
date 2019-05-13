using UnityEngine;
using System.Collections;
using System;



/**
* States of the Wave State Machine
*/
public enum IntroductionStates {
    Idle,
    Introduction,
    FamiliarizationTime,
    End, 
};


/**
* Events handled by the Wave State Machine
*/
public enum IntroductionEvents {
    Started,

};




public class IntroductionController : ICStateMachine<IntroductionStates, IntroductionEvents> {

    public GameObject informationScreen;
    public GameObject display;


    protected override void OnStart() {
        informationScreen.SetActive(true);
        display.SetActive(true);
    }


    public void HandleEvent(IntroductionEvents ev) {

    }


    public void Update () {
	
	}


    protected override void OnEnter(IntroductionStates oldState) {

    }


    protected override void OnExit(IntroductionStates oldState) {

    }

}
