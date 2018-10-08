using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public partial class SoftRender {

    public FragmentIn GLOBAL_PARALL_V1, GLOBAL_PARALL_V2, GLOBAL_PARALL_V3;

    public List<FragmentIn> Rast(FragmentIn v1, FragmentIn v2, FragmentIn v3) {
        var p1 = v1.vertex;
        var p2 = v2.vertex;
        var p3 = v3.vertex;

        int xMin = (int)Mathf.Min(p1.x, p2.x, p3.x);
        int xMax = (int)Mathf.Max(p1.x, p2.x, p3.x);
        int yMin = (int)Mathf.Min(p1.y, p2.y, p3.y);
        int yMax = (int)Mathf.Max(p1.y, p2.y, p3.y);
        var fragList = new List<FragmentIn>((xMax - xMin) * (yMax - yMin));
        for (int m = xMin; m < xMax + 1; m++) {
            for (int n = yMin; n < yMax + 1; n++) {
                if (m < 0 || m > width - 1 || n < 0 || n > height - 1) continue;
                if (!isLeftPoint(p1, p2, m + 0.5f, n + 0.5f)) continue;
                if (!isLeftPoint(p2, p3, m + 0.5f, n + 0.5f)) continue;
                if (!isLeftPoint(p3, p1, m + 0.5f, n + 0.5f)) continue;
                var frag = new FragmentIn();
                frag.pixelx = m;
                frag.pixely = n;
                LerpFragment(v1, v2, v3, frag);
                fragList.Add(frag);
            }
        }
        return fragList;
    }

    public int ParallRast(FragmentIn v1, FragmentIn v2, FragmentIn v3) {
        var p1 = v1.vertex;
        var p2 = v2.vertex;
        var p3 = v3.vertex;

        GLOBAL_PARALL_V1 = v1;
        GLOBAL_PARALL_V2 = v2;
        GLOBAL_PARALL_V3 = v3;

        int xMin = (int)Mathf.Min(p1.x, p2.x, p3.x);
        int xMax = (int)Mathf.Max(p1.x, p2.x, p3.x);
        int yMin = (int)Mathf.Min(p1.y, p2.y, p3.y);
        int yMax = (int)Mathf.Max(p1.y, p2.y, p3.y);

        var fragCount = 0;

        int threadCount = (xMax-xMin+1) *(yMax-yMin+1);
        
        var finishEvent = new ManualResetEvent(false);

        ThreadPool.SetMaxThreads(20, 20);
        ThreadPool.SetMinThreads(5, 5);

        WaitCallback cb = (arg) => {
            var fraglist = arg as List<FragmentIn>;
            for (int i = 0; i < fraglist.Count; ++i) {
                ThreadLerpFragment(fraglist[i]);
            }
            if (Interlocked.Add(ref threadCount, -fraglist.Count) == 0) {
                finishEvent.Set();
            };
        };

        List<FragmentIn> frags = new List<FragmentIn>(50);
        for (int m = xMin; m < xMax + 1; m++) {
            for (int n = yMin; n < yMax + 1; n++) {
                if ((m < 0 || m > width - 1 || n < 0 || n > height - 1) ||
                    (!isLeftPoint(p1, p2, m + 0.5f, n + 0.5f)) ||
                    (!isLeftPoint(p2, p3, m + 0.5f, n + 0.5f)) ||
                    (!isLeftPoint(p3, p1, m + 0.5f, n + 0.5f))) {

                    if (Interlocked.Decrement(ref threadCount) == 0) {
                        finishEvent.Set();
                    }
                    continue;
                }
                var frag = new FragmentIn();

                frag.pixelx = m;
                frag.pixely = n;

                frags.Add(frag);

                // 最好情况应该是一个线程一个work，让每个线程的工作多一些。
                // 一个每个frag都单独创建线程，创建的开销太大
                if (frags.Count > 100) {
                    ThreadPool.QueueUserWorkItem(cb, frags);

                    frags = new List<FragmentIn>();
                }
                
                fragCount++;
            }
        }
        if (frags.Count > 0) {
            ThreadPool.QueueUserWorkItem(cb, frags);
        }

        finishEvent.WaitOne();

        return fragCount;
    }

    public bool isLeftPoint(Vector4 a, Vector4 b, float x, float y) {
        float s = (a.x - x) * (b.y - y) - (a.y - y) * (b.x - x);
        return s > 0 ? false : true;
    }


    // 基于斜率的办法避免不了精度误差
    /*
    List<FragmentIn> Rast(FragmentIn v1, FragmentIn v2, FragmentIn v3) {
        var frags = new List<FragmentIn>();

        var vv1 = v1;
        var vv2 = v2;
        var vv3 = v3;

        // 保证v1,v2,v3 y轴依次变小
        if (v1.vertex.y < v2.vertex.y) Swap(ref v1, ref v2);
        if (v1.vertex.y < v3.vertex.y) Swap(ref v1, ref v3);
        if (v2.vertex.y < v3.vertex.y) Swap(ref v2, ref v3);

        var p1 = v1.vertex;
        var p2 = v2.vertex;
        var p3 = v3.vertex;

        if (p2.y == p3.y) {
            RastTriangle(v2, v3, v1, frags);
        }
        else if (p1.y == p2.y) {
            RastTriangle(v1, v2, v3, frags);
        }
        else {
            // y = y0 + k(x-x0)
            // x = x0 + 1/k*(y-y0)
            // 注意斜率为0或无穷大的情况
            float k = (p1.y - p3.y) / (p1.x - p3.x);
            float splitx;
            float splity;

            if (p1.x != p3.x) {
                splitx = p3.x + (p2.y - p3.y) / k;
                splity = p2.y;
            } else {
                splitx = p3.x;
                splity = p2.y;
            }

            var splitv = new FragmentIn();
            splitv.pixelx = (int)splitx;
            splitv.pixely = (int)splity;

            LerpFragment(v1, v2, v3, splitv);
            
            RastTriangle(v2, splitv, v1, frags);
            RastTriangle(v2, splitv, v3, frags);
        }

        return frags;
    }

    // 扫描线
    //   /\3
    // 1----2
    void RastTriangle(FragmentIn v1, FragmentIn v2, FragmentIn v3, List<FragmentIn> frags) {

        var p1 = v1.vertex;
        var p2 = v2.vertex;
        var p3 = v3.vertex;


        float curx1 = p1.x;
        float curx2 = p2.x;
        int inc = p1.y > p3.y ? -1 : 1;

        int scanline = (int)p1.y;
        int scanto = (int)p3.y;
        if (scanline == scanto) {
            //三角形退化

            curx1 = Mathf.Min(Mathf.Max(curx1, 0), width);
            curx2 = Mathf.Min(Mathf.Max(curx2, 0), width);

            var ix1 = (int)curx1;
            var ix2 = (int)curx2;
            if (ix1 == ix2) {
                var frag = new FragmentIn();
                frag.pixelx = ix1;
                frag.pixely = scanline;

                LerpFragment(v1, v2, v3, frag);

                frags.Add(frag);
            }
            else {
                var incx = ix1 > ix2 ? -1 : 1;
                for (int x = ix1; x != ix2; x += incx) {
                    var frag = new FragmentIn();
                    frag.pixelx = x;
                    frag.pixely = scanline;

                    LerpFragment(v1, v2, v3, frag);

                    frags.Add(frag);
                }
            }
        }
        else {
            float slop1 = (p3.x - p1.x)/(p3.y - p1.y);
            float slop2 = (p3.x - p2.x)/(p3.y - p2.y);

            for (; scanline != scanto; scanline += inc) {

                curx1 = Mathf.Min(Mathf.Max(curx1, 0), width);
                curx2 = Mathf.Min(Mathf.Max(curx2, 0), width);

                var ix1 = (int)curx1;
                var ix2 = (int)curx2;
                if (ix1 == ix2) {
                    var frag = new FragmentIn();
                    frag.pixelx = ix1;
                    frag.pixely = scanline;

                    LerpFragment(v1, v2, v3, frag);

                    frags.Add(frag);
                } else {
                    var incx = ix1 > ix2 ? -1 : 1;

                    for (int x = ix1; x != ix2; x += incx) {

                        var frag = new FragmentIn();
                        frag.pixelx = x;
                        frag.pixely = scanline;

                        LerpFragment(v1, v2, v3, frag);

                        frags.Add(frag);
                    }
                }

                curx1 += inc * slop1;
                curx2 += inc * slop2;
            }
        }
    }
    */

    void RealDoFragment(FragmentIn frag) {
        var old_depth = depthBuffer[frag.pixelx, frag.pixely];
        // do early z
        if (frag.vertex.z > old_depth && old_depth > 0) {
            EarlyZCount++;
        };

        Color color = Frag(frag);

        DrawPixel(frag.pixelx, frag.pixely, ref color);
        depthBuffer[frag.pixelx, frag.pixely] = frag.vertex.z;
    }

    // 基于三角形面积做插值
    bool LerpFragment(FragmentIn v1, FragmentIn v2, FragmentIn v3, FragmentIn ret) {
        var v0 = new Vector4(ret.pixelx, ret.pixely);
        var d01 = v0 - v1.vertex;

        var d21 = v2.vertex - v1.vertex;
        var d31 = v3.vertex - v1.vertex;

        var denom = Mathf.Abs(d21.x * d31.y - d21.y * d31.x);

        if (denom < 0.000001) {
            return false;
        }

        var u = Mathf.Abs(d01.x * d31.y - d01.y * d31.x) / denom; // close to v2
        var v = Mathf.Abs(d01.x * d21.y - d01.y * d21.x) / denom; // close to v3
        var w = 1 - u - v; // close to v1

        // do lerp
        ret.vertex = v2.vertex * u + v3.vertex * v + v1.vertex * w;
        ret.normal = v2.normal * u + v3.normal * v + v1.normal * w;
        ret.worldPos = v2.worldPos * u + v3.worldPos * v + v1.worldPos * w;
        ret.uv = v2.uv * u + v3.uv * v + v1.uv * w;
        ret.color = v2.color * u + v3.color * v + v1.color * w;

        return true;
    }

    // 基于三角形面积做插值
    void ThreadLerpFragment(FragmentIn frag) {
        var v1 = GLOBAL_PARALL_V1;
        var v2 = GLOBAL_PARALL_V2;
        var v3 = GLOBAL_PARALL_V3;
        var v0 = new Vector4(frag.pixelx, frag.pixely);
        var d01 = v0 - v1.vertex;

        var d21 = v2.vertex - v1.vertex;
        var d31 = v3.vertex - v1.vertex;

        var denom = Mathf.Abs(d21.x * d31.y - d21.y * d31.x);
        
        if (denom < 0.000001) {
            return;
        }

        var u = Mathf.Abs(d01.x * d31.y - d01.y * d31.x) / denom; // close to v2
        var v = Mathf.Abs(d01.x * d21.y - d01.y * d21.x) / denom; // close to v3
        var w = 1 - u - v; // close to v1

        // do lerp
        frag.vertex = v2.vertex * u + v3.vertex * v + v1.vertex * w;
        frag.normal = v2.normal * u + v3.normal * v + v1.normal * w;
        frag.worldPos = v2.worldPos * u + v3.worldPos * v + v1.worldPos * w;
        frag.uv = v2.uv * u + v3.uv * v + v1.uv * w;
        frag.color = v2.color * u + v3.color * v + v1.color * w;
        
        RealDoFragment(frag);

        return;
    }
}
