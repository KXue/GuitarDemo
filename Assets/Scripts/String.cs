using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class String : MonoBehaviour {
	[Range(0, 1)]
	public float dampRate;
	public float waveVelocity;
	public float maxAmplitude;
	public float length;
	public LineRenderer fretPrefab;
	public int[] pointsPerFret;
	float time = 0;
	LineRenderer lineRenderer;
	Vector3[] startingPoints;
	LineSegment[] frets;
	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
		InitializeLine();
		ResizeBox();
	}
	private void InitializeLine(){
		int totalPoints = 0;
		totalPoints += 2; // end points
		totalPoints += pointsPerFret.Length - 1; // fret points

		foreach (int points in pointsPerFret)
		{
			totalPoints += points;
		}

		startingPoints = new Vector3[totalPoints];
		frets = new LineSegment[pointsPerFret.Length];
		float incrementalLength = length / (totalPoints - 1);
		Vector3 startPosition = transform.position - (Vector3.right * length * 0.5f);
		Vector3 endPosition = transform.position + (Vector3.right * length * 0.5f);
		
		int fretStartIndex = 0;
		int fretIndex = 0;
		
		for(int i = 0; i < totalPoints; i++){
			startingPoints[i] = startPosition + (Vector3.right * incrementalLength * i);
			if(i - fretStartIndex == pointsPerFret[fretIndex] + 1){
				LineSegment fret = new LineSegment();
				fret.startIndex = fretStartIndex;
				fret.endIndex = i;
				fretStartIndex = i;
				frets[fretIndex] = fret;
				fretIndex++;
			}
		}

		LineRenderer newFret = Instantiate(fretPrefab, startingPoints[frets[0].startIndex], Quaternion.identity, transform);
		newFret.SetPosition(0, Vector3.forward * maxAmplitude);
		newFret.SetPosition(1, Vector3.forward * -maxAmplitude);
		foreach (LineSegment fret in frets)
		{
			newFret = Instantiate(fretPrefab, startingPoints[fret.endIndex], Quaternion.identity, transform);
			newFret.SetPosition(0, Vector3.forward * maxAmplitude);
			newFret.SetPosition(1, Vector3.forward * -maxAmplitude);
		}

		lineRenderer.positionCount = startingPoints.Length;
		lineRenderer.SetPositions(startingPoints);
	}
	private void ResizeBox(){
		BoxCollider collider = GetComponent<BoxCollider>();
		Vector3 newSize = new Vector3(length, 0.1f, maxAmplitude * 2); 
		collider.size = newSize;
	}
	// Update is called once per frame
	void Update () {
		List<StringTouch> touchList = GetRelevantTouches();
		Vector3[] holds = GetHoldPoints(touchList);
		// time += Time.deltaTime;
		// VibrateString(0, startingPoints.Length - 1, startingPoints);
		Vector3[] newString = GetStationaryString(holds);
		lineRenderer.positionCount = newString.Length;
		lineRenderer.SetPositions(newString);
	}
	List<StringTouch> GetRelevantTouches(){
		List<StringTouch> retList = new List<StringTouch>();
		List<Touch> touches = InputHelper.Instance.GetTouches();
		foreach (Touch touch in touches)
		{
			StringTouch relevantTouch = null;

			Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane);
			RaycastHit hit;
			bool touched = false;
        	Ray ray = Camera.main.ScreenPointToRay(touchPosition);
			if (Physics.Raycast(ray, out hit) && hit.transform == transform) {
				relevantTouch = new StringTouch();
				touched = true;
				relevantTouch.hitInfo = hit;
				relevantTouch.touch = touch;
			}

			if(touch.phase == TouchPhase.Moved){
				Vector2 oldTouch = touch.position - touch.deltaPosition;
				touchPosition = new Vector3(oldTouch.x, oldTouch.y, Camera.main.nearClipPlane);
				ray = Camera.main.ScreenPointToRay(touchPosition);
				if (Physics.Raycast(ray, out hit) && hit.transform == transform) {
					if(!touched){
						relevantTouch = new StringTouch();
						relevantTouch.hitInfo = hit;
						relevantTouch.touch = touch;
						relevantTouch.type = TouchType.EXIT;
						touched = true;
					}
					else{
						relevantTouch.type = TouchType.MOVED;
					}
				}
				else if(touched){
					relevantTouch.type = TouchType.ENTER;
				}
			}

			if(touched && relevantTouch.type == TouchType.NONE){
				switch(touch.phase){
					case TouchPhase.Began:
						relevantTouch.type = TouchType.ENTER;
					break;

					case TouchPhase.Canceled:
						relevantTouch.type = TouchType.EXIT;
					break;
					
					case TouchPhase.Ended:
						relevantTouch.type = TouchType.EXIT;
					break;
					
					case TouchPhase.Stationary:
						relevantTouch.type = TouchType.MOVED;
					break;
				}
			}
			if(touched){
				retList.Add(relevantTouch);
			}
		}
		return retList;
	}
	Vector3[] GetHoldPoints(List<StringTouch> touchList){
		List<Vector3> touchPoints = new List<Vector3>();
		foreach (StringTouch touch in touchList)
		{
			if(touch.type == TouchType.ENTER || touch.type == TouchType.MOVED){
				Vector3 localPoint = transform.InverseTransformPoint(touch.hitInfo.point);
				localPoint.y = 0;
				touchPoints.Add(localPoint);
			}
		}
		touchPoints.Sort((Vector3 a, Vector3 b)=>{return a.x.CompareTo(b.x);});
		return touchPoints.ToArray();
	}
	Vector3[] GetStationaryString(Vector3[] holdPoints){
		List<Vector3> newString = new List<Vector3>(startingPoints);
		newString.AddRange(holdPoints);
		newString.Sort((Vector3 a, Vector3 b)=>{return a.x.CompareTo(b.x);});
		
		int startingIndex = 0;
		foreach (Vector3 point in holdPoints)
		{
			int pointIndex = newString.IndexOf(point);
			StraightenSegment(newString, startingIndex, pointIndex);
			startingIndex = pointIndex;
		}
		StraightenSegment(newString, startingIndex, newString.Count - 1);
		
		return newString.ToArray();
	}
	void StraightenSegment(List<Vector3> segment, int startIndex, int endIndex){
		Vector3 startingPoint = segment[startIndex];
		Vector3 endPoint = segment[endIndex];
		float ratioZX = (endPoint.z - startingPoint.z) / (endPoint.x - startingPoint.x);
		for(int i = startIndex + 1; i < endIndex; i++){
			Vector3 newPosition = segment[i];
			newPosition.z = (newPosition.x - startingPoint.x) * ratioZX + startingPoint.z;
			segment[i] = newPosition;
		}
	}
	void VibrateString(int startIndex, int endIndex, Vector3[] points){
		Vector3 firstNode = points[startIndex];
		Vector3 secondNode = points[endIndex];
		
		float totalLength = (secondNode - firstNode).magnitude;
		int totalPoints = endIndex - startIndex;
		float increment = totalLength / totalPoints;
		float inverseTotal = 1f/totalPoints;
		float frequency = waveVelocity/totalLength;
		
		Vector3 lineDirection = (secondNode - firstNode).normalized;
		Vector3 upDirection = Quaternion.Euler(0, 90, 0) * lineDirection;

		for(int i = 0; i <= totalPoints; i++)
		{
			float displacement = maxAmplitude * Mathf.Sin(i * inverseTotal * Mathf.PI) * Mathf.Sin(time * frequency * 4 * Mathf.PI);
			points[startIndex + i] = firstNode + (lineDirection * increment * i) + (upDirection * displacement);
		}

	}
	void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.75F);
        Gizmos.DrawLine(transform.position - transform.right * length * 0.5f, transform.position + transform.right * length * 0.5f);
    }
}
public enum TouchType{ENTER, MOVED, EXIT, NONE}
struct LineSegment{
	public int startIndex;
	public int endIndex;
}
struct Vibration{
	public LineSegment segment;
	public float amplitude;
}

class StringTouch{
	public Touch touch;
	public TouchType type = TouchType.NONE;
	public RaycastHit hitInfo;
}
