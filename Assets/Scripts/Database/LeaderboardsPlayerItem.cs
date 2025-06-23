using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Leaderboards.Models;
using TMPro;

public class LeaderboardsPlayerItem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text scoreText;
    private LeaderboardEntry _player;

    public void Initialize(LeaderboardEntry player)
    {
        _player = player;
        rankText.text = (player.Rank + 1).ToString(); // Rank is 0-based
        nameText.text = player.PlayerName;
        scoreText.text = player.Score.ToString();
    }

    void Start()
    {
    }

}
