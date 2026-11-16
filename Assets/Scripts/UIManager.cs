using System.Collections;
using UnityEngine;
using TMPro;
using UnityEditor;
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI respawnText;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.2f;
    [SerializeField] private GameObject pauseScreen;

    private Coroutine pulseRoutine;
    private Vector3 scoreBaseScale = Vector3.one;

    void Awake()
    {
        if (scoreText != null) scoreBaseScale = scoreText.rectTransform.localScale;
    }

    public void UpdateRespawnText(bool enabled, string input)
    {
        if (respawnText == null) return;
        respawnText.enabled = enabled;
        respawnText.text = input;
    }

    public void UpdateScoreText(bool enabled, string input)
    {
        if (scoreText == null) return;
        scoreText.enabled = enabled;
        scoreText.text = input;
    }

    public void Pause(bool pause)
    {
        pauseScreen.SetActive(pause);
        Time.timeScale = pause ? 0f : 1f;
    }

    public void PulseScore()
    {
        if (scoreText == null) return;
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        RectTransform rt = scoreText.rectTransform;
        Vector3 peak = new Vector3(scoreBaseScale.x * pulseScale, scoreBaseScale.y * pulseScale, scoreBaseScale.z);
        float half = pulseDuration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            rt.localScale = Vector3.Lerp(scoreBaseScale, peak, t / half);
            yield return null;
        }
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            rt.localScale = Vector3.Lerp(peak, scoreBaseScale, t / half);
            yield return null;
        }

        rt.localScale = scoreBaseScale;
        pulseRoutine = null;
    }
}
