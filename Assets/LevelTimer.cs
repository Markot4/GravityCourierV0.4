using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Timer koji automatski postavlja vrijeme ovisno o levelu
/// Level 1 = 10 sekundi, Level 2 = 20 sekundi, itd.
/// </summary>
public class LevelTimer : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Text UI element za prikaz vremena (optional)")]
	public Text timerText;
	[Tooltip("UIManager za prikaz lose panela")]
	public UIManager uiManager;

	[Header("Time Settings")]
	[Tooltip("Koliko sekundi za Level 1?")]
	public float level1Time = 10f;
	[Tooltip("Koliko sekundi za Level 2?")]
	public float level2Time = 20f;
	[Tooltip("Koliko sekundi za Level 3?")]
	public float level3Time = 30f;
	[Tooltip("Koliko sekundi za sve ostale levele?")]
	public float defaultTime = 15f;

	[Header("Warning Settings")]
	[Tooltip("Koliko sekundi prije kraja za crveno upozorenje?")]
	public float warningTime = 5f;
	[Tooltip("Zvuk upozorenja (optional)")]
	public AudioClip warningSound;

	// Runtime varijable
	private float remainingTime;
	private float totalTime;
	private bool timerRunning = false;
	private bool warningPlayed = false;

	void Start()
	{
		// Auto-find UIManager ako nije assigniran
		if (uiManager == null)
		{
			uiManager = FindObjectOfType<UIManager>();
			if (uiManager == null)
			{
				Debug.LogError("LevelTimer: UIManager nije pronađen u sceni!");
			}
		}

		// Auto-find Text UI ako nije assigniran
		if (timerText == null)
		{
			timerText = FindObjectOfType<Text>();
			if (timerText != null)
			{
				Debug.Log("LevelTimer: Automatski pronađen Timer Text UI.");
			}
		}

		// Odredi vrijeme za trenutni level
		SetTimeForCurrentLevel();

		// Pokreni timer
		StartTimer();
	}

	void Update()
	{
		if (!timerRunning) return;

		// Odbrojavanje
		remainingTime -= Time.deltaTime;

		// Upozorenje kada ostane malo vremena
		if (!warningPlayed && remainingTime <= warningTime && remainingTime > 0f)
		{
			PlayWarningSound();
			warningPlayed = true;
		}

		// Provjeri je li vrijeme isteklo
		if (remainingTime <= 0f)
		{
			remainingTime = 0f;
			OnTimeUp();
		}

		// Ažuriraj prikaz
		UpdateTimerDisplay();
	}

	/// <summary>
	/// Postavi vrijeme ovisno o trenutnom levelu
	/// </summary>
	void SetTimeForCurrentLevel()
	{
		Scene currentScene = SceneManager.GetActiveScene();
		int sceneIndex = currentScene.buildIndex;
		string sceneName = currentScene.name.ToLower();

		if (sceneName.Contains("[") && sceneName.Contains("s]"))
		{
			int startIndex = sceneName.IndexOf("[") + 1;
			int endIndex = sceneName.IndexOf("s]");

			if (endIndex > startIndex)
			{
				string timeString = sceneName.Substring(startIndex, endIndex - startIndex);

				if (float.TryParse(timeString, out float customTime))
				{
					totalTime = customTime;
					remainingTime = totalTime;
					Debug.Log($"LevelTimer: Custom vrijeme iz imena scene '{sceneName}' - {totalTime} sekundi");
					return; // Gotovo, izađi iz funkcije
				}
			}
		}

		// Logika: Detekcija levela
		if (sceneName.Contains("level 1") || sceneName.Contains("level1") || sceneIndex == 1)
		{
			totalTime = level1Time;
			Debug.Log($"LevelTimer: Level 1 detektiran - {level1Time} sekundi");
		}
		else if (sceneName.Contains("level 2") || sceneName.Contains("level2") || sceneIndex == 2)
		{
			totalTime = level2Time;
			Debug.Log($"LevelTimer: Level 2 detektiran - {level2Time} sekundi");
		}
		else if (sceneName.Contains("level 3") || sceneName.Contains("level3") || sceneIndex == 3)
		{
			totalTime = level3Time;
			Debug.Log($"LevelTimer: Level 3 detektiran - {level3Time} sekundi");
		}
		else
		{
			totalTime = defaultTime;
			Debug.Log($"LevelTimer: Nepoznati level - koristi default {defaultTime} sekundi");
		}

		remainingTime = totalTime;
	}

	/// <summary>
	/// Pokreni timer
	/// </summary>
	public void StartTimer()
	{
		timerRunning = true;
		Debug.Log("LevelTimer: Timer pokrenut!");
	}

	/// <summary>
	/// Pauziraj timer
	/// </summary>
	public void PauseTimer()
	{
		timerRunning = false;
		Debug.Log("LevelTimer: Timer pauziran.");
	}

	/// <summary>
	/// Nastavi timer
	/// </summary>
	public void ResumeTimer()
	{
		timerRunning = true;
		Debug.Log("LevelTimer: Timer nastavljen.");
	}

	/// <summary>
	/// Ažuriraj UI text s preostalim vremenom
	/// </summary>
	void UpdateTimerDisplay()
	{
		if (timerText == null) return;

		// Formatiraj vrijeme kao MM:SS
		int minutes = Mathf.FloorToInt(remainingTime / 60f);
		int seconds = Mathf.FloorToInt(remainingTime % 60f);

		timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);

		// Promijeni boju kada ostane malo vremena
		if (remainingTime <= warningTime)
		{
			timerText.color = Color.red;
		}
		else
		{
			timerText.color = Color.white;
		}
	}

	/// <summary>
	/// Poziva se kada vrijeme istekne
	/// </summary>
	void OnTimeUp()
	{
		timerRunning = false;

		Debug.Log("LevelTimer: VRIJEME ISTEKLO!");

		// Prikaži lose panel preko UIManager-a
		if (uiManager != null)
		{
			uiManager.ShowLose();
		}
		else
		{
			Debug.LogError("LevelTimer: Ne mogu prikazati Lose Panel - UIManager nedostaje!");
		}
	}

	/// <summary>
	/// Reproduciraj zvuk upozorenja
	/// </summary>
	void PlayWarningSound()
	{
		if (AudioManager.Instance != null && warningSound != null)
		{
			AudioManager.Instance.PlaySFX(warningSound);
		}
		Debug.LogWarning("LevelTimer: UPOZORENJE - ostalo malo vremena!");
	}

	// ===== DODATNE FUNKCIJE =====

	/// <summary>
	/// Dodaj bonus vrijeme (za power-upove)
	/// </summary>
	public void AddBonusTime(float bonusSeconds)
	{
		remainingTime += bonusSeconds;
		Debug.Log($"LevelTimer: Dodano {bonusSeconds} sekundi!");
	}

	/// <summary>
	/// Resetiraj timer na početnu vrijednost
	/// </summary>
	public void ResetTimer()
	{
		remainingTime = totalTime;
		timerRunning = true;
		warningPlayed = false;
		UpdateTimerDisplay();
		Debug.Log("LevelTimer: Timer resetiran.");
	}

	/// <summary>
	/// Dobavi preostalo vrijeme
	/// </summary>
	public float GetRemainingTime()
	{
		return remainingTime;
	}

	/// <summary>
	/// Provjeri je li timer aktivan
	/// </summary>
	public bool IsTimerRunning()
	{
		return timerRunning;
	}
}