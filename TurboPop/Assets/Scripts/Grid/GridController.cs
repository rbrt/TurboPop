using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridController : MonoBehaviour {

	static GridController instance;
	static int segmentCount = 10,
			   gridWidth = 7,
			   gridHeight = 7;

	float movementAmount = 0,
		  movementFrequency = 5,
		  lastMoveTime = 0;

	List<GridSegment> gridSegments;

	GridSegment bufferSegment;

	void Awake(){
		if (instance == null){
			instance = this;
			gridSegments = new List<GridSegment>();

			lastMoveTime = Time.time;
		}
	}

	void Start(){
		movementAmount = GridInstantiator.Offset;
		GridInstantiator.Instance.CreateGrid();
	}

	void Update(){
		if (Time.time - lastMoveTime > movementFrequency){
			lastMoveTime = Time.time;
			AdvanceSegments();
		}
	}

	public static GridController Instance{
		get {
			return instance;
		}
	}

	public static int SegmentCount {
		get {
			return segmentCount;
		}
	}

	public static int GridWidth  {
		get {
			return gridWidth;
		}
	}

	public static int GridHeight {
		get {
			return gridHeight;
		}
	}

	public void SetBufferSegment(GridSegment segment){
		bufferSegment = segment;
		segment.transform.position = Vector3.one * 500;
		bufferSegment.ClearSegment();
	}

	public float GetTimeRemainingUntilSegmentsAdvance(){
		return 1 - (Time.time - lastMoveTime) / movementFrequency;
	}

	public void AddSegment(GridSegment segment){
		gridSegments.Add(segment);
	}

	public void AdvanceSegments(){
		HandleFrontmostSegmentOnAdvance();

		gridSegments.ForEach(segment => {
			this.StartSafeCoroutine(MoveSegment(segment));
		});
	}

	void HandleFrontmostSegmentOnAdvance(){
		// if the frontmost segment is not destroyed, push it forward and "kill"
		// any columns that remain.
		if (!gridSegments[0].IsDestroyed()){
			/*
			This logic can come later, because it shouldn't matter until we have
			reuse of segments working properly
			*/
		}

		// Deparent frontmostSegment and remove from list
		var oldFrontmostSegment = gridSegments[0];
		var oldCoords = oldFrontmostSegment.transform.localPosition;
		oldFrontmostSegment.transform.parent = null;

		gridSegments.RemoveAt(0);

		// Set bufferSegment as last segment in grid
		gridSegments.Add(bufferSegment);

		// Initialize last segment
		bufferSegment.InitializeSegment();
		bufferSegment.transform.parent = transform;
		bufferSegment.transform.localPosition = new Vector3(oldCoords.x,
															oldCoords.y,
															oldCoords.z + SegmentCount * GridInstantiator.Offset);

		// Set old frontmost segment as bufferSegment and clear it
		SetBufferSegment(oldFrontmostSegment);
	}

	/*
	Builds a 2D array of GridSegmentElements with no other GridSegmentElements in front
	of them. Indices in the array are not searched for again, because they are marked found
	in a separate 2D array of bools.
	*/
	public GridSegmentElement[,] GetFrontmostElementsForGrid(){
		GridSegmentElement[,] frontmostElements = new GridSegmentElement[gridHeight, gridWidth];
		bool[,] foundElements = new bool[gridHeight, gridWidth];
		int found = gridHeight * gridWidth;

		for (int i = 0; i < gridSegments.Count && found > 0; i++){
			for (int j = 0; j < gridHeight; j++){
				for (int k = 0; k < gridWidth ; k++){
					if (!foundElements[j,k]){
						var elementToCheck = gridSegments[i].GetSegmentRowAtIndex(k).GetSegmentElementAtIndex(j);
						if (!elementToCheck.Destroyed){
							found--;
							frontmostElements[j,k] = elementToCheck;
							foundElements[j,k] = true;
						}
					}
				}
			}
		}

		return frontmostElements;
	}

	void LogOutFrontmostElements(){
		var frontmostElements = GetFrontmostElementsForGrid();
		for (int i = 0; i < frontmostElements.GetLength(0); i++){
			for (int j = 0; j < frontmostElements.GetLength(1); j++){
				Debug.Log(frontmostElements[i,j].ToString(), frontmostElements[i,j].gameObject);
			}
		}
	}

	IEnumerator MoveSegment(GridSegment segment){
		float duration = .15f;
		Vector3 basePosition = segment.transform.localPosition;
		Vector3 newPosition = basePosition;
		newPosition.z -= movementAmount;

		for (float i = 0; i <= 1; i+= Time.deltaTime / duration){
			segment.transform.localPosition = Vector3.Lerp(basePosition, newPosition, i);
			yield return null;
		}
		segment.transform.localPosition = newPosition;
	}
}
