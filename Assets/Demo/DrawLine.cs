using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawLine : MonoBehaviour {



	// Use this for initialization
	void Start () {
        var image = GetComponent<RawImage>();
        var texture = new Texture2D((int)image.rectTransform.sizeDelta.x, (int)image.rectTransform.sizeDelta.y);
        image.texture = texture;

        var raster = new SoftRender(texture, null);

        float width = texture.width;
        float height = texture.height;

        for (int i =0; i < 100; ++i) {
            var start = new SoftRender.VertexIn(Random.Range(-width, width * 2), Random.Range(-height, height * 2));
            var end = new SoftRender.VertexIn(Random.Range(-width, width * 2), Random.Range(-height, height * 2));
            var color = new Color(Random.value, Random.value, Random.value);
            raster.DrawLine(start, end, ref color);
        }
        texture.Apply();
	}

}
