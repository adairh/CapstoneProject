using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace An_An
{
    public class MainMenuUI : MonoBehaviour
    {
        public Transform content;
        public GameObject buttonPrefab;

        [Space] public LevelData levelData;

        public LevelLoader levelLoader;

        [Space] public GameObject mainMenu;

        public GameObject levelGrid;

        [Space] public float fadeSpeed = 0.25f;

        [Space] public GameObject quitButton;

        [Space] public Text resetProgress;

        private GameObject currentView;
        private int resetProgressStage;

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        quitButton.SetActive(false);
#endif
        }

        private void Start()
        {
            InitLevelGrid();

            if (LevelLoader.loadLevelGrid)
            {
                mainMenu.SetActive(false);
                levelGrid.SetActive(true);
                currentView = levelGrid;
            }
            else
            {
                mainMenu.SetActive(true);
                currentView = mainMenu;
            }
        }

        // Initializes level grid for every level
        private void InitLevelGrid()
        {
            var levelCount = levelData.levels.Count;

            for (var i = 0; i < levelCount; i++)
            {
                var go = Instantiate(buttonPrefab, content);

                var btn = go.GetComponent<GridButtonUI>();
                btn.SetUpButton(levelLoader, i, levelData.levels[i]);
            }
        }

        // Switches between "views"
        public void ChangeView(GameObject to)
        {
            AudioManager.Play(SFX.Click);

            FadeToView(currentView, to);
            currentView = to;
        }

        // Makes smooth transition beteween views
        private void FadeToView(GameObject from, GameObject to)
        {
            var fromCanvas = from.GetComponent<CanvasGroup>();
            var toCanvas = to.GetComponent<CanvasGroup>();

            toCanvas.alpha = 0.0f;

            var seq = LeanTween.sequence();
            seq.append(fromCanvas.LeanAlpha(0, fadeSpeed));
            seq.append(() =>
            {
                from.SetActive(false);
                to.SetActive(true);
            });
            seq.append(toCanvas.LeanAlpha(1, fadeSpeed));
        }

        public void ResetProgress()
        {
            switch (resetProgressStage)
            {
                case 0:
                    if (levelData.levels[1].unlocked)
                    {
                        resetProgress.text = "ARE YOU SURE ABOUT THAT?";
                        resetProgressStage++;
                    }
                    else
                    {
                        resetProgress.text = "THERE\'S NOTHING TO RESET YET!";
                    }

                    break;
                case 1:
                    resetProgress.text = "BUT ARE YOU REALLY SURE ABOUT THAT?";
                    resetProgressStage++;
                    break;
                case 2:
                    resetProgress.text = "PRESS IT ONE MORE TIME TO RESET";
                    resetProgressStage++;
                    break;
                case 3:
                    levelData.ResetProgress();
                    levelLoader.LoadMenu(false);
                    break;
            }
        }

        public void QuitGame()
        {
            AudioManager.Play(SFX.Click);

            //Application.Quit();

            SceneManager.LoadScene("MAIN"); // quay ve scene Main
        }
    }
}