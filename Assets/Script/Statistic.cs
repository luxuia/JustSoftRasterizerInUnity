using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SoftRender {

    public int VertexCount;
    public int FragmentCount;
    public int EarlyZCount;
    public int FinalWriteCount;

    public void ClearStatistic() {
        VertexCount = 0;
        FragmentCount = 0;
        EarlyZCount = 0;
        FinalWriteCount = 0;
    }
}
