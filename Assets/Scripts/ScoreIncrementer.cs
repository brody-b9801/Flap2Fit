using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreIncrementer : MonoBehaviour
{
    private static int score;
    public static int getScore() => score;
    private static UIManager uiManager;


    void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        uiManager.UpdateRespawnText(false, "");
    }

    void OnTriggerExit(Collider other)
    {   
        if (other.name != "BatMesh") return;
        score++;
        uiManager.UpdateScoreText(true, $"Score: {score}");
        uiManager.PulseScore();
    }
    

    public static void Die()
    {
        uiManager.UpdateRespawnText(true, $"Final Score: {score}\nPress 'R' to respawn!");
        uiManager.UpdateScoreText(false, $"Score: 0");
    }

    public static void Respawn()
    {
        score = 0;
        uiManager.UpdateRespawnText(false, "");
        uiManager.UpdateScoreText(true, $"Score: {score}");
    }
}
