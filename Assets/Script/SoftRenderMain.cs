//#define USE_PARALLER
//#define DRAW_WIREFRAME

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;



public partial class SoftRender {

    public Texture2D texture; // color buffer
    public float[,] depthBuffer; // depth buffer
    public Color[] colorBuffer;
    public Camera camera;

    List<FragmentIn> vertexList;

    public Vector2 ClipLowLeft = new Vector2(10, 10);
    public Vector2 ClipUpRight;

    int width;
    int height;

	public SoftRender(Texture2D texture, Camera camera) {
        Reset(texture, camera);
    }

    public void Reset(Texture2D texture, Camera camera) {
        ClearStatistic();

        this.texture = texture;

        this.camera = camera;

        ClipUpRight = new Vector2(texture.width - 10, texture.height - 10);

        vertexList = new List<FragmentIn>();

        width = texture.width;
        height = texture.height;

        if (depthBuffer != null && depthBuffer.GetLength(0) == width && depthBuffer.GetLength(1) == height) {
            Array.Clear(depthBuffer, 0, width * height);
        }
        else {
            depthBuffer = new float[width, height];
        }
        if (colorBuffer != null && colorBuffer.Length == width*height) {
            Array.Clear(colorBuffer, 0, width * height);
        }
        else {
            colorBuffer = new Color[width * height];
        }
        var grey = new Color(0.1f, 0.1f, 0.1f, 0.1f);
        for (var i = 0; i < width * height; ++i) {
            colorBuffer[i] = grey;
        }

        var lights = GameObject.FindObjectsOfType<Light>();
        ShaderGlobal.lights = new LightData[lights.Length];
        for (int i = 0; i < lights.Length; ++i) {
            var lightdata = new LightData();
            var light = lights[i];
            lightdata.forward = light.transform.forward;
            lightdata.pos = light.transform.position;
            lightdata.spotAngle = light.spotAngle;
            lightdata.type = light.type;
            lightdata.range = light.range;
            lightdata.color = light.color;
            lightdata.intensity = light.intensity;

            ShaderGlobal.lights[i] = lightdata;
        }
        if (camera)
            ShaderGlobal.CameraPos = camera.transform.position;
    }

    void DrawPixel(int x, int y, ref Color color) {
        colorBuffer[x+ y*width] = color;
    }


    #region draw2d
    //Bresenham Alg
    public void DrawLine(VertexIn start, VertexIn end, ref Color color) {
        //裁剪不可见的线
        if (!CohenSutherlandLineClip(ref start.pos, ref end.pos, ClipLowLeft, ClipUpRight)) {
            return;
        }

        int x0 = (int)start.pos.x;
        int x1 = (int)end.pos.x;

        int y0 = (int)start.pos.y;
        int y1 = (int)end.pos.y;
        
        if (x0 == x1 && y0 == y1) {
            //点
            DrawPixel(x0, y0, ref color);
        } else if (x0 == x1) {
            // 竖线
            int inc = y0 > y1 ? -1 : 1;
            for (var y = y0; y != y1; y += inc) DrawPixel(x0, y, ref color);
            DrawPixel(x1, y1, ref color);
        } else if (y0 == y1) {
            // 横线
            int inc = x0 > x1 ? -1 : 1;
            for (var x = x0; x != x1; x += inc) DrawPixel(x, y0, ref color);
            DrawPixel(x1, y1, ref color);
        } else {
            int dx = (x0 < x1) ? x1 - x0 : x0 - x1;
            int dy = (y0 < y1) ? y1 - y0 : y0 - y1;

            // k < 0
            if (dx > dy) {
                if (x1 < x0) {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }

                int rem = 0, x, y;
                for (x = x0, y = y0; x<=x1; x++) {
                    DrawPixel(x, y, ref color);
                    rem += dy;
                    if (rem >= dx) {
                        rem -= dx;
                        y += (y1 > y0) ? 1 : -1;
                    }
                }
            } else {
                if (y1 < y0) {
                    Swap(ref x0, ref x1);
                    Swap(ref y0, ref y1);
                }
                int rem = 0, x, y;
                for (x = x0, y = y0; y<=y1; y++) {
                    DrawPixel(x, y, ref color);
                    rem += dx;
                    if (rem >= dy) {
                        rem -= dy;
                        x += (x1 > x0) ? 1 : -1;
                    }
                }
            }
        }
    }

    bool CohenSutherlandLineClip(ref Vector3 start, ref Vector3 end, Vector2 min, Vector2 max) {
        float x0 = start.x;
        float x1 = end.x;
        
        float y0 = start.y;
        float y1 = end.y;

        int encode0 = Encode(ref start, ref min, ref max);
        int encode1 = Encode(ref end, ref min, ref max);

        bool accept = false;

        while (true) {
            if ((encode0 | encode1) == 0) {
                accept = true;
                break;
            } else if ((encode0 & encode1) > 0) {
                // 起点终点都在边上
                break;
            } else {
                float x = 0, y = 0;
                int encodeOut = encode0 > 0 ? encode0 : encode1;

                float k = (y1 - y0) / (x1 - x0);

                // y = y0 + k(x-x0)
                // x = x0 + 1/k*(y-y0)
                if ((encodeOut & ENCODE_SIDE_TOP)>0) {
                    y = max.y;
                    x = x0 + (y - y0) / k;
                } else if ((encodeOut & ENCODE_SIDE_BOTTOM) > 0) {
                    y = min.y;
                    x = x0 + (y - y0) / k;
                } else if ((encodeOut & ENCODE_SIDE_LEFT) > 0) {
                    x = min.x;
                    y = y0 + (x - x0) * k;
                } else if ((encodeOut&ENCODE_SIDE_RIGHT) > 0) {
                    x = max.x;
                    y = y0 + (x - x0) * k;
                }

                if (encodeOut == encode0) {
                    x0 = x;
                    y0 = y;
                    var new_pos = new Vector3(x, y);
                    encode0 = Encode(ref new_pos, ref min, ref max);
                } else {
                    x1 = x;
                    y1 = y;
                    var new_pos = new Vector3(x, y);
                    encode1 = Encode(ref new_pos, ref min, ref max);
                }
            }
        }
        if (accept) {
            start.x = x0;
            start.y = y0;

            end.x = x1;
            end.y = y1;
        }
        return accept;
    }

    int Encode(ref Vector3 pos, ref Vector2 min, ref Vector2 max) {
        int code = ENCODE_SIDE_INSIDE;

        if (pos.x < min.x)
            code |= ENCODE_SIDE_LEFT;
        else if (pos.x > max.x)
            code |= ENCODE_SIDE_RIGHT;

        if (pos.y < min.y)
            code |= ENCODE_SIDE_BOTTOM;
        else if (pos.y > max.y)
            code |= ENCODE_SIDE_TOP;

        return code;
    }

    //   /\3
    // 1----2
    void fillBottomFlatTriangle(VertexIn v1, VertexIn v2, VertexIn v3, Color color) {

        var p1 = v1.pos;
        var p2 = v2.pos;
        var p3 = v3.pos;

        float slop1 = (p3.y - p1.y) / (p3.x - p1.x);
        float slop2 = (p3.y - p2.y) / (p3.x - p2.x);

        float curx1 = p1.x;
        float curx2 = p2.x;
        int inc = p1.y > p3.y ? -1 : 1;

        int scanline = (int)p1.y;
        int scanto = (int)p3.y;
        if (scanline == scanto) {
            //三角形退化
            var start = new VertexIn(curx1, scanline);
            var end = new VertexIn(curx2, scanline);
            DrawLine(start, end, ref color);
        }
        else {
            for (; scanline != scanto; scanline += inc) {
                var start = new VertexIn(curx1, scanline);
                var end = new VertexIn(curx2, scanline);
                DrawLine(start, end, ref color);

                curx1 += inc/slop1;
                curx2 += inc/slop2;
            }
        }
    }

    public void DrawTriangle2D(VertexIn v1, VertexIn v2, VertexIn v3, Color color) {
        var p1 = v1.pos;
        var p2 = v2.pos;
        var p3 = v3.pos;

        // 保证v1,v2,v3 y轴依次变小
        if (p1.y < p2.y) Swap(ref v1, ref v2);
        if (p1.y < p3.y) Swap(ref v1, ref v3);
        if (p2.y < p3.y) Swap(ref v2, ref v3);

        p1 = v1.pos;
        p2 = v2.pos;
        p3 = v3.pos;

        if (p2.y == p3.y) {
            fillBottomFlatTriangle(v2, v3, v1, color);
        } else if (p1.y == p2.y) {
            fillBottomFlatTriangle(v1, v2, v3, color);
        } else {
            // y = y0 + k(x-x0)
            // x = x0 + 1/k*(y-y0)
            float k = (p1.y - p3.y) / (p1.x - p3.x);
            float splitx = p3.x + (p2.y - p3.y) / k;
            float splity = p3.y + (splitx - p3.x) * k;

            var splitv = new VertexIn(splitx, splity);
            fillBottomFlatTriangle(v2, splitv, v1, color);
            fillBottomFlatTriangle(v2, splitv, v3, color);
        }
    }
    #endregion

    #region draw3d
    public void DrawFrame() {
        var meshs = GameObject.FindObjectsOfType<MeshFilter>();

        foreach (var mesh in meshs) {
            var m = mesh.sharedMesh;
            if (m) {
                ShaderGlobal.l2wMat = mesh.transform.localToWorldMatrix;
                ShaderGlobal.l2wInvTMat = ShaderGlobal.l2wMat.inverse.transpose;
                ShaderGlobal.MVPMat = camera.projectionMatrix * camera.worldToCameraMatrix * ShaderGlobal.l2wMat;
                var vao = new VAO(m);

                var mat = mesh.gameObject.GetComponent<Renderer>().sharedMaterial;
                if (mat != null && mat.mainTexture is Texture2D) {
                    ShaderGlobal.MainTex = (mat.mainTexture as Texture2D).GetPixels();
                    ShaderGlobal.MainTexW = mat.mainTexture.width;
                    ShaderGlobal.MainTexH = mat.mainTexture.height;
                } else {
                    ShaderGlobal.MainTex = null;
                }

                DrawElement(vao);
            }
        }
        texture.SetPixels(colorBuffer);
    }

    void DrawElement(VAO vao) {
        // 做MVP
        RunVertexShader(vao);

        //裁剪掉在view frustrum plane外的三角形、可能会生成新的
        ClipCoord();

        // NDC，viewport transform
        // 改成在RunVertexShader里算，方便做剔除
        //NDCCoord();

        //用indices装配三角形
        SetupTriangles();

        RunFragmentShader();

        vertexList.Clear();
    }

    void RunVertexShader(VAO vao) {
        for (int i = 0; i < vao.vbo.Length; ) {

            var v0 = Vert(vao.vbo[i]);
            var v1 = Vert(vao.vbo[i+1]);
            var v2 = Vert(vao.vbo[i+2]);
            DoNDCCoord(v0);
            DoNDCCoord(v1);
            DoNDCCoord(v2);

            var d01 = v1.vertex - v0.vertex;
            var d02 = v2.vertex - v0.vertex;
            // 顺便 背面剔除 backface culling
            var dimen = d01.x * d02.y - d01.y * d02.x;

            if (dimen < 0) {
                vertexList.Add(v0);
                vertexList.Add(v1);
                vertexList.Add(v2);
            }
            i += 3;
        }
    }

    void ClipCoord() {

        // TODO 三角形裁剪
    }

    void DoNDCCoord(FragmentIn frag) {
        var projectv = frag.vertex;
        // 转换到NDC空间，再到viewport空间
        projectv.x = (projectv.x / projectv.w / 2 + 0.5f) * width;
        projectv.y = (projectv.y / projectv.w / 2 + 0.5f) * height;
        projectv.z = (projectv.z / projectv.w + 0.5f);

        frag.vertex = projectv;

        frag.pixelx = (int)projectv.x;
        frag.pixely = (int)projectv.y;

        // 透视除法矫正,见Rasterizer步骤
        frag.uv.x /= projectv.w;
        frag.uv.y /= projectv.w;
    }

    void NDCCoord() {
        foreach (var vert in vertexList) {
            DoNDCCoord(vert);
        }
    }

    void SetupTriangles() {
        // 在输入数据里已经处理，相当于Vert阶段多处理一些顶点，先不管
    }

    void RunFragmentShader() {
        VertexCount += vertexList.Count;

        for (int i =0; i<vertexList.Count;) {

#if DRAW_WIREFRAME
            var fragcount = RastWireFrame(vertexList[i], vertexList[i + 1], vertexList[i + 2]);

            FragmentCount += fragcount;
#elif USE_PARALLER
            var fragcount = ParallRast(vertexList[i], vertexList[i + 1], vertexList[i + 2]);

            FragmentCount += fragcount;
#else
            var fragcount = Rast(vertexList[i], vertexList[i + 1], vertexList[i + 2]);

            FragmentCount += fragcount;
#endif
            i += 3;
        }
    }
#endregion
}
