using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

//https://docs.unity3d.com/Packages/com.unity.ui@1.0/api/UnityEngine.UIElements.MeshGenerationContext.html
//https://discussions.unity.com/t/introducing-the-vector-api-in-unity-2022-1/864911
public class NodeConnectionElement: VisualElement
{
    public Vector2 startPos;
    public Vector2 endPos;

    public NodeConnectionElement(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        style.position = Position.Absolute;
        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Painter2D paint2D = mgc.painter2D;

        paint2D.strokeColor = Color.white;
        paint2D.lineWidth = 10f;
        paint2D.lineCap = LineCap.Round;
        paint2D.BeginPath();
        paint2D.MoveTo(startPos);
        paint2D.LineTo(endPos);
        paint2D.ClosePath();
        paint2D.Stroke();
    }

    public void UpdateLine(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.MarkDirtyRepaint();
    }
}
