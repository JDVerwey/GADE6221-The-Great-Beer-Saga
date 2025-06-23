using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Leaderboards;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class LeaderboardsMenu : MonoBehaviour
{
    [Header("Leaderboard Info")]
    public string leaderboardID = "Most_Drunken_Viking"; // The ID from the Unity Dashboard
    public int playersPerPage = 10;

    [Header("UI References")]
    public GameObject leaderboardPanel;
    public Button closeButton;
    public Button openButton;

    [Header("Leaderboard Content")]
    public GameObject playerItemPrefab;
    public Transform playersContainer;
    public GameObject loadingText;

    private int _currentPage = 1;
    private int _totalPages = 0;

    void Start()
    {
        leaderboardPanel.SetActive(false);
        loadingText.SetActive(false);

        if (openButton) openButton.onClick.AddListener(Open);
        if (closeButton) closeButton.onClick.AddListener(ClosePanel);
    }

    public void Open()
    {
        leaderboardPanel.SetActive(true);
        ClearList();
        _currentPage = 1;
        _totalPages = 0;
        LoadPlayers(_currentPage);
    }

    public void ClosePanel()
    {
        leaderboardPanel.SetActive(false);
    }

    private async void LoadPlayers(int page)
    {
        loadingText.SetActive(true);

        try
        {
            var options = new GetScoresOptions { Offset = (page - 1) * playersPerPage, Limit = playersPerPage };
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID, options);

            ClearList();

            foreach (var score in scoresResponse.Results)
            {
                GameObject playerItem = Instantiate(playerItemPrefab, playersContainer);
                playerItem.GetComponent<LeaderboardsPlayerItem>().Initialize(score);
            }

            _totalPages = (scoresResponse.Total + playersPerPage - 1) / playersPerPage;
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load leaderboard scores: {e}");
            loadingText.GetComponent<TMP_Text>().text = "Error loading scores.";
        }
        finally
        {
            loadingText.SetActive(false);
        }
    }
    
    

    private void ClearList()
    {
        foreach (Transform child in playersContainer)
        {
            Destroy(child.gameObject);
        }
    }
}