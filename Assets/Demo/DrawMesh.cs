using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawMesh : MonoBehaviour {

    public int TriggerCount = 10;

    void Update() {
        var image = GetComponent<RawImage>();
        var texture = new Texture2D((int)image.rectTransform.sizeDelta.x, (int)image.rectTransform.sizeDelta.y);
        image.texture = texture;

        var raster = new SoftRender(texture, Camera.main);

        raster.DrawFrame();

        texture.Apply();
    }
}
