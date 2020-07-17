using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask TargetMask;
    public SpriteRenderer Dot;
    public Color DotHighlightColor;
    private Color originalColor;

    void Start()
    {
        Cursor.visible = false;
        originalColor = Dot.color;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);
    }

    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, TargetMask))
        {
            Dot.color = DotHighlightColor;
        }
        else
        {
            Dot.color = originalColor;
        }
    }
}