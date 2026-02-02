using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager za glavni izbornik igre
/// VAŽNO: Ova skripta je SAMO za Main Menu scenu!
/// </summary>
public class MainMenuManager : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Index scene za gameplay (obično 1)")]
	public int gameSceneIndex = 1;

	void Start()
	{
		// ⭐ POJAČANA PROVJERA: U glavnom izborniku UVIJEK otključaj kursor
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		Debug.Log("MainMenuManager: Glavni izbornik spreman - kursor otključan.");
	}

	// ⭐ DODANO: Dodatna sigurnost u Update-u
	void Update()
	{
		// Osiguraj da kursor ostane vidljiv u MainMenu-u
		if (Cursor.lockState != CursorLockMode.None)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Debug.LogWarning("MainMenuManager: Kursor bio zaključan - ispravljam!");
		}
	}

	public void PlayGame()
	{
		Debug.Log("MainMenuManager: Učitavam igru...");

		// Provjera da li scena postoji u Build Settings
		if (gameSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(gameSceneIndex);
		}
		else
		{
			Debug.LogError($"MainMenuManager: Scena index {gameSceneIndex} ne postoji! Dodaj scene u File > Build Settings.");
		}
	}

	public void QuitGame()
	{
		Debug.Log("MainMenuManager: Zatvaranje igre...");

#if UNITY_EDITOR
		// U editoru samo zaustavi playmode
		UnityEditor.EditorApplication.isPlaying = false;
#else
			// U buildu zatvori aplikaciju
			Application.Quit();
#endif
	}

	// NOVI DODATAK: Učitaj specifičnu scenu (za level select menu)
	public void LoadScene(int sceneIndex)
	{
		if (sceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			Debug.Log($"MainMenuManager: Učitavam scenu index {sceneIndex}");
			SceneManager.LoadScene(sceneIndex);
		}
		else
		{
			Debug.LogError($"MainMenuManager: Scena {sceneIndex} ne postoji!");
		}
	}

	// NOVI DODATAK: Restart trenutne scene (za Restart gumb u main menu)
	public void RestartCurrentScene()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		Debug.Log($"MainMenuManager: Restarting {currentScene.name}");
		SceneManager.LoadScene(currentScene.name);
	}
}