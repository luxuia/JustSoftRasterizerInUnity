﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//当前pass的信息
public static class ShaderGlobal {
    public static Matrix4x4 l2wMat;
    public static Matrix4x4 l2wInvTMat;
    public static Matrix4x4 MVPMat;
    public static Vector3 CameraPos;

    public static Color[] MainTex;
    public static int MainTexW, MainTexH;

    //漫反射光
    public static Color Albedo = Color.white;
    public static Color SpecularColor = Color.white;
    public static float Specular = 64f;

    //环境光
    public static Color Ambient = Color.gray;

    public static SoftRender.LightData[] lights;
}
