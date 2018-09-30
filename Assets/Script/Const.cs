using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class SoftRender {

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

    public class VAO {
        public VertexIn[] vbo;

        public VAO(Mesh mesh) {
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;

            vbo = new VertexIn[triangles.Length];

            Color color = Color.white;
           
            for (int i = 0; i < triangles.Length/3; ++i) {

                for (int j = 0; j < 3; ++j) {
                    var v = new VertexIn();

                    var idx = triangles[i*3+j];

                    v.pos = vertices[idx];
                    v.normal = normals[idx];
                    v.uv = uvs[idx];
                    v.color = color;

                    vbo[i*3+j] = v;
                }
            }
        }

        public VAO(ref Vector3[] vertices, ref Color[] colors) {
            vbo = new VertexIn[vertices.Length];

            for (int i =0;i<vertices.Length;++i) {
                var v = new VertexIn();
                v.pos = vertices[i];
                v.color = colors[i];

                vbo[i] = v;
            }
        }
    }


    public class VertexIn {
        public Vector3 pos;
        public Color color;
        public Vector2 uv;
        public Vector3 normal;

        public Vector4 viewportData;

        public VertexIn() { }

        public VertexIn(float x, float y) {
            pos = new Vector3(x, y);
            color = Color.white;
            uv = new Vector2();
            normal = Vector3.up;
        }
    }

    public class FragmentIn {
        public Vector3 worldPos;
        public Vector3 normal;
        public Vector2 uv;
        public Color color;

        // viewport pos
        // x,y | z depth, | w = 1/z
        public Vector4 vertex;

        //正常shader里没有这个，为了方便操作
        public int pixelx;
        public int pixely;
    }
}
