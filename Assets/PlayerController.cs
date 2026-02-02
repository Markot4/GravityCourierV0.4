using UnityEngine;
using UnityEngine.SceneManagement; // ⭐ DODANO za provjeru scene

/// <summary>
/// Kontroler za kretanje igrača u 3D prostoru (First Person)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[Header("References")]
	public Rigidbody rb;
	[Tooltip("Child objekt s kamerom za vertikalno gledanje")]
	public Transform head;
	public Camera playerCamera;

	[Header("Movement Settings")]
	[Tooltip("Brzina hodanja (m/s)")]
	public float walkSpeed = 5f;
	[Tooltip("Brzina trčanja sa Shift (m/s)")]
	public float runSpeed = 10f;

	[Header("Look Settings")]
	[Tooltip("Osjetljivost miša za horizontalno okretanje")]
	public float mouseSensitivityX = 2f;
	[Tooltip("Osjetljivost miša za vertikalno gledanje")]
	public float mouseSensitivityY = 2f;
	[Tooltip("Koliko može gledati gore/dolje (stupnjevi)")]
	public float verticalLookLimit = 85f;

	[Header("Ground Check")]
	[Tooltip("Maksimalna udaljenost do tla za skok")]
	public float groundCheckDistance = 0.3f;
	public LayerMask groundLayer;

	// Runtime varijable
	private Vector3 moveDirection;
	private float verticalRotation = 0f;
	private bool isGrounded = false;
	private bool inputEnabled = true; // NOVO: Kontrola inputa

	void Start()
	{
		// Auto-assign komponente ako nisu postavljene
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}

		if (head == null)
		{
			// Pokušaj pronaći child objekt s kamerom
			if (playerCamera != null)
			{
				head = playerCamera.transform;
			}
			else
			{
				Debug.LogError("PlayerController: 'head' Transform nije assigniran!", this);
			}
		}

		// Postavi Rigidbody settings za FPS kontroler
		if (rb != null)
		{
			rb.freezeRotation = true; // Spriječi da fizika rotira igrača
			rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}

		// ⭐ KLJUČNI POPRAVAK: Zaključaj kursor SAMO u gameplay scenama
		// Provjeri da li je ovo MainMenu scena
		Scene currentScene = SceneManager.GetActiveScene();
		bool isMainMenu = (currentScene.name == "MainMenu" || currentScene.buildIndex == 0);

		if (!isMainMenu)
		{
			// Zaključaj kursor samo u gameplay scenama
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Debug.Log("PlayerController: Kursor zaključan (gameplay scena)");
		}
		else
		{
			// U MainMenu ostavi kursor slobodan
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Debug.Log("PlayerController: Kursor otkključan (MainMenu scena)");
		}
	}

	void Update()
	{
		// Provjera za ESC key (otključaj kursor)
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		// KLJUČNO: Samo ako je input omogućen
		if (!inputEnabled) return;

		HandleLook();
		HandleMovementInput();
	}

	void FixedUpdate()
	{
		// Provjeri je li na tlu
		CheckGroundStatus();

		// Primijeni kretanje u FixedUpdate (za fiziku)
		ApplyMovement();
	}

	/// <summary>
	/// Rotacija igrača pomoću miša
	/// </summary>
	void HandleLook()
	{
		if (head == null) return;

		// Horizontalna rotacija (okrećemo cijeli objekt igrača)
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
		transform.Rotate(Vector3.up * mouseX);

		// Vertikalna rotacija (samo head/kamera)
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
		verticalRotation -= mouseY;
		verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

		head.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
	}

	/// <summary>
	/// Čitanje WASD/Arrow keys inputa
	/// </summary>
	void HandleMovementInput()
	{
		if (rb == null) return;

		// Odaberi brzinu (trčanje ili hodanje)
		float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

		// Čitaj input
		float moveX = Input.GetAxis("Horizontal"); // A/D ili Left/Right
		float moveZ = Input.GetAxis("Vertical");   // W/S ili Up/Down

		// Lokalni smjer kretanja (relativno prema igraču)
		moveDirection = new Vector3(moveX, 0f, moveZ).normalized * currentSpeed;
	}

	/// <summary>
	/// Primijeni fizičko kretanje
	/// </summary>
	void ApplyMovement()
	{
		if (rb == null) return;

		// Transformiraj lokalni smjer u svjetski (player može biti rotiran)
		Vector3 worldMoveDirection = transform.TransformDirection(moveDirection);

		// VAŽNO: Zadrži postojeću Y komponentu (gravitacija!)
		worldMoveDirection.y = rb.velocity.y;

		// Primijeni brzinu
		rb.velocity = worldMoveDirection;
	}

	/// <summary>
	/// Provjeri je li igrač na tlu (za budući jump sistem)
	/// </summary>
	void CheckGroundStatus()
	{
		if (rb == null) return;

		// Raycast prema dolje
		isGrounded = Physics.Raycast(
			transform.position,
			Vector3.down,
			groundCheckDistance,
			groundLayer
		);
	}

	// ===== NOVI DODACI ZA BUDUĆE PROŠIRENJE =====

	/// <summary>
	/// NOVO: Omogući/onemogući input (poziva UIManager)
	/// </summary>
	public void SetInputEnabled(bool enabled)
	{
		inputEnabled = enabled;

		// Zaustavi kretanje kada se input onemogući
		if (!enabled && rb != null)
		{
			moveDirection = Vector3.zero;
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
		}

		Debug.Log($"PlayerController: Input {(enabled ? "omogućen" : "onemogućen")}");
	}

	/// <summary>
	/// PRIMJER: Funkcija za skakanje (dodaj kasnije)
	/// </summary>
	public void Jump(float jumpForce = 5f)
	{
		if (rb != null && isGrounded)
		{
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			Debug.Log("PlayerController: Skok!");
		}
	}

	/// <summary>
	/// PRIMJER: Teleportacija igrača (za respawn)
	/// </summary>
	public void Teleport(Vector3 newPosition)
	{
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			transform.position = newPosition;
			Debug.Log($"PlayerController: Teleportiran na {newPosition}");
		}
	}

	// Vizualizacija ground check u editoru
	void OnDrawGizmosSelected()
	{
		Gizmos.color = isGrounded ? Color.green : Color.red;
		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
	}
}