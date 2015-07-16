using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridController : MonoBehaviour {

	static GridController instance;
	static int segmentCount = 10,
			   gridWidth = 7,
			   gridHeight = 7;

	List<GridSegment> gridSegments;

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

	public void AddSegment(GridSegment segment){
		gridSegments.Add(segment);
	}

	public void DestroyMatchedElements(GridSegmentElement element){
		var frontmostElements = GetFrontmostElementsForGrid();

	}

	void DeterminePositionOfClickedElementInGrid(GridSegmentElement element){
		var row = element.GetComponentInParent<GridSegmentRow>();

		int x = row.GetIndexOfElementInRow(element);
		int y = row.GetComponentInParent<GridSegment>().GetIndexOfRowInSegment(row);

	}

	/*
	Builds a 2D array of GridSegmentElements with no other GridSegmentElements in front
	of them. Indices in the array are not searched for again, because they are marked found
	in a separate 2D array of bools.
	*/
	GridSegmentElement[,] GetFrontmostElementsForGrid(){
		GridSegmentElement[,] frontmostElements = new GridSegmentElement[gridHeight, gridWidth];
		bool[,] foundElements = new bool[gridHeight, gridWidth];
		int found = gridHeight * gridWidth;

		for (int i = 0; i < gridSegments.Count && found > 0; i++){
			for (int j = 0; j < gridHeight; j++){
				for (int k = 0; k < gridWidth ; k++){
					if (!foundElements[j,k]){
						var elementToCheck = gridSegments[i].GetSegmentRowAtIndex(j).GetSegmentElementAtIndex(k);
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

	void Awake(){
		if (instance == null){
			instance = this;
			gridSegments = new List<GridSegment>();
		}
	}

	void Start () {

	}

	void Update () {

	}

	void LogOutFrontmostElements(){
		var frontmostElements = GetFrontmostElementsForGrid();
		for (int i = 0; i < frontmostElements.GetLength(0); i++){
			for (int j = 0; j < frontmostElements.GetLength(1); j++){
				Debug.Log(frontmostElements[i,j].ToString(), frontmostElements[i,j].gameObject);
			}
		}
	}
}
