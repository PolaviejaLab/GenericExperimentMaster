/**
 * State machine for a threat (e.g. knife) that
 * falls on a target (e.g. hand) and then follows it.
 */
 
using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;


/**
 * States the threat can be in
 */
public enum ThreatState {
	Initial,        // Idle state
	Falling,        // Falling on / towards target
	Following       // Stick to target
};


/**
 * Events handled by the threat state machine.
 */
public enum ThreatEvent {
    ReleaseThreat,      // Drop the threat
	TargetReached       // Target reached
};


public class Threat: ICStateMachine<ThreatState, ThreatEvent> 
{
    // Reference to parent classes
    public TrialController trialController;

    public LeapHandController handController;

    public GameObject threat;
	public Transform targetTransform;

	private float threatSpeed;
	private float gravity = 9.81f;

    public Vector3 threatOffset;
    public Vector3 handOffset;

    public float followingTimeout;

    public bool hideOnStopped = false;
    public bool isHeadMounted = false;

    Vector3 handPositionReWorld;

    Vector3 initialThreatPosition;
	Quaternion initialThreatRotation;
	Quaternion savedRotation;

     void Start() {
        // Store the initial transformation of the threat
        // this way we can reset it later
        initialThreatPosition = threat.transform.position;
		initialThreatRotation = threat.transform.rotation;

        if(hideOnStopped)
            threat.SetActive(false);

        followingTimeout  = 3.0f;

    }
	
    
    protected override void OnStart() {
       if(hideOnStopped)
            threat.SetActive(true);

    }
    
    
    protected override void OnStop() {
        if(hideOnStopped)
            threat.SetActive(false);
    }
    
    
	void Update () 
    {
        Frame frame = handController.Provider.CurrentFrame;
        
        foreach (var hand in frame.Hands) {
            if (hand.IsLeft) continue;
            Vector3 handPosition = hand.PalmPosition.ToVector3();

            handPositionReWorld = handPosition;

            Debug.Log(targetTransform.transform.position);
            break;
        }

        if (!IsStarted())
            return;

        switch (GetState()) {
            case ThreatState.Initial:

                break;
                       
            case ThreatState.Falling:
                if (!trialController.knifeOnReal) {
                    FallOnTarget();
                }
                else if (trialController.knifeOnReal) {
                    FallOnReal(handPositionReWorld);
                }
                break;

            case ThreatState.Following:
                if (!trialController.knifeOnReal) {
                    threat.transform.position = targetTransform.position + threatOffset / 30;
                    threat.transform.rotation = (targetTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (trialController.knifeOnReal) {
                    threat.transform.position = handPositionReWorld + handOffset;
                    threat.transform.rotation = (targetTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (GetTimeInState() > followingTimeout) {
                    StopMachine();
                }
                break;            
        }

        // If threat is close to target, emit TargetReached event
        if (Vector3.Distance(threat.transform.position, targetTransform.position + threatOffset / 30) < 0.001 && !trialController.knifeOnReal) {
            HandleEvent(ThreatEvent.TargetReached);
        }

        if (Vector3.Distance(threat.transform.position, handPositionReWorld + handOffset) < 0.001 && trialController.knifeOnReal) {
            HandleEvent(ThreatEvent.TargetReached);
        }
    }


    public void HandleEvent(ThreatEvent ev) {
        switch(GetState()) {
            case ThreatState.Initial:
                if(ev == ThreatEvent.ReleaseThreat)
                    ChangeState(ThreatState.Falling);
                break;
                
            case ThreatState.Falling:
                if(ev == ThreatEvent.TargetReached)
                    ChangeState(ThreatState.Following);
                break;
        }
    }


	protected override void OnEnter(ThreatState oldState){
        switch(GetState()) {
            case ThreatState.Falling:
                threat.transform.position += threatOffset/30;
                threatSpeed = 0.0f;
                break;

            case ThreatState.Following:
                savedRotation = targetTransform.rotation;
                break;
        }
	}    


	protected override void OnExit(ThreatState newState) {
        switch(GetState()) {
            // Reset initial position and rotation when following is complete
            case ThreatState.Initial:
   
                break;

            case ThreatState.Following:
                threat.transform.position = initialThreatPosition;
                threat.transform.rotation = initialThreatRotation;
                break;
        }        
	}


    /**
     * Advances the threat position such that is falls on the target
     */
	private void FallOnTarget() {
		threatSpeed += gravity * Time.deltaTime;
		        
		threat.transform.position = Vector3.MoveTowards(
            threat.transform.position, 
            targetTransform.position + threatOffset/30, // find the right proportion
			threatSpeed * Time.deltaTime);
	}

    private void FallOnReal(Vector3 handPositionReWorld) {
        threatSpeed += gravity * Time.deltaTime;

        threat.transform.position = Vector3.MoveTowards(
            threat.transform.position,
            handPositionReWorld + handOffset,
            threatSpeed * Time.deltaTime);
    }
}
