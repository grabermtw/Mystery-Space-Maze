using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    Vector3 initPos;
    public float freq;
    public float amp;
    public float flightSpeed;
    private bool floating;

    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
        StartCoroutine(Floating());
    }

    public void Fly()
    {
        floating = false;
        StartCoroutine(FlyAway());
    }

    private IEnumerator FlyAway()
    {
        while(transform.position.y < 50)
        {
            transform.position += new Vector3(0, flightSpeed * Time.deltaTime, 0);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator Floating()
    {
        floating = true;
        while(floating)
        {
            transform.position = initPos + new Vector3(0, Mathf.Sin(Time.fixedTime * Mathf.PI * freq) * amp, 0);
            yield return null;
        }
    }
}
