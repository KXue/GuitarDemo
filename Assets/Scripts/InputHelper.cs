using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
//Source: https://romanluks.eu/blog/how-to-simulate-touch-with-mouse-in-unity/
public class InputHelper : MonoBehaviour
{
	public float touchRadius;
    private TouchCreator lastFakeTouch;
	private List<Touch> holds;
	private int holdID = 0;
	private static InputHelper instance;
	public static InputHelper Instance{
		get{
			return instance;
		}
	}
	private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
		holds = new List<Touch>();
    }

    public List<Touch> GetTouches()
    {
        List<Touch> touches = new List<Touch>();
        touches.AddRange(Input.touches);
		touches.AddRange(holds);
#if UNITY_EDITOR
        if (lastFakeTouch == null) lastFakeTouch = new TouchCreator();
        if (Input.GetMouseButtonDown(0))
        {
            lastFakeTouch.phase = TouchPhase.Began;
            lastFakeTouch.deltaPosition = new Vector2(0, 0);
            lastFakeTouch.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.fingerId = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastFakeTouch.phase = TouchPhase.Ended;
            Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
            lastFakeTouch.position = newPosition;
            lastFakeTouch.fingerId = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            lastFakeTouch.phase = TouchPhase.Moved;
            Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
            lastFakeTouch.position = newPosition;
            lastFakeTouch.fingerId = 0;
        }
        else
        {
            lastFakeTouch = null;
        }
        if (lastFakeTouch != null) touches.Add(lastFakeTouch.Create());
#endif
        return touches;
    }
	private void Update() {
		if(Input.GetMouseButtonDown(1)){
			TouchCreator newHold = new TouchCreator();
			newHold.phase = TouchPhase.Moved;
            newHold.deltaPosition = new Vector2(0, 0);
            newHold.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            newHold.fingerId = ++holdID;
			holds.Add(newHold.Create());
		}
		if(Input.GetButton("Clear")){
			holds.Clear();
		}
	}
	private void OnDrawGizmos() {
		Gizmos.color = new Color(1, 1, 0, 0.75f);
		if(holds != null){
			foreach(Touch touch in holds){
				Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane);
				touchPosition = Camera.main.ScreenToWorldPoint(touchPosition);
				touchPosition.y = 0;
				Gizmos.DrawSphere(touchPosition, touchRadius);
			}
		}
		if(lastFakeTouch != null){
			Vector3 touchPosition = new Vector3(lastFakeTouch.position.x, lastFakeTouch.position.y, Camera.main.nearClipPlane);
			touchPosition = Camera.main.ScreenToWorldPoint(touchPosition);
			touchPosition.y = 0;
			Gizmos.DrawSphere(touchPosition, touchRadius);
		}
	}

}

public class TouchCreator
{
    static BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
    static Dictionary<string, FieldInfo> fields;

    object touch;

    public float deltaTime { get { return ((Touch)touch).deltaTime; } set { fields["m_TimeDelta"].SetValue(touch, value); } }
    public int tapCount { get { return ((Touch)touch).tapCount; } set { fields["m_TapCount"].SetValue(touch, value); } }
    public TouchPhase phase { get { return ((Touch)touch).phase; } set { fields["m_Phase"].SetValue(touch, value); } }
    public Vector2 deltaPosition { get { return ((Touch)touch).deltaPosition; } set { fields["m_PositionDelta"].SetValue(touch, value); } }
    public int fingerId { get { return ((Touch)touch).fingerId; } set { fields["m_FingerId"].SetValue(touch, value); } }
    public Vector2 position { get { return ((Touch)touch).position; } set { fields["m_Position"].SetValue(touch, value); } }
    public Vector2 rawPosition { get { return ((Touch)touch).rawPosition; } set { fields["m_RawPosition"].SetValue(touch, value); } }

    public Touch Create()
    {
        return (Touch)touch;
    }

    public TouchCreator()
    {
        touch = new Touch();
    }

    static TouchCreator()
    {
        fields = new Dictionary<string, FieldInfo>();
        foreach (var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            fields.Add(f.Name, f);
            // Debug.Log("name: " + f.Name);
        }
    }
}