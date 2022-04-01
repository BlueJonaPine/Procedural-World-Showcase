using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    //public static bool ValueInTargetTolerance(float value, float target, float tolerance)
    //{
    //    if (((value <= target) && (value >= (target - tolerance))) ||
    //        ((value >= target) && (value <= (target + tolerance))))
    //        return true;
    //    else
    //        return false;
    //}
    public static bool ValueInTargetTolerance(float value, float target, float tolerance)
    {
        if (Mathf.Abs(Mathf.Abs(value) - Mathf.Abs(target)) <= tolerance)
            return true;
        else
            return false;
    }
}
