//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BottomNavigationBar : MonoBehaviour
//{
//    [SerializeField] GameObject[] panels;

//    public void NavigationBarClick(GameObject activePanel)
//    {
//        for (int i = 0; i < panels.Length; i++)
//        {
//            panels[i].SetActive(false);
//        }

//        activePanel.SetActive(true);
//    }

//    void Start()
//    {
//        NavigationBarClick(panels[0]);
//    }

//    //vuot man hinh de chuyen doi giua cac tab

//    private int currentIndex = 0;

//    //vuot ve sau (sang phai)
//    public void SwipeToNext()
//    {
//        int nextIndex = (currentIndex + 1) % panels.Length;
//        NavigationBarClick(panels[nextIndex]);
//        currentIndex = nextIndex;
//    }

//    //vuot ve truoc (sang trai)
//    public void SwipeToPrevious()
//    {
//        int prevIndex = (currentIndex - 1 + panels.Length) % panels.Length;
//        NavigationBarClick(panels[prevIndex]);
//        currentIndex = prevIndex;
//    }
//}

using UnityEngine;

public class BottomNavigationBar : MonoBehaviour
{
    [SerializeField] GameObject[] panels; // gom 4 panel: home, file, draw, setting

    private int currentIndex = 0;

    void Start()
    {
        ShowPanel(0); // hien thi panel home dau tien
    }

    public void NavigationBarClick(int index)
    {
        ShowPanel(index);
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
        currentIndex = index;
    }

    // Swipe 
    public void SwipeToNext()
    {
        int nextIndex = (currentIndex + 1) % panels.Length;
        ShowPanel(nextIndex);
    }

    public void SwipeToPrevious()
    {
        int prevIndex = (currentIndex - 1 + panels.Length) % panels.Length;
        ShowPanel(prevIndex);
    }
}
