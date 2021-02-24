using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public bool IsGrounded => _groundingObjects.Count > 0;
	private Vector3 _groundNormal = Vector3.up;
	public Vector3 GroundNormal => _groundNormal;
	public float MaxSlope = 80f;
	public float MaxSpeed = 2f;

	[SerializeField, NotNull] private Rigidbody _rigidBody;

	public void Move(Vector2 direction){
		Vector3 adjustedGroundVector = Vector3.left;
		Vector3 v = Vector3.Cross(_groundNormal,Vector3.Cross(Vector3.up,_groundNormal));

		// adjustedGroundVector.x *= direction.x;
		// adjustedGroundVector.z *= direction.y;
		// if(_groundNormal != Vector3.up){
		// 	Vector3 upNormalCross = ;
		// 	adjustedGroundVector =
		// }
		Debug.DrawRay(transform.position, v.normalized, Color.red);
		//float f = ;
		if(v != Vector3.up){
			int i = 0;
		}
		var f1 = Mathf.Rad2Deg*Mathf.Atan2(v.y, v.x);
		var f2 = Mathf.Rad2Deg*Mathf.Atan2(v.y, v.z);
		Quaternion q1 = Quaternion.identity;
		Quaternion q2 = Quaternion.identity;
		Vector3 v3;
		if(v.x < 0){
			if(f1 != 90)
				q1 = Quaternion.Euler(0f,0f,f1);
			if(f2 != 90)
				q2 = Quaternion.Euler(-f2,0f,0f);
		}else{
			if(f1 != 90)
				q1 = Quaternion.Euler(0f,0f,f1);
			if(f2 != 90)
				q2 = Quaternion.Euler(-f2,0f,0f);
		}
		v3 = q2 * (q1  * new Vector3(direction.x, 0, direction.y));
		/*}else{
			q1 = Quaternion.Euler(-f1,0f,0f);
			q2 = Quaternion.Euler(0f,0f,f2);
			v3 = q2 * q1 *  new Vector3(direction.x, 0, direction.y);
		}*/
		Debug.DrawRay(transform.position, v3, Color.green);
		_rigidBody.AddForce(v3*20f);// new Vector3(-adjustedGroundVector.y, adjustedGroundVector.x, adjustedGroundVector.z)*100f;
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

	}

	private void FixedUpdate() {
		if(_jumpPressed)
			Jump();
		else
			Move(_moveVec);
	}

	public void Jump(){
		if(IsGrounded)
			_rigidBody.AddForce(Vector3.up*3f, ForceMode.Impulse);
	}

	private void OnCollisionEnter(Collision other) {
		foreach(ContactPoint contactPoint in other.contacts){
			float f = Vector3.Angle(contactPoint.normal, Vector3.up);
			if(f <= MaxSlope && f >= -MaxSlope){
				_groundNormal = contactPoint.normal;
				_groundingObjects.Add(other.gameObject);
				Debug.Log($"new normal {f}");
				break;
			}
		}
	}

	private void OnCollisionStay(Collision other) {
		if(_groundingObjects.Contains(other.gameObject)){

			float maxY = Mathf.Sin(Mathf.Deg2Rad*MaxSlope);
			bool isGood = false;
			foreach(ContactPoint contactPoint in other.contacts){
				float f = Vector3.Angle(contactPoint.normal, Vector3.up);
				if(f <= MaxSlope && f >= -MaxSlope){
					_groundNormal = contactPoint.normal;
					_groundingObjects.Add(other.gameObject);
					isGood = true;
					break;
				}
			}
			if(!isGood){
				_groundingObjects.Remove(other.gameObject);
				_groundNormal = Vector3.up;
			}
		}
	}

	private HashSet<GameObject> _groundingObjects = new HashSet<GameObject>();

	private void OnCollisionExit(Collision other) {
		_groundingObjects.Remove(other.gameObject);
		if(_groundingObjects.Count == 0)
			_groundNormal = Vector3.up;
	}
}