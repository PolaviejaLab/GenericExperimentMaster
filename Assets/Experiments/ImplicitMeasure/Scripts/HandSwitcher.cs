using UnityEngine;
using System.Collections;
using Leap.Unity;

public class HandSwitcher : MonoBehaviour
{

    public GameObject[] leftGraphicsModelMale;
    public GameObject[] rightGraphicsModelMale;

    public GameObject[] leftGraphicsModelFemale;
    public GameObject[] rightGraphicsModelFemale;

    public int selected;
    private int previous = -1;

    public bool useMale;
    private bool oldUseMale;

    public bool showLeftHand = true;
    public bool showRightHand = true;

    public float noiseLevelLeft;
    public float noiseLevelRight;

    public float lambdaLeft;
    public float lambdaRight;

    public bool ignoreUpdatesLeft = false;
    public bool ignoreUpdatesRight = false;

    private bool oldIgnoreUpdatesLeft = true;
    private bool oldIgnoreUpdatesRight = true;

    private bool oldRight, oldLeft;
    private float oldNoiseRight, oldNoiseLeft;
    private float oldLambdaRight, oldLambdaLeft;

    // Use this for initialization
    void Start()
    {
        UpdateModels();
        UpdateNoiseLevels();
    }

    void Update()
    {
        if (selected < -1 || selected >= leftGraphicsModelMale.Length || selected >= rightGraphicsModelMale.Length)
            selected = 0;

        if (selected != previous || useMale != oldUseMale ||
            showLeftHand != oldLeft || showRightHand != oldRight ||
            noiseLevelLeft != oldNoiseLeft || noiseLevelRight != oldNoiseRight ||  
            lambdaLeft != oldLambdaLeft || lambdaRight != oldLambdaRight ||
            ignoreUpdatesLeft != oldIgnoreUpdatesLeft || ignoreUpdatesRight != oldIgnoreUpdatesRight)
        {
            UpdateModels();
            UpdateNoiseLevels();
        }


        previous = selected;
        oldUseMale = useMale;
        oldLeft = showLeftHand;
        oldRight = showRightHand;
        oldNoiseRight = noiseLevelRight;
        oldNoiseLeft = noiseLevelLeft;
        oldLambdaRight = lambdaRight;
        oldLambdaLeft = lambdaLeft;
        oldIgnoreUpdatesLeft = ignoreUpdatesLeft;
        oldIgnoreUpdatesRight = ignoreUpdatesRight;

    }

    protected void UpdateModels()
    {
        for (int i = 0; i < leftGraphicsModelMale.Length; i++)
            leftGraphicsModelMale[i].SetActive(i == selected && useMale && showLeftHand);
        for (int i = 0; i < rightGraphicsModelMale.Length; i++)
            rightGraphicsModelMale[i].SetActive(i == selected && useMale && showRightHand);

        for (int i = 0; i < leftGraphicsModelFemale.Length; i++)
            leftGraphicsModelFemale[i].SetActive(i == selected && !useMale && showLeftHand);
        for (int i = 0; i < rightGraphicsModelFemale.Length; i++)
            rightGraphicsModelFemale[i].SetActive(i == selected && !useMale && showRightHand);
    }


    /**
     * Change the noise levels 
     */
    protected void UpdateNoiseLevels()
    {
        for (int i = 0; i < leftGraphicsModelMale.Length; i++)
        {
            RiggedHandEx hand = leftGraphicsModelMale[i].GetComponent<RiggedHandEx>();

            if (hand == null) continue;

            hand.ignoreUpdates = ignoreUpdatesLeft;
            hand.noiseType = (noiseLevelLeft == 0) ? NoiseType.NoNoise : NoiseType.NormalRandomWalk;
            hand.noiseLevel = noiseLevelLeft;
            hand.lambda = lambdaLeft;
        }

        for (int i = 0; i < rightGraphicsModelMale.Length; i++)
        {
            RiggedHandEx hand = rightGraphicsModelMale[i].GetComponent<RiggedHandEx>();

            if (hand == null) continue;

            hand.ignoreUpdates = ignoreUpdatesRight;
            hand.noiseType = (noiseLevelRight == 0) ? NoiseType.NoNoise : NoiseType.NormalRandomWalk;
            hand.noiseLevel = noiseLevelRight;
            hand.lambda = lambdaRight;
        }

        for (int i = 0; i < leftGraphicsModelFemale.Length; i++)
        {
            RiggedHandEx hand = leftGraphicsModelFemale[i].GetComponent<RiggedHandEx>();

            hand.ignoreUpdates = ignoreUpdatesLeft;
            hand.noiseType = (noiseLevelLeft == 0) ? NoiseType.NoNoise : NoiseType.NormalRandomWalk;
            hand.noiseLevel = noiseLevelLeft;
            hand.lambda = lambdaLeft;
        }

        for (int i = 0; i < rightGraphicsModelFemale.Length; i++)
        {
            RiggedHandEx hand = rightGraphicsModelFemale[i].GetComponent<RiggedHandEx>();

            hand.ignoreUpdates = ignoreUpdatesRight;
            hand.noiseType = (noiseLevelRight == 0) ? NoiseType.NoNoise : NoiseType.NormalRandomWalk;
            hand.noiseLevel = noiseLevelRight;
            hand.lambda = lambdaRight;
        }

    }
}
