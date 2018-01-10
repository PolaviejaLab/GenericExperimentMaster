using UnityEngine;
using System;
using System.Collections;

public class NormalRandom
{
    public static double Random(double mean = 0, double stdDev = 1)
    {
        System.Random rand = new System.Random();
        double u1 = rand.NextDouble();
        double u2 = rand.NextDouble();

        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mean + stdDev * randStdNormal;
    }


    public static Vector3 Random(Vector3 mean, Vector3 stdDev)
    {
        Vector3 rnd = new Vector3((float) Random(), (float) Random(), (float) Random());
        rnd.Scale(stdDev);
        return rnd + mean;
    }

}
