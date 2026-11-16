using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI respawnText;

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
}
