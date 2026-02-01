using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public float duration = 2f;
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        StartCoroutine(Fade());
    }

    System.Collections.IEnumerator Fade()
    {
        float t = 0;
        Color c = img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = 1 - (t / duration);
            img.color = c;
            yield return null;
        }

        c.a = 0;
        img.color = c;

        gameObject.SetActive(false);

    }
}
