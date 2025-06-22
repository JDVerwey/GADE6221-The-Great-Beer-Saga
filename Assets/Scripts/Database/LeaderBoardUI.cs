// File: LeaderboardUI.cs
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Leaderboards;
using System.Collections.Generic;
using System.Threading.Tasks;

// This script manages fetching and displaying the leaderboard data.
public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel; // The entire panel, to be shown/hidden
    public Button closeButton;
    public Button openButton; // Optional: a button in your main menu to open this panel

    [Header("Leaderboard Content")]
    public GameObject entryPrefab; 
    public Transform contentContainer; 
    public GameObject loadingText; 

    void Start()
    {
        // Deactivate the panel by default
        leaderboardPanel.SetActive(false);
        loadingText.SetActive(false);

        // Wire up buttons
        if (closeButton) closeButton.onClick.AddListener(HideLeaderboard);
        if (openButton) openButton.onClick.AddListener(ShowLeaderboard);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        FetchAndDisplayScores();
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public async void FetchAndDisplayScores()
    {
        // Clear any old results
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        loadingText.SetActive(true);

        try
        {
            // The ID here MUST match the ID created in the dashboard
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync("Most_Drunken_Viking");

            loadingText.SetActive(false);

            // Loop through the leaderboard entries and instantiate the prefab
            foreach (var entry in scoresResponse.Results)
            {
                GameObject newEntry = Instantiate(entryPrefab, contentContainer);
                LeaderboardResult resultRow = newEntry.GetComponent<LeaderboardResult>();

                // The Rank is 0-based, so add 1 for display
                resultRow.SetData(entry.Rank + 1, entry.PlayerName, (int)entry.Score);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to fetch leaderboard scores: {e}");
            loadingText.GetComponent<TMPro.TMP_Text>().text = "Error loading scores.";
        }
    }
}