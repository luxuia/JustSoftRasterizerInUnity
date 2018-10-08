using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawMesh : MonoBehaviour {

    public Text DebugInfo;

    SoftRender render;

    void Awake() {
        var image = GetComponent<RawImage>();
        var texture = new Texture2D((int)image.rectTransform.sizeDelta.x, (int)image.rectTransform.sizeDelta.y);
        image.texture = texture;

        render = new SoftRender(texture, Camera.main);
    }
    
    void Update() {
        render.Reset(render.texture, Camera.main);

        render.DrawFrame();

        DebugInfo.text = string.Format(@"vertex count {0}
fragment count {1}
early-z  count {2}
final-write count {3}", render.VertexCount, render.FragmentCount, render.EarlyZCount, render.FinalWriteCount);

        render.texture.Apply();
    }
}
