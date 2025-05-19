using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text

// Updates interface elements according to game state (health, coins, current phase).
public class UIManager : MonoBehaviour
{
    // References to UI elements (Assign in Inspector)
    public Text player1HealthText; // Example for displaying Stage deck count as health
    public Text player2HealthText;
    public Text currentPhaseText;
    public GameObject player1CoinIndicator;
    public GameObject player2CoinIndicator;

    // Singleton pattern instance (optional)
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Debug.Log("UIManager Awake");
    }

    void Start()
    {
        // Initial UI setup
        UpdatePhaseText("Pre-Game");
        // Set initial health display (needs access to DeckManagers or GameManager)
        // UpdateHealthUI(0, 5); // Example: Player 1 starts with 5 stage cards
        // UpdateHealthUI(1, 5);
        // Set initial coin indicator (needs info from GameManager)
        // UpdateCoinIndicator(0); // Example: Player 1 starts with initiative (1 coin)
    }

    public void UpdateHealthUI(int playerIndex, int remainingStages)
    {
        int health = remainingStages * 5; // Calculate health based on stages
        if (playerIndex == 0 && player1HealthText != null)
        {
            player1HealthText.text = $"Health: {health} ({remainingStages} Stages)";
        }
        else if (playerIndex == 1 && player2HealthText != null)
        {
            player2HealthText.text = $"Health: {health} ({remainingStages} Stages)";
        }
    }

    public void UpdatePhaseText(string phaseName)
    {
        if (currentPhaseText != null)
        {
            currentPhaseText.text = "Phase: " + phaseName;
        }
    }

    public void UpdateCoinIndicator(int playerWithInitiative) // 0 or 1
    {
        if (player1CoinIndicator != null && player2CoinIndicator != null)
        {
            player1CoinIndicator.SetActive(playerWithInitiative == 0); // Show P1 coin if they have initiative
            player2CoinIndicator.SetActive(playerWithInitiative == 1); // Show P2 coin if they have initiative
            // You might show 1 coin for P1 and 2 coins for P2 visually instead
        }
    }

    // TODO: Add methods to update other UI elements (player resources, turn timer, etc.)
} 