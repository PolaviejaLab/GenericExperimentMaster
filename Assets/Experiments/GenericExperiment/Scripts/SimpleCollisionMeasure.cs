using UnityEngine;
using System.Collections;

public class SimpleCollisionMeasure : MonoBehaviour
{
    public ImplicitMeasure measureController;

    public MeasureEvents triggerEvent;
    public GameObject[] objects;
    public float probability;

    public bool CompareByName = false;

    void OnTriggerStay(Collider col)
    {
        //   Debug.Log(col.name);
        probability = Random.Range(0.01f, 0.99f);
        if (col.name == "HandContainer")
        {
            if (objects.Length == 0)
            {
                measureController.HandleEvent(triggerEvent);
            }
            else {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (CompareByName)
                    {
                        if (col.gameObject.name == objects[i].name)
                            measureController.HandleEvent(triggerEvent);
                    }
                    else {
                        if (col.gameObject == objects[i])
                            measureController.HandleEvent(triggerEvent);
                    }
                }
            }
        }
    }
}
