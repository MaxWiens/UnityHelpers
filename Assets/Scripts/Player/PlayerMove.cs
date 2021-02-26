using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {
	public float MaxSlope = 80f;
	public float MaxSpeed = 5f;
	public float MoveForce = 5f;
	public float InitalJumpForce = 8f;
	public float HoldJumpForce = 18f;
	public bool IsGrounded => _groundingObjects.Count > 0;
	private Vector3 _groundNormal = Vector3.up;
	public Vector3 GroundNormal => _groundNormal;
	[SerializeField, NotNull] private Rigidbody _rigidBody;
	private HashSet<GameObject> _groundingObjects = new HashSet<GameObject>();
	private HashSet<GameObject> _wallObjects = new HashSet<GameObject>();
	private Vector3 _wallNormal = Vector3.zero;

	private bool _isJumping = false;
	private float _jumpTimer = 0f;
	private float _jumpIncreaseTime = 0.5f;

	public void Move(Vector2 direction){
		if(direction.x != 0 || direction.y != 0){
			if((IsGrounded && _rigidBody.velocity.sqrMagnitude < MaxSpeed*MaxSpeed) || (!IsGrounded && new Vector2(_rigidBody.velocity.x,_rigidBody.velocity.z).sqrMagnitude < MaxSpeed*MaxSpeed)){
				Vector3 adjustedMoveVec = Vector3.zero;
				if(_groundNormal.x == 0 && _groundNormal.z == 0f){
					adjustedMoveVec = new Vector3(direction.x, 0f, direction.y);
				}else{
					Vector3 normalCrossUp = Vector3.Cross(Vector3.up,_groundNormal).normalized;
					Vector3 slopeDirection = Vector3.Cross(_groundNormal,normalCrossUp).normalized;
					float theata = Vector2.SignedAngle(direction, new Vector2(slopeDirection.x,slopeDirection.z))+90;
					Quaternion q = Quaternion.AngleAxis(theata, _groundNormal);
					adjustedMoveVec = q * normalCrossUp;
				}

				if(_wallNormal.x != 0f || _wallNormal.y != 0f ||_wallNormal.z != 0f)
					adjustedMoveVec = _wallNormal+adjustedMoveVec;
				if((IsGrounded && _isJumping) || !IsGrounded){
					_rigidBody.velocity = new Vector3(adjustedMoveVec.x*MoveForce, _rigidBody.velocity.y, adjustedMoveVec.z*MoveForce);
				}else if(IsGrounded){
					_rigidBody.velocity = adjustedMoveVec*MoveForce;
				}
				//_rigidBody.AddForce(adjustedMoveVec*MoveForce);
			}
		}else{
			if((IsGrounded && _isJumping) || !_isJumping){
				_rigidBody.velocity = new Vector3(_rigidBody.velocity.x*0.5f,_rigidBody.velocity.y,_rigidBody.velocity.z*0.5f);
			}else if(IsGrounded){
				_rigidBody.velocity = new Vector3(_rigidBody.velocity.x*0.5f,_rigidBody.velocity.y*0.5f,_rigidBody.velocity.z*0.5f);
			}
		}
		//Debug.DrawRay(transform.position, adjustedMoveVec, Color.cyan);
	}

	private Vector2 _moveVec = new Vector2();
	private bool _jumpPressed = false;
	private void Update() {
		Debug.DrawRay(transform.position, _groundNormal, Color.blue);
		_moveVec = new Vector2();
		if(Input.GetKey(KeyCode.W)) _moveVec.y = 1;
		if(Input.GetKey(KeyCode.A)) _moveVec.x = -1;
		if(Input.GetKey(KeyCode.S)) _moveVec.y -= 1;
		if(Input.GetKey(KeyCode.D)) _moveVec.x += 1;
		if(Input.GetKey(KeyCode.Space))
			_jumpPressed = true;
		else
			_jumpPressed = false;

		if(_isJumping)
			_jumpTimer += Time.deltaTime;
		Debug.Log($"IsJumping: {_isJumping}");
		Debug.Log($"IsGrounded: {IsGrounded}");
	}

	private void FixedUpdate() {
		Jump();
		Move(_moveVec);
	}

	public void Jump(){
		if(_jumpPressed){
			if(IsGrounded && !_isJumping){
				_rigidBody.AddForce(Vector3.up*InitalJumpForce, ForceMode.Impulse);
				_isJumping = true;
			}else if(_isJumping){
				if(_jumpTimer >= _jumpIncreaseTime){
					_jumpTimer = 0f;
					_isJumping = false;
				}else
					_rigidBody.AddForce(Vector3.up*HoldJumpForce);
			}
		}else{
			_isJumping = false;
			_jumpTimer = 0f;
		}
	}

	private void OnCollisionEnter(Collision other) {
		foreach(ContactPoint contactPoint in other.contacts){
			float f = Vector3.Angle(contactPoint.normal, Vector3.up);
			if(f <= MaxSlope){
				_groundNormal = contactPoint.normal;
				_groundingObjects.Add(other.gameObject);
				//Debug.Log($"new normal {f}");
				break;
			}else if(f <= 180 - MaxSlope){
				//Debug.Log($"wall normal? {f}");
				_wallNormal= contactPoint.normal;
				_wallObjects.Add(other.gameObject);
			}else{
				//Debug.Log($"celing normal? {f}");
			}

		}
	}

	private void OnCollisionStay(Collision other) {
		if(_groundingObjects.Contains(other.gameObject)){

			float maxY = Mathf.Sin(Mathf.Deg2Rad*MaxSlope);
			bool isGood = false;
			foreach(ContactPoint contactPoint in other.contacts){
				float f = Vector3.Angle(contactPoint.normal, Vector3.up);
				if(f <= MaxSlope){
					// is floor
					_groundNormal = contactPoint.normal;
					_groundingObjects.Add(other.gameObject);
					isGood = true;
					break;
				}else if(f <= 180 - MaxSlope){
					//Debug.Log($"wall normal? {f}");
					_wallNormal= contactPoint.normal;
					_wallObjects.Add(other.gameObject);
				}else{
					//Debug.Log($"celing normal? {f}");
				}
			}
			if(!isGood){
				_groundingObjects.Remove(other.gameObject);
				_groundNormal = Vector3.up;
			}
		}
	}



	private void OnCollisionExit(Collision other) {

		if(_groundingObjects.Remove(other.gameObject)){
			if(_groundingObjects.Count == 0)
				_groundNormal = Vector3.up;
			return;
		}else if(_wallObjects.Remove(other.gameObject)){
			if(_wallObjects.Count == 0)
				_wallNormal = Vector3.zero;
			return;
		}
	}
}