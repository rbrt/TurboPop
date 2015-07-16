using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSegmentRow : MonoBehaviour {

	List<GridSegmentElement> elements;

	void Awake(){
		elements = new List<GridSegmentElement>();
	}

	public void AddElement(GridSegmentElement element){
		elements.Add(element);
	}

	public GridSegmentElement GetSegmentElementAtIndex(int index){
		return elements[index];
	}

	public int GetIndexOfElementInRow(GridSegmentElement element){
		return elements.IndexOf(element);
	}
}
