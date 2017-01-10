using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VRExtensionMethods{
    public static T GetRandom<T>(this List<T> list) {
        if (list == null || list.Count == 0)
            return default(T);
        return list[Random.Range(0, list.Count)];
    }
    public static T GetRandom<T>(this T[] list) {
        if (list == null || list.Length == 0)
            return default(T);
        return list[Random.Range(0, list.Length)];
    }
}
