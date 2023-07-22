using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
	public int CurrentWorld { get; private set; } = 0;

	[Header("Settings")]
	[SerializeField] private float followSpeed = 6f;

	[Header("Settings/Transition")]
	[SerializeField] private float transitionSpeed = 10f;

	[Header("Bindings")]
	[SerializeField] private Transform follow = null;

	[Header("Bindings/Transition")]
	[SerializeField] private new Camera camera = null;
	[SerializeField] private Image tint = null;
	[SerializeField] private World[] worlds = System.Array.Empty<World>();

	private Vector3 offset = Vector3.zero;
	private float transitionHeight = 1f;
	private Coroutine transitionRoutine = null;
	
	private List<WorldCache> worldCache = new();

	/// <summary>
	/// Unity's callback
	/// </summary>
	void Start()
	{
		//Follow offset
		offset = transform.position - follow.position;

		foreach(var world in worlds)
			worldCache.Add(new()
			{
				obstacles = world.obstaclesParent.GetComponentsInChildren<Animator>()
			});

		//Prepare first world view
		SetWorld(0);
	}

	/// <summary>
	/// Unity's callback
	/// </summary>
	private void Update()
	{
		//Camera follow control
		var finalPos = follow.position + offset;
		finalPos.y *= transitionHeight * transitionHeight; //steep ease
		transform.position = Vector3.Lerp(
			transform.position, finalPos,
			Time.deltaTime * followSpeed
		);

		//Transition control
		if(Input.GetKeyDown(KeyCode.X)
		&& transitionRoutine == null
		)
			transitionRoutine = StartCoroutine(Transition());
	}

	/// <summary>
	/// Switching transition coroutine
	/// </summary>
	IEnumerator Transition()
	{
		System.Func<float, float, float> transitionProgress = (progress, target) => {
			tint.color = new(
				tint.color.r, tint.color.g, tint.color.b,
				Mathf.Sqrt(1 - progress)
			);
			return Mathf.Lerp(progress, target, Time.unscaledDeltaTime * transitionSpeed);
		};

		//FadeOut
		while(transitionHeight > 0.01f)
		{
			transitionHeight = transitionProgress(transitionHeight, 0f);
			Time.timeScale = 0.1f + Mathf.Sqrt(transitionHeight);
			yield return null;
		}

		Switch();

		//FadeIn
		while(transitionHeight < 0.98f)
		{
			transitionHeight = transitionProgress(transitionHeight, 1f);
			Time.timeScale = 0.1f + (transitionHeight * transitionHeight);
			yield return null;
		}
		transitionHeight = transitionProgress(1f, 1f);
		Time.timeScale = 1f;

		transitionRoutine = null;
	}

	/// <summary>
	/// Switches to the next world on the list (rotary)
	/// </summary>
	public void Switch()
	{
		//Rotary switch
		int nextWorld = CurrentWorld + 1;
		if(nextWorld >= worlds.Length)
			nextWorld = 0;

		SetWorld(nextWorld);
	}

	/// <summary>
	/// Sets current worlds, disables others
	/// </summary>
	/// <param name="index">World index</param>
	private void SetWorld(int index)
	{
		if(index < 0 || index >= worlds.Length)
		{
			Debug.LogError("[CameraManager] No world of such index!");
			return;
		}

		CurrentWorld = index;
		World world = worlds[index];

		//Collision and animators update
		int playerMaskIdx = LayerMask.NameToLayer("Player");
		for(int i = 0; i < worlds.Length; ++i)
		{
			Physics.IgnoreLayerCollision(
				playerMaskIdx, LayerMask.NameToLayer(worlds[i].name), false
			);
			foreach(var obs in worldCache[i].obstacles)
				obs.speed = 0f;
		}
		//---Current world
		Physics.IgnoreLayerCollision(
			playerMaskIdx, LayerMask.NameToLayer(world.name), true
		);
		foreach(var obs in worldCache[index].obstacles)
			obs.speed = 1f;

		//Camera positioning update
		camera.transform.SetPositionAndRotation(
			world.cameraPositioningTransform.position,
			world.cameraPositioningTransform.rotation
		);
		camera.cullingMask = world.cullingMask;

		Debug.Log($"[CameraManager] New world set {index}!");
	}

	[System.Serializable]
	public class World
	{
		public string name = null;
		public LayerMask cullingMask = -1;
		public Transform cameraPositioningTransform = null;
		public Transform obstaclesParent = null;
	}
	[System.Serializable]
	public class WorldCache
	{
		public Animator[] obstacles = System.Array.Empty<Animator>();
	}
}
