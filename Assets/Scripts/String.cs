using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class String : MonoBehaviour {
	[Range(0, 1)]
	public float dampRate;
	public float waveVelocity;
	public Vector3 startPosition;
	public Vector3 endPosition;
	public int[] pointsPerFret;
	float time = 0;
	LineRenderer lineRenderer;
	Vector3[] startingPoints;
	List<Vector3> touchPoints;

	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
		InitializeLine();
	}
	private void InitializeLine(){

		float totalLength = (endPosition - startPosition).magnitude;
		Vector3 lineDirection = (endPosition - startPosition).normalized;
		int totalPoints = 0;
		foreach (int points in pointsPerFret)
		{
			totalPoints += points;
		}

		startingPoints = new Vector3[totalPoints + 1];
		float incrementalLength = totalLength / totalPoints;
		for(int i = 0; i < totalPoints; i++){
			startingPoints[i] = startPosition + (lineDirection * incrementalLength * i);
		}

		Debug.Log(totalPoints);
		startingPoints[totalPoints] = endPosition;
		lineRenderer.positionCount = startingPoints.Length;
		lineRenderer.SetPositions(startingPoints);
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		VibrateString(0, startingPoints.Length - 1, startingPoints);
		lineRenderer.SetPositions(startingPoints);
	}
	void VibrateString(int startIndex, int endIndex, Vector3[] points){
		Vector3 firstNode = points[startIndex];
		Vector3 secondNode = points[endIndex]; 
		
		float totalLength = (secondNode - firstNode).magnitude;
		int totalPoints = endIndex - startIndex;
		float increment = totalLength / totalPoints;
		float inverseTotal = 1f/totalPoints;
		
		Vector3 lineDirection = (secondNode - firstNode).normalized;
		Vector3 upDirection = Quaternion.Euler(0, 90, 0) * lineDirection;

		for(int i = 0; i <= totalPoints; i++)
		{
			float displacement = Mathf.Sin(i * inverseTotal * Mathf.PI) * Mathf.Sin(time);
			points[startIndex + i] = firstNode + (lineDirection * increment * i) + (upDirection * displacement);
		}

	}
	void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.75F);
        Gizmos.DrawLine(transform.position + startPosition, transform.position + endPosition);
    }
}
