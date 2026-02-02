using UnityEngine;

/// <summary>
/// Zona koja respawnira objekte kada padnu van mape
/// Povezana s RespawnManager (respawn logika) i AudioManager (death sound)
/// </summary>
public class DeathZone : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Povuci ovdje RespawnManager objekt iz scene")]
	public RespawnManager respawnManager;

	[Header("Settings")]
	[Tooltip("Koliko sekundi čekati prije respawna?")]
	public float respawnDelay = 0.2f;
	[Tooltip("Efekt koji se prikazuje kada objekt padne (opcionalno)")]
	public ParticleSystem deathParticles;

	void Start()
	{
		// Auto-find RespawnManager ako nije assigniran
		if (respawnManager == null)
		{
			respawnManager = FindObjectOfType<RespawnManager>();
			
			if (respawnManager == null)
			{
				Debug.LogError("DeathZone: RespawnManager nije pronađen u sceni! Dodaj RespawnManager objekt.", this);
			}
		}

		// Provjera particle sistema
		if (deathParticles != null)
		{
			deathParticles.Stop();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// Sigurnosna provjera prije korištenja
		if (respawnManager == null)
		{
			Debug.LogWarning("DeathZone: Ne mogu respawnati - RespawnManager nedostaje!");
			return;
		}

		Rigidbody rb = other.attachedRigidbody;
		if (rb != null)
		{
			Debug.Log($"DeathZone: {rb.name} pao u DeathZone!");

			// Reproduciraj death sound preko AudioManager-a
			if (AudioManager.Instance != null)
			{
				AudioManager.Instance.PlayDeathSound();
			}

			// Prikaži particle efekt na poziciji gdje je pao
			if (deathParticles != null)
			{
				// Instanciraj particles na poziciji pada
				ParticleSystem particles = Instantiate(
					deathParticles, 
					other.transform.position, 
					Quaternion.identity
				);
				particles.Play();
				
				// Uništi particles nakon što završe
				Destroy(particles.gameObject, particles.main.duration);
			}

			// Respawnaj objekt nakon delay-a
			if (respawnDelay > 0f)
			{
				// Privremeno isključi objekt da "nestane"
				other.gameObject.SetActive(false);
				
				// Pozovi respawn nakon delay-a
				Invoke(nameof(RespawnObject), respawnDelay);
				
				// Zapamti koji objekt treba respawnati
				objectToRespawn = rb;
			}
			else
			{
				// Instant respawn
				respawnManager.Respawn(rb);
			}
		}
	}

	// Privremena varijabla za delayed respawn
	private Rigidbody objectToRespawn;

	/// <summary>
	/// Respawnaj objekt nakon delay-a
	/// </summary>
	private void RespawnObject()
	{
		if (objectToRespawn != null && respawnManager != null)
		{
			// Aktiviraj objekt nazad
			objectToRespawn.gameObject.SetActive(true);
			
			// Respawnaj ga
			respawnManager.Respawn(objectToRespawn);
			
			objectToRespawn = null;
		}
	}

	// Vizualizacija zone u editoru
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.3f);
		Gizmos.DrawCube(transform.position, transform.localScale);
	}

	void OnDrawGizmosSelected()
	{
		// Nacrtaj warning ikonu za death zone
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, transform.localScale);
	}
}