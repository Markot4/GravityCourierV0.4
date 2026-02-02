using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager za respawnanje objekata na njihove početne pozicije
/// VAŽNO: Ime datoteke MORA biti "RespawnManager.cs" (bez typo-a!)
/// </summary>
public class RespawnManager : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Automatski traži sve Rigidbody objekte u sceni pri startu?")]
	public bool autoRegisterRigidbodies = true;

	[Tooltip("Resetirati samo objekte koji nisu kinematski?")]
	public bool onlyNonKinematic = true;

	// Dictionary za pamćenje početnih pozicija
	private Dictionary<Rigidbody, Vector3> startPositions = new Dictionary<Rigidbody, Vector3>();
	private Dictionary<Rigidbody, Quaternion> startRotations = new Dictionary<Rigidbody, Quaternion>();

	void Start()
	{
		if (autoRegisterRigidbodies)
		{
			RegisterAllRigidbodies();
		}
	}

	/// <summary>
	/// Registriraj sve Rigidbody objekte u sceni
	/// </summary>
	public void RegisterAllRigidbodies()
	{
		Rigidbody[] bodies = FindObjectsOfType<Rigidbody>();
		int registeredCount = 0;

		foreach (Rigidbody rb in bodies)
		{
			// Provjeri je li trebamo registrirati
			if (onlyNonKinematic && rb.isKinematic)
			{
				continue; // Preskoči kinematske objekte
			}

			// Spremi početnu poziciju i rotaciju
			if (!startPositions.ContainsKey(rb))
			{
				startPositions[rb] = rb.position;
				startRotations[rb] = rb.rotation;
				registeredCount++;
			}
		}

		Debug.Log($"RespawnManager: Registrirano {registeredCount} objekata za respawn.");
	}

	/// <summary>
	/// Ručno registriraj specifični Rigidbody
	/// </summary>
	public void RegisterRigidbody(Rigidbody rb)
	{
		if (rb == null)
		{
			Debug.LogWarning("RespawnManager: Pokušaj registracije null Rigidbody!");
			return;
		}

		if (!startPositions.ContainsKey(rb))
		{
			startPositions[rb] = rb.position;
			startRotations[rb] = rb.rotation;
			Debug.Log($"RespawnManager: Registriran {rb.name}");
		}
	}

	/// <summary>
	/// Respawnaj specifični Rigidbody objekt
	/// </summary>
	public void Respawn(Rigidbody rb)
	{
		if (rb == null)
		{
			Debug.LogWarning("RespawnManager: Pokušaj respawna null Rigidbody!");
			return;
		}

		if (!startPositions.ContainsKey(rb))
		{
			Debug.LogWarning($"RespawnManager: {rb.name} nije registriran! Registriram sada...");
			RegisterRigidbody(rb);
			return;
		}

		// Resetiraj brzine
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		// Vrati na početnu poziciju i rotaciju
		rb.position = startPositions[rb];
		rb.rotation = startRotations[rb];

		Debug.Log($"RespawnManager: {rb.name} respawniran na {startPositions[rb]}");
	}

	/// <summary>
	/// Respawnaj sve registrirane objekte
	/// </summary>
	public void RespawnAll()
	{
		int respawnedCount = 0;

		foreach (var kvp in startPositions)
		{
			Rigidbody rb = kvp.Key;
			if (rb != null)
			{
				Respawn(rb);
				respawnedCount++;
			}
		}

		Debug.Log($"RespawnManager: Respawnano {respawnedCount} objekata.");
	}

	// ===== NOVI DODACI ZA PROŠIRENJE =====

	/// <summary>
	/// PRIMJER: Postavi novu početnu poziciju za objekt (checkpoint sistem)
	/// </summary>
	public void UpdateStartPosition(Rigidbody rb, Vector3 newPosition)
	{
		if (rb != null && startPositions.ContainsKey(rb))
		{
			startPositions[rb] = newPosition;
			Debug.Log($"RespawnManager: Nova start pozicija za {rb.name}: {newPosition}");
		}
	}

	/// <summary>
	/// PRIMJER: Ukloni objekt iz respawn sistema
	/// </summary>
	public void UnregisterRigidbody(Rigidbody rb)
	{
		if (rb != null)
		{
			startPositions.Remove(rb);
			startRotations.Remove(rb);
			Debug.Log($"RespawnManager: {rb.name} uklonjen iz respawn sistema.");
		}
	}

	/// <summary>
	/// Debug info - koliko je objekata registrirano
	/// </summary>
	public int GetRegisteredCount()
	{
		return startPositions.Count;
	}
}