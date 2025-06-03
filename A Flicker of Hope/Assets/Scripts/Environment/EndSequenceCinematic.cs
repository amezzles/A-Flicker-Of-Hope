using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndSequenceCinematic : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera cinematicCamera;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    public void StartCinematicSequence()
    {
        StartCoroutine(FadeToBlackThenCinematic());
    }

    private IEnumerator FadeToBlackThenCinematic()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1f);

        //switch cameras
        playerCamera.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        //fade back in
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 0f);
    }
}
