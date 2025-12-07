using UnityEngine;

[CreateAssetMenu(fileName = "NewBook", menuName = "Game/Book Data (Image)")]
public class BookData : ScriptableObject
{
    public string title;
    public Sprite[] pages; // each sprite = one page image
}
