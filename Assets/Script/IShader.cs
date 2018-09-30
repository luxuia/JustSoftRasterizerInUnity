using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SoftRender {

	public FragmentIn Vert(VertexIn v) {
        var vert = new FragmentIn();

        vert.worldPos = ShaderGlobal.l2wMat.MultiplyPoint3x4(v.pos);
        vert.normal = ShaderGlobal.l2wInvTMat.MultiplyVector(v.normal).normalized;
        vert.uv = v.uv;
        vert.color = v.color;

        Vector4 pos = v.pos;
        pos.w = 1;
        vert.vertex = ShaderGlobal.MVPMat * pos;

        return vert;
    }

    public Color Frag(FragmentIn v) {
        var color = ShaderGlobal.Ambient;
        Vector3 dir; // 光线的反方向
        float intensity; //光线强度
        Vector3 viewdir = (ShaderGlobal.CameraPos - v.worldPos).normalized;

        foreach(var light in ShaderGlobal.lights) {
            switch (light.type) {
                // 这一部分计算要预处理下
                case LightType.Directional:
                    dir = -light.transform.forward;
                    intensity = light.intensity;
                    break;
                case LightType.Point:
                    dir = light.transform.position - v.worldPos;
                    // 正常的intensity公式是 1/(a+b*dis+c*dis^2)
                    intensity = Mathf.Clamp01( light.intensity * (1- dir.magnitude/light.range));
                    
                    dir.Normalize();
                    break;
                case LightType.Spot:
                    dir = light.transform.position - v.worldPos;
                    float rho = Mathf.Max(0, Vector3.Dot(dir.normalized, -light.transform.forward));
                    intensity = Mathf.Clamp01((Mathf.Cos(light.spotAngle * Mathf.Deg2Rad) - rho) * light.intensity * (1- dir.magnitude/light.range));
                    dir.Normalize();
                    break;
                default:
                    dir = -light.transform.forward;
                    intensity = light.intensity;
                    break;
            }
            float diff = Mathf.Max(0, Vector3.Dot(v.normal, dir));
            Vector3 h = (dir + viewdir).normalized;
            float nh = Vector3.Dot(h, v.normal);
            float spec = Mathf.Pow(nh, ShaderGlobal.Specular);

            color += (ShaderGlobal.Albedo * light.color * diff + ShaderGlobal.SpecularColor * light.color * spec)*intensity;
      }

        return color;
        //return new Color(v.uv.x, v.uv.y, 0);
        //return new Color(v.normal.x, v.normal.y, v.normal.z, 1);
    }
}
