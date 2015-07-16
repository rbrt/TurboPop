using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSegment : MonoBehaviour{

	List<GridSegmentRow> segmentRows;

	void Awake(){
		segmentRows = new List<GridSegmentRow>();
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
}
