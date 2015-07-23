using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSegment : MonoBehaviour{

	List<GridSegmentRow> segmentRows;

	void Awake(){
		segmentRows = new List<GridSegmentRow>();
	}

	public void InitializeSegment(){
		for (int i = 0; i < segmentRows.Count; i++){
			segmentRows[i].InitializeRow();
		}
	}

	public void ClearSegment(){
		
	}

	public void AddSegmentRow(GridSegmentRow row){
		segmentRows.Add(row);
	}

	public GridSegmentRow GetSegmentRowAtIndex(int index){
		return segmentRows[index];
	}

	public int GetIndexOfRowInSegment(GridSegmentRow row){
		return segmentRows.IndexOf(row);
	}

	public bool IsDestroyed(){
		for (int i = 0; i < segmentRows.Count; i++){
			if (segmentRows[i].IsDestroyed()){
				return false;
			}
		}

		return true;
	}
}
