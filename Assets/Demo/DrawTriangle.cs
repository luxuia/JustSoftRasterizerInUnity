using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawTriangle : MonoBehaviour {

    public int TriggerCount = 10;

    void Start() {
        var image = GetComponent<RawImage>();
        var texture = new Texture2D((int)image.rectTransform.sizeDelta.x, (int)image.rectTransform.sizeDelta.y);
        image.texture = texture;

        var raster = new SoftRender(texture, null);

        float width = texture.width;
        float height = texture.height;

        for (int i = 0; i < TriggerCount; ++i) {
            var v1 = new SoftRender.VertexIn(Random.Range(0, width ), Random.Range(0, height ));
            var v2 = new SoftRender.VertexIn(Random.Range(0, width ), Random.Range(0, height ));
            var v3 = new SoftRender.VertexIn(Random.Range(0, width), Random.Range(0, height));
            //var v1 = new Vertex(689, 235);
            //var v2 = new Vertex(284, 213);
            //var v3 = new Vertex(371, 363);
            Debug.Log(v1.pos.ToString() + v2.pos.ToString() + v3.pos.ToString());
            var color = new Color(Random.value, Random.value, Random.value);
            raster.DrawTriangle2D(v1, v2, v3, color);
        }
        texture.Apply();
    }
}
