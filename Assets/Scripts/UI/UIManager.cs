using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager za sve UI panele (pauza, pobjeda, gubitak)
/// AŽURIRANO: Popravljen kursor + automatski sljedeći level
/// </summary>
public class UIManager : MonoBehaviour
{
	[Header("UI Panels")]
	[Tooltip("Panel koji se prikazuje kada igrač pauzira (ESC key)")]
	public GameObject pausePanel;
	[Tooltip("Panel koji se prikazuje kada igrač pobijedi")]
	public GameObject winPanel;
	[Tooltip("Panel koji se prikazuje kada igrač izgubi (opcionalno)")]
	public GameObject losePanel;

	[Header("Settings")]
	[Tooltip("Zaključaj kursor kada je igra aktivna?")]
	public bool lockCursorDuringGameplay = true;
	[Tooltip("Omogući pauziranje sa ESC tipkom?")]
	public bool enableEscPause = true;
	[Tooltip("Automatski učitaj sljedeći level nakon pobjede?")]
	public bool autoLoadNextLevel = false;
	[Tooltip("Koliko sekundi čekati prije auto-učitavanja sljedećeg levela?")]
	public float autoLoadDelay = 3f;

	[Header("Player Reference (Optional)")]
	[Tooltip("Referenca na igrača za blokiranje kretanja (opcionalno)")]
	public PlayerController playerController;

	[Header("Audio (Optional)")]
	[Tooltip("Zvuk kada se otvori panel (opcionalno)")]
	public AudioClip panelOpenSound;

	// Runtime varijable
	private bool isPaused = false;
	private bool isGameOver = false;

	void Start()
	{
		// Sakrij sve panele na početku
		if (pausePanel != null)
		{
			pausePanel.SetActive(false);
		}
		else
		{
			Debug.LogWarning("UIManager: Pause Panel nije assigniran!");
		}

		if (winPanel != null)
		{
			winPanel.SetActive(false);
		}
		else
		{
			Debug.LogWarning("UIManager: Win Panel nije assigniran!");
		}

		if (losePanel != null)
		{
			losePanel.SetActive(false);
		}

		// Auto-find PlayerController ako nije assigniran
		if (playerController == null)
		{
			playerController = FindObjectOfType<PlayerController>();
		}

		// Zaključaj kursor na početku (samo u gameplay sceni)
		if (lockCursorDuringGameplay)
		{
			LockCursor();
		}
	}

	void Update()
	{
		// ESC key za pauziranje (samo ako igra nije gotova)
		if (enableEscPause && !isGameOver && Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePause();
		}
	}

	// ===== PAUSE FUNKCIJE =====

	/// <summary>
	/// Prebaci između pauze i nastavka igre
	/// </summary>
	public void TogglePause()
	{
		if (isGameOver) return;

		isPaused = !isPaused;

		if (pausePanel != null)
		{
			pausePanel.SetActive(isPaused);
		}

		// Zamrzni/odmrzni vrijeme
		Time.timeScale = isPaused ? 0f : 1f;

		LevelTimer timer = FindObjectOfType<LevelTimer>();
		if (timer != null)
		{
			if (isPaused)
				timer.PauseTimer();
			else
				timer.ResumeTimer();
		}

		// KLJUČNI DIO: Blokiraj input igrača tijekom pauze
		if (playerController != null)
		{
			playerController.SetInputEnabled(!isPaused);
		}

		// Upravljaj kursorom
		if (isPaused)
		{
			UnlockCursor();
			PlayPanelSound();
		}
		else
		{
			if (lockCursorDuringGameplay)
			{
				LockCursor();
			}
		}

		Debug.Log($"UIManager: Igra {(isPaused ? "pauzirana" : "nastavljena")}");
	}

	/// <summary>
	/// Nastavi igru (gumb Resume u pause panelu)
	/// </summary>
	public void ResumeGame()
	{
		isPaused = false;

		if (pausePanel != null)
		{
			pausePanel.SetActive(false);
		}

		Time.timeScale = 1f;

		// Omogući input igrača
		if (playerController != null)
		{
			playerController.SetInputEnabled(true);
		}

		if (lockCursorDuringGameplay)
		{
			LockCursor();
		}

		Debug.Log("UIManager: Resume kliknut");
	}

	// ===== WIN/LOSE FUNKCIJE =====

	/// <summary>
	/// Prikaži Win panel (poziva se iz GoalZone)
	/// </summary>
	public void ShowWin()
	{
		if (isGameOver) return;

		isGameOver = true;

		// KLJUČNI DIO: Blokiraj input igrača
		if (playerController != null)
		{
			playerController.SetInputEnabled(false);
		}

		if (winPanel != null)
		{
			winPanel.SetActive(true);
			Time.timeScale = 0f;
			UnlockCursor();
			PlayPanelSound();

			// Reproduciraj win sound ako postoji AudioManager
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlayWinSound();
			}

			Debug.Log("UIManager: WIN! Čestitamo!");

			// NOVO: Automatski učitaj sljedeći level ako je omogućeno
			if (autoLoadNextLevel)
			{
				Invoke(nameof(LoadNextLevel), autoLoadDelay);
			}
		}
		else
		{
			Debug.LogError("UIManager: Win Panel nije assigniran!");
		}
	}

	/// <summary>
	/// Prikaži Lose panel
	/// </summary>
	public void ShowLose()
	{
		if (isGameOver) return;

		isGameOver = true;

		// Blokiraj input igrača
		if (playerController != null)
		{
			playerController.SetInputEnabled(false);
		}

		if (losePanel != null)
		{
			losePanel.SetActive(true);
			Time.timeScale = 0f;
			UnlockCursor();
			PlayPanelSound();

			Debug.Log("UIManager: LOSE! Pokušaj ponovno.");
		}
		else
		{
			Debug.LogWarning("UIManager: Lose Panel nije assigniran!");
		}
	}

	// ===== NAVIGATION FUNKCIJE =====

	/// <summary>
	/// Restart trenutnog levela
	/// </summary>
	public void RestartLevel()
	{
		Debug.Log("UIManager: Restarting level...");

		Time.timeScale = 1f;
		isGameOver = false;

		Scene currentScene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(currentScene.buildIndex);
	}

	/// <summary>
	/// Idi na glavni izbornik
	/// </summary>
	public void GoToMainMenu()
	{
		Debug.Log("UIManager: Odlazak na glavni izbornik...");

		Time.timeScale = 1f;
		isGameOver = false;

		SceneManager.LoadScene(0);
	}

	/// <summary>
	/// Idi na sljedeći level
	/// </summary>
	public void LoadNextLevel()
	{
		Debug.Log("UIManager: Učitavanje sljedećeg levela...");

		Time.timeScale = 1f;
		isGameOver = false;

		int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

		if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(nextSceneIndex);
		}
		else
		{
			Debug.LogWarning("UIManager: Nema više levela! Vraćam na glavni izbornik.");
			GoToMainMenu();
		}
	}

	/// <summary>
	/// Quit igru
	/// </summary>
	public void QuitGame()
	{
		Debug.Log("UIManager: Zatvaranje igre...");

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
	}

	// ===== HELPER FUNKCIJE =====

	/// <summary>
	/// Zaključaj kursor
	/// </summary>
	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	/// <summary>
	/// Otključaj kursor
	/// </summary>
	private void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	/// <summary>
	/// Reproduciraj zvuk panela
	/// </summary>
	private void PlayPanelSound()
	{
		if (AudioManager.Instance != null && panelOpenSound != null)
		{
			AudioManager.Instance.PlaySFX(panelOpenSound);
		}
	}

	// ===== JAVNE HELPER FUNKCIJE =====

	public bool IsPaused() => isPaused;
	public bool IsGameOver() => isGameOver;

	public void ResetGameOverState()
	{
		isGameOver = false;
		Debug.Log("UIManager: Game over stanje resetirano.");
	}
}