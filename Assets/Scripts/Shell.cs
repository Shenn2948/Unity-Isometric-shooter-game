using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public float ForceMin;
    public float ForceMax;

    private float lifetime = 4;
    private float fadetime = 2;

    void Start()
    {
        float force = Random.Range(ForceMin, ForceMax);
        Rigidbody.AddForce(transform.right * force);
        Rigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    void Update()
    {
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;

        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }

        Destroy(gameObject);
    }
}