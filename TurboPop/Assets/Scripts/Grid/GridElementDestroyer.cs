﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridElementDestroyer : MonoBehaviour {

	static GridElementDestroyer instance;
	static int matchThreshold = 3;

	List<GridSegmentElement> elementsToClear;

	TurboMeter turboMeter;

	public static GridElementDestroyer Instance{
		get {
			return instance;
		}
	}

	void Awake(){
		if (instance == null){
			instance = this;
			elementsToClear = new List<GridSegmentElement>();
		}
	}

	void Start(){
		turboMeter = GUIController.Instance.GetTurboMeter();
	}

	public void DestroyMatchedElements(GridSegmentElement element){
		if (element.CubeColour == CubeColours.Dead){
			return;
		}

		elementsToClear.Clear();
		bool[,] checkedIndices = new bool[GridController.GridWidth, GridController.GridHeight];

		var frontmostElements = GridController.Instance.GetFrontmostElementsForGrid();
		IntPair clickedPosition = DeterminePositionOfClickedElementInGrid(element);

		DetermineMatchedElements(frontmostElements,
								 checkedIndices,
								 element,
								 clickedPosition);

		if (elementsToClear.Count >= matchThreshold){
			DestroyElements(elementsToClear);
		}
	}

	public void DestroyFrontmostSegment(){
		var frontmostSegment = GridController.Instance.GetFrontmostSegment();
		var elements = frontmostSegment.GetAllUndestroyedElementsInSegment();
		elements.ForEach(x => x.DestroyElement());
	}

	public void DestroyElements(List<GridSegmentElement> elements){
		elementsToClear.ForEach(x => x.DestroyElement());
		turboMeter.IncreaseTurbo(elementsToClear.Count);
	}

	/*
	Recursively checks a 2D array of GridSegmentElements for adjacent matches to
	a target element. It works its way out from the coordinates of the target
	element, and marks checked indices to avoid checking anything more than once
	*/
	void DetermineMatchedElements(GridSegmentElement[,] frontmostElements,
								  bool[,] checkedIndices,
								  GridSegmentElement element,
								  IntPair coords){

		// Will happen if a player manages to make it through all grid segments
		if(frontmostElements[coords.x, coords.y] == null){
			return;
		}

		if (frontmostElements[coords.x, coords.y].CubeColour == element.CubeColour && !checkedIndices[coords.x, coords.y]){
			checkedIndices[coords.x, coords.y] = true;
			elementsToClear.Add(frontmostElements[coords.x, coords.y]);
		}
		else {
			return;
		}

		IntPair right = new IntPair(coords.x + 1, coords.y),
				left = new IntPair(coords.x - 1, coords.y),
				up = new IntPair(coords.x, coords.y + 1),
				down = new IntPair(coords.x, coords.y - 1);

		if (up.y < GridController.GridHeight){
			DetermineMatchedElements(frontmostElements, checkedIndices, element, up);
		}

		if (down.y >= 0){
			DetermineMatchedElements(frontmostElements, checkedIndices, element, down);
		}

		if (right.x < GridController.GridWidth){
			DetermineMatchedElements(frontmostElements, checkedIndices, element, right);
		}

		if (left.x >= 0){
			DetermineMatchedElements(frontmostElements, checkedIndices, element, left);
		}
	}

	IntPair DeterminePositionOfClickedElementInGrid(GridSegmentElement element){
		var row = element.GetComponentInParent<GridSegmentRow>();

		return new IntPair(row.GetIndexOfElementInRow(element),
						   row.GetComponentInParent<GridSegment>().GetIndexOfRowInSegment(row));
	}

}
