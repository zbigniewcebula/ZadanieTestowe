using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private float speed = 5f;
	[SerializeField] private float jumpForce = 15f;
	[SerializeField] private LayerMask terrainMask = 0;

	[Header("Bindings")]
	[SerializeField] private CharacterController controller = null;

	private Vector2 vel = Physics.gravity;
	
	void FixedUpdate()
	{
		//Basic gravity
		if(controller.isGrounded)
			vel.y = 0f;
		else
			vel.y += Physics.gravity.y * Time.fixedDeltaTime;

		//Basic linear horizontal movement
		vel.Set(Input.GetAxis("Horizontal") * speed, vel.y);

		//Basic "coyote time" jump
		Debug.DrawRay(transform.position, Vector3.down, Color.red);
		bool willBeGrounded = controller.isGrounded;
		if(Physics.SphereCast(
			transform.position, 0.2f, Vector3.down,
			out var hitInfo,
			controller.height * 1.1f,
			terrainMask
		))
			willBeGrounded = true;

		//Basic jumping
		if(willBeGrounded
		&& Input.GetAxisRaw("Jump") > 0
		&& vel.y < 1f
		)
			vel.Set(vel.x, -Physics.gravity.y * jumpForce * Time.fixedDeltaTime);

		//Physics update
		controller.Move(vel * Time.fixedDeltaTime);

		//Fixing on Z-axis (effecr of 3D characterController on 3D terrain on 2D plane)
		controller.transform.position = new Vector3(
			controller.transform.position.x,
			controller.transform.position.y,
			0
		);
	}
}
