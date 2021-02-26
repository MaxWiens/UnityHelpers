using UnityEngine;

public class BackForth : MonoBehaviour {
	[SerializeField, NotNull] private Rigidbody _rigidBody;
	private float _timer = 0f;
	public float MoveTime = 2f;
	public float Distance = 5;

	private Vector3 _home;

	private void Start() {
		_home = _rigidBody.position;
	}

	private void FixedUpdate() {
		_timer += Time.fixedDeltaTime;
		_rigidBody.position = _home+new Vector3(Mathf.Sin(_timer/MoveTime)*Distance,0f,0f);

		if(_timer >= MoveTime*2*Mathf.PI) _timer -= MoveTime*2*Mathf.PI;
	}
}