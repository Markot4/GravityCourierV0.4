using UnityEngine;

/// <summary>
/// Zona koja detektira kada igrač završi level
/// Povezana s UIManager (prikazuje win panel) i AudioManager (reprodukuje win sound)
/// </summary>
public class GoalZone : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Povuci ovdje UIManager objekt iz scene")]
	public UIManager uiManager;

	[Header("Settings")]
	[Tooltip("Želiš li vizualni efekt kada igrač dođe do cilja?")]
	public ParticleSystem winParticles;
	[Tooltip("Koliko sekundi čekati prije prikaza win panela?")]
	public float delayBeforeWin = 0.5f;

	private bool goalTriggered = false; // Sprečava višestruko aktiviranje

	void Start()
	{
		// Auto-find UIManager ako nije assigniran
		if (uiManager == null)
		{
			uiManager = FindObjectOfType<UIManager>();
			
			if (uiManager == null)
			{
				Debug.LogError("GoalZone: UIManager nije pronađen u sceni! Dodaj UIManager objekt.", this);
			}
		}

		// Provjera particle sistema
		if (winParticles != null)
		{
			winParticles.Stop(); // Zaustavi particles na početku
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// Sprečavamo da se Goal aktivira više puta
		if (goalTriggered) return;

		// Provjeri ima li objekt "Player" tag
		if (other.CompareTag("Player"))
		{
			Debug.Log("GoalZone: Igrač je stigao do cilja!");
			goalTriggered = true;

			// Aktiviraj particle efekt ako postoji
			if (winParticles != null)
			{
				winParticles.Play();
			}

			// Reproduciraj win sound preko AudioManager-a
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlayWinSound();
			}
			else
			{
				Debug.LogWarning("GoalZone: AudioManager nije pronađen - nema win sound-a.");
			}

			// Prikaži win panel nakon kratkog delay-a
			if (uiManager != null)
			{
				Invoke(nameof(ShowWinPanel), delayBeforeWin);
			}
			else
			{
				Debug.LogError("GoalZone: Ne mogu prikazati pobjedu - UIManager nedostaje!");
			}
		}
	}

	/// <summary>
	/// Prikaži win panel (poziva se nakon delay-a)
	/// </summary>
	private void ShowWinPanel()
	{
		if (uiManager != null)
		{
			uiManager.ShowWin();
		}
	}

	/// <summary>
	/// Reset goal stanja (za debug ili restart levela)
	/// </summary>
	public void ResetGoal()
	{
		goalTriggered = false;
		if (winParticles != null)
		{
			winParticles.Stop();
		}
		Debug.Log("GoalZone: Goal resetiran.");
	}

	// Vizualizacija zone u editoru
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0, 1, 0, 0.3f);
		Gizmos.DrawCube(transform.position, transform.localScale);
	}

	void OnDrawGizmosSelected()
	{
		// Nacrtaj ikonu za goal
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, transform.localScale);
	}
}