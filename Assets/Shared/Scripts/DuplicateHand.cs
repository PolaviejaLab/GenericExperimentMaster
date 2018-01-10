using UnityEngine;
using Leap.Unity;
using System.Collections;

/**
 * Structure of the object:
 *
 *  - BaseObject
 *   - Skinned mesh renderer
 *   - HandContainer
 *    - Hand
 *     - Finger (x5)
 *    - ForearmContainer (optional)
 */
public class DuplicateHand : MonoBehaviour
{
    public IHandModel template;

    public int min(int a, int b)
    {
        if (a > b)
            return b;
        return a;
    }

    public void CopyTransform(Transform src, Transform dst)
    {
        for (int i = 0; i < min(src.childCount, dst.childCount); i++)
        {            
            Transform srcT = src.GetChild(i);
            Transform dstT = dst.GetChild(i);

            CopyTransform(srcT, dstT);
        }

        dst.localPosition = src.localPosition;
        dst.localRotation = src.localRotation;
    }

    public void Update()
    {
        CopyTransform(template.transform, transform);
    }
}
