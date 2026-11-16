using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreIncrementer : MonoBehaviour
{
    private static int score;
    private static TextMeshProUGUI scoreText;
    private static TextMeshProUGUI respawnText;

    void Awake()
    {
        scoreText = GameObject.Find("scoreCounter").GetComponent<TextMeshProUGUI>();
        respawnText = GameObject.Find("respawnText").GetComponent<TextMeshProUGUI>();
        respawnText.gameObject.SetActive(false);
    }

    void OnTriggerExit(Collider other)
    {   
        if (other.name != "BatMesh") return;
        score++;
        scoreText.text = $"Score: {score}";
    }
    

    public static void Die()
    {
        respawnText.gameObject.SetActive(true);
        respawnText.text = $"Final Score: {score}\nPress 'R' to respawn!";
        scoreText.gameObject.SetActive(false);
    }

    public static void Respawn()
    {
        score = 0;
        respawnText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(true);
        scoreText.text = $"Score: {score}";
    }
}
