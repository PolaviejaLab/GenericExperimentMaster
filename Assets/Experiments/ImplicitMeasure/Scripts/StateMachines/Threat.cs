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
	Initial,    // Idle state
	Falling,    // Falling on / towards target
	Following   // Stick to target
};


/**
 * Events handled by the threat state machine.
 */
public enum ThreatEvent {
    ReleaseThreat,  // Drop the threat
	TargetReached   // Target reached
};


public class Threat: ICStateMachine<ThreatState, ThreatEvent> 
{
    public LeapHandController handController;

    public GameObject threat;
	public Transform targetTransform;

	private float threatSpeed;
	private float gravity = 9.81f;

    public Vector3 threatOffset;
    public Vector3 handOffset;

    public bool knifeOnReal;

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

        followingTimeout = 3.0f;
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
        // Get the latest frame
        Frame frame = handController.Provider.CurrentFrame;
        
        // Get hand position re world for first right hand in scene.
        // If no hands are found, this results in handPositionReWorld being (0, 0, 0)!!!! We might want to fix this!
        foreach (var hand in frame.Hands) {
            // Together with the break; a very ugly way to make sure only one right hand position is processed
            if (hand.IsLeft) continue;
            //if (!hand.IsValid) continue;

            // I think there should be a function for this from LEAP, but I guess this works too.
            /* Vector3 handPosition = new Vector3(
                hand.PalmPosition.x,
                hand.PalmPosition.y,
                hand.PalmPosition.z); */
            //hand.ScaleFactor 
            Vector3 handPosition = hand.PalmPosition.ToVector3();

            // This converts the local hand coordinates (relative to LEAP) to Unity world coordinates
            //handPositionReWorld = handController.transform.TransformPoint(handPosition);
            handPositionReWorld = handPosition;
            //Debug.Log(handPositionReWorld);
            
            Debug.Log(targetTransform.transform.position);
            break;

        
        }

        if (!IsStarted())
            return;

        switch (GetState()) {        
            case ThreatState.Falling:
                if (!knifeOnReal) {
                    FallOnTarget();
                }
                else if (knifeOnReal) {
                    FallOnReal(handPositionReWorld);
                }
                break;

            case ThreatState.Following:
                if (!knifeOnReal) {
                    threat.transform.position = targetTransform.position + threatOffset / 30;
                    threat.transform.rotation = (targetTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (knifeOnReal) {
                    threat.transform.position = handPositionReWorld + handOffset;
                    threat.transform.rotation = (targetTransform.rotation * Quaternion.Inverse(savedRotation)) * initialThreatRotation;
                }

                if (GetTimeInState() > followingTimeout) {
                    StopMachine();
                }
                break;            
        }

        // If threat is close to target, emit TargetReached event
        if (Vector3.Distance(threat.transform.position, targetTransform.position + threatOffset / 30) < 0.001 && !knifeOnReal) {
            HandleEvent(ThreatEvent.TargetReached);
        }

        if (Vector3.Distance(threat.transform.position, handPositionReWorld + handOffset) < 0.001 && knifeOnReal) {
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
