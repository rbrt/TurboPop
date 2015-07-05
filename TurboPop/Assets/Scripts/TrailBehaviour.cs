using UnityEngine;
using System.Collections;

public class TrailBehaviour : MonoBehaviour {

	float minX = -7f,
		  maxX = 7f,
		  minY = -5f,
		  maxY = 5f,
		  step = 1,
		  initialZ;

	SafeCoroutine animationCoroutine;

	void Start(){
		initialZ = transform.localPosition.z;
		animationCoroutine = this.StartSafeCoroutine(Animate());
	}

	IEnumerator Animate(){
		float duration = .1f;

		while (true){
			Debug.Log("Got new point!");
			var targetPoint = GetNewPoint();

			while (!AtDestination(targetPoint)){
				var currentPos = transform.localPosition;
				var targetPos = GetMoveTrail(targetPoint);
				for (float i = 0; i <= 1; i += Time.deltaTime / duration){
					transform.localPosition = Vector3.Lerp(currentPos, targetPos, i);
					yield return null;
				}
				transform.localPosition = targetPos;
			}
		}
	}

	Vector3 GetMoveTrail(Vector3 destination){
		var localPos = transform.localPosition;
		if (localPos.x != destination.x && localPos.y != destination.y){
			if (Random.Range(0,100) > 50){
				return GetMoveTowardsY(destination);
			}
			else{
				return GetMoveTowardsX(destination);
			}
		}
		else if (localPos.x != destination.x){
			return GetMoveTowardsX(destination);
		}
		else{
			return GetMoveTowardsY(destination);
		}
	}

	Vector3 GetMoveTowardsY(Vector3 destination){
		var move = transform.localPosition;
		move.y = move.y > destination.y ? move.y - 1 : move.y + 1;
		return move;
	}

	Vector3 GetMoveTowardsX(Vector3 destination){
		var move = transform.localPosition;
		move.x = move.x > destination.x ? move.x - 1 : move.x + 1;
		return move;
	}

	bool AtDestination(Vector3 destination){
		Debug.Log(destination + " " + transform.localPosition);
		return Vector3.Distance(destination, transform.localPosition) == 0;
	}

	Vector3 GetNewPoint(){
		return new Vector3((int)Random.Range(minX,maxX),
						   (int)Random.Range(minY,maxY),
						   initialZ);
	}
}
