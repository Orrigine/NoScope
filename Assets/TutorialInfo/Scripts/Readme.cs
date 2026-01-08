using UnityEngine;

[CreateAssetMenu(fileName = "Readme", menuName = "Tutorial/Readme", order = 1)]
public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title;
    public Section[] sections;

    // Used by the editor to know whether to load the stored layout only once
    [HideInInspector]
    public bool loadedLayout = false;

    [System.Serializable]
    public class Section
    {
        public string heading;
        [TextArea(3, 10)]
        public string text;
        public string linkText;
        public string url;
    }
}
