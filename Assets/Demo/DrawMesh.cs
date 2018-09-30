using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawMesh : MonoBehaviour {

    public Text DebugInfo;
    
    void Update() {
        var image = GetComponent<RawImage>();
        var texture = new Texture2D((int)image.rectTransform.sizeDelta.x, (int)image.rectTransform.sizeDelta.y);
        image.texture = texture;

        var raster = new SoftRender(texture, Camera.main);

        raster.DrawFrame();

        DebugInfo.text = string.Format(@"vertex count {0}
fragment count {1}
early-z  count {2}
final-write count {3}", raster.VertexCount, raster.FragmentCount, raster.EarlyZCount, raster.FinalWriteCount);

        texture.Apply();
    }
}
