using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SoftRender {

	public FragmentIn Vert(VertexIn v) {
        var vert = new FragmentIn();

        vert.worldPos = WorldMat.MultiplyPoint3x4(v.pos);
        vert.normal = WorldInvTMat.MultiplyVector(v.normal).normalized;
        vert.uv = v.uv;
        vert.color = v.color;

        Vector4 pos = v.pos;
        pos.w = 1;
        vert.vertex = MVPMat * pos;

        return vert;
    }

    public Color Frag(FragmentIn v) {
        return new Color(v.normal.x, v.normal.y, v.normal.z, 1);
    }
}
