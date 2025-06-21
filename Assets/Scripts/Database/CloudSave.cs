using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks; // Required for Task
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System; // Required for Exception

// A simple class to hold our player data. This is cleaner than using dictionaries everywhere.
public class PlayerData
{
    public string PlayerName;
    public int HighestScore;
}

public class CloudSave : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static CloudSave Instance { get; private set; }

    private void Awake()
    {
        // If an Instance already exists, destroy this new one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // This is the first instance, so make it the singleton.
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads.
    }
    // --- End Singleton Pattern ---

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // It's good practice to initialize and sign in as soon as the service is ready.
        await SetupAndSignIn();
    }

    private async Task SetupAndSignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in successfully as: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to initialize or sign in: " + e.Message);
        }
    }

    // Public method to save data. It now returns a Task so the caller can await it.
    public async Task SaveData(string playerName, int highestScore)
    {
        // Using a dictionary to structure the data for saving.
        var data = new Dictionary<string, object>
        {
            {"PlayerName", playerName},
            {"HighestScore", highestScore}
        };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"Data saved successfully: PlayerName={playerName}, HighestScore={highestScore}");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save data: " + e.Message);
        }
    }

    // Public method to load data. It returns a Task<PlayerData> to give the loaded data back to the caller.
    public async Task<PlayerData> LoadData()
    {
        try
        {
            // Define the keys we want to load from the cloud.
            var fieldsToLoad = new HashSet<string>
            {
                "PlayerName",
                "HighestScore"
            };

            // Load the data from the cloud.
            var loadedData = await CloudSaveService.Instance.Data.Player.LoadAsync(fieldsToLoad);

            PlayerData playerData = new PlayerData();

            // Safely extract the data using GetAs<T>() for type safety.
            if (loadedData.TryGetValue("PlayerName", out var playerNameValue))
            {
                playerData.PlayerName = playerNameValue.Value.GetAs<string>();
            }

            if (loadedData.TryGetValue("HighestScore", out var highestScoreValue))
            {
                playerData.HighestScore = highestScoreValue.Value.GetAs<int>();
            }

            Debug.Log($"Data loaded: PlayerName={playerData.PlayerName}, HighestScore={playerData.HighestScore}");
            return playerData;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load data: " + e.Message);
            return null; // Return null or a default PlayerData object on failure.
        }
    }
}