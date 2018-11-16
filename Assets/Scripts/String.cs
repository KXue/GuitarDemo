using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class String : MonoBehaviour {
	[Range(0, 1)]
	public float dampRate;
	public float waveVelocity;
	public float maxAmplitude;
	public float length;
	public int[] pointsPerFret;
	float time = 0;
	LineRenderer lineRenderer;
	Vector3[] startingPoints;
	List<Vector3> touchPoints;
	class LineSegment{
		public int startIndex;
		public int endIndex;
	}
	class Vibration{
		public LineSegment segment;
		public float amplitude;
	}
	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
		InitializeLine();
		ResizeBox();
	}
	private void InitializeLine(){
		Vector3 lineDirection = Vector3.right;
		int totalPoints = 0;
		foreach (int points in pointsPerFret)
		{
			totalPoints += points;
		}

		startingPoints = new Vector3[totalPoints + 1];
		float incrementalLength = length / totalPoints;
		Vector3 startPosition = transform.position - (Vector3.right * length * 0.5f);
		Vector3 endPosition = transform.position + (Vector3.right * length * 0.5f);
		
		for(int i = 0; i < totalPoints; i++){
			startingPoints[i] = startPosition + (lineDirection * incrementalLength * i);
		}

		Debug.Log(totalPoints);
		startingPoints[totalPoints] = endPosition;
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
