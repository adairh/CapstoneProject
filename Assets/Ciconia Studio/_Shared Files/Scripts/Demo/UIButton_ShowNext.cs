using UnityEngine;

public class UIButton_ShowNext : MonoBehaviour
{
    // GameObjects list
    public GameObject[] GameObjectsList;
    private int shownGameObjectIndex = -1;


    private void Start()
    {
        for (var i = 0; i < GameObjectsList.Length; ++i)
            GameObjectsList[i].SetActive(false);
        SelectNextGameObject();
    }


    // Next or previous GameObjects onClick
    public void SelectNextGameObject()
    {
        var index = shownGameObjectIndex >= GameObjectsList.Length - 1 ? -1 : shownGameObjectIndex;
        SelectGameObject(index + 1);
    }

    public void SelectPreviousGameObject()
    {
        var index = shownGameObjectIndex <= 0 ? GameObjectsList.Length : shownGameObjectIndex;
        SelectGameObject(index - 1);
    }

    public void SelectGameObject(int index)
    {
        if (shownGameObjectIndex >= 0)
            GameObjectsList[shownGameObjectIndex].SetActive(false);
        shownGameObjectIndex = index;
        GameObjectsList[shownGameObjectIndex].SetActive(true);
    }
}