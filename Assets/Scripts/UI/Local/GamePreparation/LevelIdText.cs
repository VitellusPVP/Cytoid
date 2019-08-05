using UnityEngine;
using UnityEngine.UI;

public class LevelIdText : MonoBehaviour, ScreenBecameActiveListener
{
    [GetComponent] public Text text;
    public void OnScreenBecameActive()
    {
        text.text = Context.ActiveLevel?.Meta.id ?? "Unknown";
    }
}