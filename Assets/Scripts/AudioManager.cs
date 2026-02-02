using UnityEngine;

/// <summary>
/// Manager za pozadinsku glazbu i sound efekte
/// Napravljen kao Singleton - može se pristupiti iz bilo koje skripte
/// </summary>
public class AudioManager : MonoBehaviour
{
	// Singleton pattern - samo jedna instanca u sceni
	public static AudioManager Instance { get; private set; }

	[Header("Music")]
	[Tooltip("AudioSource za pozadinsku glazbu")]
	public AudioSource musicSource;
	[Tooltip("Pozadinska glazba koja će se reproducirati")]
	public AudioClip backgroundMusic;
	[Tooltip("Glasnoća glazbe (0-1)")]
	[Range(0f, 1f)]
	public float musicVolume = 0.5f;

	[Header("Sound Effects")]
	[Tooltip("AudioSource za sound efekte")]
	public AudioSource sfxSource;
	[Tooltip("Zvuk za pobjedu")]
	public AudioClip winSound;
	[Tooltip("Zvuk za pad u DeathZone")]
	public AudioClip deathSound;
	[Tooltip("Zvuk za klik na gumb")]
	public AudioClip buttonClickSound;
	[Tooltip("Glasnoća efekata (0-1)")]
	[Range(0f, 1f)]
	public float sfxVolume = 0.7f;

	[Header("Singleton Settings")]
	[Tooltip("Trebamo li čuvati AudioManager između scena?")]
	public bool persistBetweenScenes = true;

	void Awake()
	{
		// Singleton pattern - osiguraj samo jednu instancu
		if (Instance == null)
		{
			Instance = this;

			// OPCIONALNO: Čuvaj samo ako je omogućeno
			if (persistBetweenScenes)
			{
				DontDestroyOnLoad(gameObject);
			}
		}
		else
		{
			Destroy(gameObject); // Uništi duplikate
			return;
		}
	}

	void Start()
	{
		// Auto-assign komponente ako nisu postavljene
		if (musicSource == null)
		{
			// Pokušaj pronaći AudioSource komponente na ovom objektu
			AudioSource[] sources = GetComponents<AudioSource>();
			if (sources.Length > 0)
			{
				musicSource = sources[0];
			}
			else
			{
				// Dodaj novu AudioSource za glazbu
				musicSource = gameObject.AddComponent<AudioSource>();
			}
		}

		if (sfxSource == null)
		{
			// Dodaj drugu AudioSource za sound efekte
			sfxSource = gameObject.AddComponent<AudioSource>();
		}

		// Postavi postavke za glazbu
		if (musicSource != null)
		{
			musicSource.loop = true; // Ponavljaj glazbu
			musicSource.playOnAwake = false;
			musicSource.volume = musicVolume;
		}

		// Postavi postavke za sound efekte
		if (sfxSource != null)
		{
			sfxSource.loop = false;
			sfxSource.playOnAwake = false;
			sfxSource.volume = sfxVolume;
		}

		// Započni glazbu ako je assignirana
		PlayBackgroundMusic();
	}

	/// <summary>
	/// Pokreni pozadinsku glazbu
	/// </summary>
	public void PlayBackgroundMusic()
	{
		if (musicSource != null && backgroundMusic != null)
		{
			musicSource.clip = backgroundMusic;
			musicSource.Play();
			Debug.Log("AudioManager: Glazba pokrenuta.");
		}
		else
		{
			if (backgroundMusic == null)
				Debug.LogWarning("AudioManager: Background Music clip nije assigniran!");
		}
	}

	/// <summary>
	/// Zaustavi pozadinsku glazbu
	/// </summary>
	public void StopBackgroundMusic()
	{
		if (musicSource != null)
		{
			musicSource.Stop();
		}
	}

	/// <summary>
	/// Pauziraj/nastavi glazbu
	/// </summary>
	public void ToggleMusicPause()
	{
		if (musicSource != null)
		{
			if (musicSource.isPlaying)
				musicSource.Pause();
			else
				musicSource.UnPause();
		}
	}

	/// <summary>
	/// Reproduciraj sound efekt
	/// </summary>
	public void PlaySFX(AudioClip clip)
	{
		if (sfxSource != null && clip != null)
		{
			sfxSource.PlayOneShot(clip, sfxVolume);
		}
		else
		{
			if (clip == null)
				Debug.LogWarning("AudioManager: AudioClip je null!");
		}
	}

	/// <summary>
	/// Reproduciraj sound efekt s custom glasnoćom
	/// </summary>
	public void PlaySFX(AudioClip clip, float volume)
	{
		if (sfxSource != null && clip != null)
		{
			sfxSource.PlayOneShot(clip, volume);
		}
	}

	// ===== SPECIFIČNI SOUND EFEKTI =====

	/// <summary>
	/// Zvuk za pobjedu (poziva se iz GoalZone)
	/// </summary>
	public void PlayWinSound()
	{
		if (winSound != null)
		{
			PlaySFX(winSound);
			Debug.Log("AudioManager: Win sound!");
		}
	}

	/// <summary>
	/// Zvuk za pad u DeathZone
	/// </summary>
	public void PlayDeathSound()
	{
		if (deathSound != null)
		{
			PlaySFX(deathSound);
			Debug.Log("AudioManager: Death sound!");
		}
	}

	/// <summary>
	/// Zvuk za klik na UI gumb
	/// </summary>
	public void PlayButtonClick()
	{
		if (buttonClickSound != null)
		{
			PlaySFX(buttonClickSound);
		}
	}

	// ===== KONTROLA GLASNOĆE =====

	/// <summary>
	/// Postavi glasnoću glazbe
	/// </summary>
	public void SetMusicVolume(float volume)
	{
		musicVolume = Mathf.Clamp01(volume);
		if (musicSource != null)
		{
			musicSource.volume = musicVolume;
		}
	}

	/// <summary>
	/// Postavi glasnoću sound efekata
	/// </summary>
	public void SetSFXVolume(float volume)
	{
		sfxVolume = Mathf.Clamp01(volume);
		if (sfxSource != null)
		{
			sfxSource.volume = sfxVolume;
		}
	}

	/// <summary>
	/// Mute/unmute sve zvukove
	/// </summary>
	public void ToggleMute()
	{
		AudioListener.pause = !AudioListener.pause;
	}
}