using UnityEngine;
using System.Collections;
using System.IO;
using System;

using Leap;
using Leap.Unity;

/**
 * Our own extensions to the LEAP software.
 */
public class HandController : MonoBehaviour 
{
    public LeapHandController handController;
    public IHandModel[] models;

    private StreamWriter streamWriter;

    // Use this for initialization
    void Start ()
    {
        // Last time we found that the LEAP Unity Assets apply this transformation
        // matrix to convert the LEAP coordinates into Unity coordinates.
        //
        //  -> I've just confirmed that this occurs in LeapServiceProvider.cs, see CurrentFrame() function.
        //
        // That means that just using the PalmPosition from CurrentFrame() at this point is not a good idea, 
        // because it has been transformed.
        // There are two options: 
        //  1) We can try and invert the operation (multiply the aforementioned matrix, or it's inverse, by this vector).
        //  2) We can try to obtain the unprocessed position.
        //
        // -> If you look at LeapServiceProvider.cs, last function, you will see that is uses
        //    LeapHandController.Provider.GetLeapController().Frame
        //    To obtain the real frame (before transformation). I have changed the code below
        //    to do the same.
        //
        //  Your job is to verify that this code does indeed do what it's supposed to do.
        //  Perhaps make some recordings of you moving the hand in a known way and
        //  look at the data to see if it is correct.
        //
        //  Hope this helps... I've got a (customer) deadline this week, so might be a bit unresponsive (that's why 
        //  I forgot to call you last week as well)... should be more reachable next week :)
        // 

        Debug.Log(transform.worldToLocalMatrix.m00);
        Debug.Log(transform.worldToLocalMatrix.m01);
        Debug.Log(transform.worldToLocalMatrix.m02);
        Debug.Log(transform.worldToLocalMatrix.m03);
        Debug.Log(transform.worldToLocalMatrix.m10);
        Debug.Log(transform.worldToLocalMatrix.m11);
        Debug.Log(transform.worldToLocalMatrix.m12);
        Debug.Log(transform.worldToLocalMatrix.m13);
        Debug.Log(transform.worldToLocalMatrix.m20);
        Debug.Log(transform.worldToLocalMatrix.m21);
        Debug.Log(transform.worldToLocalMatrix.m22);
        Debug.Log(transform.worldToLocalMatrix.m23);
        Debug.Log(transform.worldToLocalMatrix.m30);
        Debug.Log(transform.worldToLocalMatrix.m31);
        Debug.Log(transform.worldToLocalMatrix.m32);
        Debug.Log(transform.worldToLocalMatrix.m33);
    }


    // Update is called once per frame
    void Update ()
    {
        if (!Application.isPlaying) return;
        if (!handController) return;

        UpdateRecorder();

        Frame frame = handController.Provider.CurrentFrame;

        foreach(var hand in frame.Hands) {
            foreach(var model in models) {
                if (model == null) continue;
                if (!model.enabled) continue;

                if (model.Handedness == Chirality.Left && !hand.IsLeft) continue;
                if (model.Handedness == Chirality.Right && !hand.IsRight) continue;

                model.SetLeapHand(hand);
                model.UpdateHand();
            }
        }
    }


    public void StartRecording(string filename) 
    {
        streamWriter = new StreamWriter(filename, true);
    }

    public void StopRecording() 
    {
        streamWriter.Close();
        streamWriter = null;
    }

    /**
	 * Writes a 3d vector to file
	 */
    private void WritePosition(StreamWriter writer, Vector vector) {
        writer.Write(vector.x);
        writer.Write(", ");
        writer.Write(vector.y);
        writer.Write(", ");
        writer.Write(vector.z);
    }


    protected void UpdateRecorder() 
    {
        if (streamWriter == null || handController == null)
            return;

   
        LeapServiceProvider provider = (LeapServiceProvider) handController.Provider;
        Frame frame = provider.GetLeapController().Frame();

        bool gotHand = false;

        streamWriter.Write(DateTime.UtcNow.ToString("o") + ", ");

        int num_hands = frame.Hands.Count;
        for (int h = 0; h < num_hands; ++h) {
            if (frame.Hands[h].IsRight) {
                gotHand = true;

                streamWriter.Write(frame.Hands[h].Confidence);
                streamWriter.Write(", ");
                
                WritePosition(streamWriter, frame.Hands[h].PalmPosition);
                break;
            }
        }

        if (!gotHand) {
            streamWriter.Write("False, 0, 0, 0");
        }

        streamWriter.WriteLine();
        streamWriter.Flush();
    }
}
