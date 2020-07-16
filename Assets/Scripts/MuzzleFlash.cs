using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject FlashHolder;
    public float FlashTime;
    public Sprite[] FlashSprites;
    public SpriteRenderer[] SpriteRenderers;

    void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        FlashHolder.SetActive(true);

        int flashSpriteIndex = Random.Range(0, FlashSprites.Length);
        foreach (SpriteRenderer t in SpriteRenderers)
        {
            t.sprite = FlashSprites[flashSpriteIndex];
        }

        Invoke("Deactivate", FlashTime);
    }

    private void Deactivate()
    {
        FlashHolder.SetActive(false);
    }
}