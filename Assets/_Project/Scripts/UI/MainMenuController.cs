using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class MainMenuController : MonoBehaviour
{
    // Assign scene names in the Inspector or hardcode them
    public string gameBoardSceneName = "GameBoard"; // Make sure this matches your scene name
    public string deckBuilderSceneName = "DeckBuilder"; // Placeholder
    public string shopSceneName = "Shop"; // Placeholder

    public void PlayGame()
    {
        Debug.Log("Play button clicked - Loading Game Board...");
        // Use a SceneLoader if you create one, otherwise use SceneManager directly
        SceneManager.LoadScene(gameBoardSceneName);
    }

    public void OpenDeckBuilder()
    {
        Debug.Log("Deck button clicked - Loading Deck Builder...");
        // SceneManager.LoadScene(deckBuilderSceneName); // Uncomment when DeckBuilder scene exists
    }

    public void PlaySolo() // Assuming "Solo" is another game mode or placeholder
    {
        Debug.Log("Solo button clicked - Functionality TBD");
        // Potentially load the game board with AI or specific settings
        // SceneManager.LoadScene(gameBoardSceneName);
    }

    public void OpenShop()
    {
        Debug.Log("Shop button clicked - Loading Shop...");
        // SceneManager.LoadScene(shopSceneName); // Uncomment when Shop scene exists
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked");
#if UNITY_EDITOR
        // If running in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running in a built application
        Application.Quit();
#endif
    }
} 