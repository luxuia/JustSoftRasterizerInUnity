using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Const {

    public const int ENCODE_SIDE_INSIDE = 0;
    public const int ENCODE_SIDE_LEFT = 1 << 0;
    public const int ENCODE_SIDE_RIGHT = 1 << 1;
    public const int ENCODE_SIDE_TOP = 1 << 2;
    public const int ENCODE_SIDE_BOTTOM = 1 << 3;

    public static void Swap<T>(ref T lhs, ref T rhs) {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}
