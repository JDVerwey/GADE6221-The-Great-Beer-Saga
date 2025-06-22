using UnityEngine;
using TMPro;

// This script goes on your Leaderboard Entry Prefab.
public class LeaderboardResult : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text scoreText;

    public void SetData(int rank, string name, int score)
    {
        this.rankText.text = $"{rank}.";
        this.nameText.text = name;
        this.scoreText.text = score.ToString();
    }
}