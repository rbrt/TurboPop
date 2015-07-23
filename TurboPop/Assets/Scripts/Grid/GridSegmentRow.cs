using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSegmentRow : MonoBehaviour {

	List<GridSegmentElement> elements;

	public List<GridSegmentElement> Elements{
		get {
			return elements;
		}
	}

	void Awake(){
		elements = new List<GridSegmentElement>();
	}

	public void InitializeRow(){
		for (int i = 0; i < elements.Count; i++){
			elements[i].InitializeElement(GridInstantiator.Instance.GetRandomCubeColour());
		}
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

	public bool IsDestroyed(){
		for (int i = 0; i < elements.Count; i++){
			if (elements[i].Destroyed){
				return false;
			}
		}

		return true;
	}
}
