using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public enum GameState
{
    Playing,
    GameOver,
    WaitingForMoveToEnd
}

public enum PlayMode
{
    ClearMergedOnes,
    Infinity,
    SpawnFive,
    TimeAttack
}

public class MatchGameManager : MonoBehaviour
{
    private static MatchGameManager instance;

    [Header("Game State Text")] public Text GameOverText;

    public Text GameOverScoreText;
    public GameObject GameOverPanel;
    public GameObject LoadFilePanel;
    public InputField LoadFilePath;
    public Text LoadFileSource;

    [Header("Feedback UI")] public Text KaomojiText;

    public Text WordsLeftText;
    public Text WordsCorrectText;
    public Button ClearMergedOnesButton;
    public Button InfinityButton;
    public Button SpawnFiveButton;
    public Button TimeAttackButton;
    public Color HighlightedModeColor;
    public Text TimeCounterText;
    public float TimeGiven;
    public AudioClip Confirm;
    public AudioClip Merge;
    public Toggle AskMeaningToggle;
    public Dropdown FilePickerDropDown;

    [Header("Game properties")] public GameState State;

    public PlayMode Mode;
    public bool AskEnglishMeaning;
    public bool UseSoundEffects = true;
    public float Timer = 60;

    [Range(0, 2f)] public float Delay;

    public int SetCount = 4;
    private AudioSource audioSource;

    private readonly Cell[,] Cells = new Cell[4, 4];
    private readonly List<Cell[]> columns = new();

    private List<SimpleWord>
        currentSet = new(); //Contains the first three words randomized, will add the next 3 when finished etc

    private readonly List<Cell> EmptyCells = new();

    private int failedToMergeCombo;

    //
    private bool hasMoveMade;
    private readonly bool[] lineMoveComplete = new bool[4] { true, true, true, true };
    private int numberOfWordsLinked;
    private readonly List<Cell[]> rows = new();
    private int shownWords; //increments one by one as new words get added
    private float timeGiven;
    private int totalWordCount;

    private int
        wordIndex; //current word index when generating words to show on the grid, increments in steps of 4 according to SetCount

    private readonly List<SimpleWord> wordStream = new(); //stays fixed

    public static MatchGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Trying to access static instance while it has not been assigned yet");
                Debug.Break();
            }

            return instance;
        }
    }

    /********** CORE AND BUTTON HANDLERS ********/
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("There's already a Game Manager in the scene, destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        var AllCells = FindObjectsOfType<Cell>();
        foreach (var cell in AllCells)
        {
            cell.CellStyle = 0;
            Cells[cell.RowIndex, cell.ColumnIndex] = cell;
            EmptyCells.Add(cell);
        }

        columns.Add(new[] { Cells[0, 0], Cells[1, 0], Cells[2, 0], Cells[3, 0] });
        columns.Add(new[] { Cells[0, 1], Cells[1, 1], Cells[2, 1], Cells[3, 1] });
        columns.Add(new[] { Cells[0, 2], Cells[1, 2], Cells[2, 2], Cells[3, 2] });
        columns.Add(new[] { Cells[0, 3], Cells[1, 3], Cells[2, 3], Cells[3, 3] });

        rows.Add(new[] { Cells[0, 0], Cells[0, 1], Cells[0, 2], Cells[0, 3] });
        rows.Add(new[] { Cells[1, 0], Cells[1, 1], Cells[1, 2], Cells[1, 3] });
        rows.Add(new[] { Cells[2, 0], Cells[2, 1], Cells[2, 2], Cells[2, 3] });
        rows.Add(new[] { Cells[3, 0], Cells[3, 1], Cells[3, 2], Cells[3, 3] });

        //
        audioSource = GetComponent<AudioSource>();

        //
        ScanForFiles();
        LoadSampleData();

        //
        StartClearMergeMode();
        //ResetGame(); //StartClearMergeMode already contains a call to this
    }

    private void Update()
    {
        if (State == GameState.GameOver)
            return;

        //UI
        WordsCorrectText.text = numberOfWordsLinked.ToString();
        WordsLeftText.text = totalWordCount.ToString(); //complete words 

        if (Mode == PlayMode.TimeAttack)
        {
            TimeGiven -= Time.deltaTime;

            TimeCounterText.text = Mathf.RoundToInt(TimeGiven).ToString();

            if (TimeGiven <= 0)
            {
                TimeGiven = timeGiven;
                GameOver(LocalizationManager.Instance.GetLocalizedValue("GameOverTimeUp"));
            }
        }
    }

    public void NewGameButtonHandler()
    {
        ResetGame();
    }

    public void OpenLoadFilePanel()
    {
        LoadFilePanel.SetActive(true);
        LoadFileSource.text = "";
    }

    public void ToggleMeaning(Toggle change)
    {
        AskEnglishMeaning = change.isOn;

        GenerateWordStream(AnkiReader.Words);
        ResetGame();
    }

    public void ToggleAudio(Toggle change)
    {
        UseSoundEffects = change.isOn;
    }

    public void OptionSelectedFromDropdown(Dropdown dropDown)
    {
        LoadFileFromPath(dropDown.options[dropDown.value].text);

        GenerateWordStream(AnkiReader.Words);
        ResetGame();
    }

    public void ScanForFiles()
    {
        var path = Application.dataPath + "/..";
        var files = Directory.GetFiles(path, "*.txt").ToList();

        var filenames = new List<string>();

        foreach (var file in files) filenames.Add(Path.GetFileName(file));
        FilePickerDropDown.ClearOptions();
        FilePickerDropDown.AddOptions(filenames);
    }

    public void LoadSampleData()
    {
        for (var i = 0; i < FilePickerDropDown.options.Count; i++)
            if (FilePickerDropDown.options[i].text == "sampledata.txt")
                FilePickerDropDown.value = i;
    }

    public void LoadFileFromInput()
    {
        LoadFileFromPath(LoadFilePath.text);
    }

    public void LoadFileFromPath(string filepath)
    {
        var path = filepath;
        path = path.Replace('"', ' ').Trim();

        var reader = new StreamReader(path);
        var text = reader.ReadToEnd();
        reader.Close();

        AnkiReader.ParseWords(text);

        var output = "Parsed words:\n";
        var emptyMeaning = false;

        foreach (var word in AnkiReader.Words)
        {
            output += word.Kanji + ", " + word.Hiragana + ", " + word.Meaning;
            output += "\n";

            if (word.Meaning == "")
                emptyMeaning = true;
        }

        if (emptyMeaning)
        {
            output += "No word meanings detected (3rd column in text file). Starting the game without asking them";
            AskEnglishMeaning = false;
            AskMeaningToggle.isOn = false;
            AskMeaningToggle.gameObject.SetActive(false);
        }
        else
        {
            AskMeaningToggle.gameObject.SetActive(true);
            AskMeaningToggle.isOn = true;
        }

        LoadFileSource.text = output;

        GenerateWordStream(AnkiReader.Words);
        ResetGame();
    }

    public void CloseLoadFilePanel()
    {
        LoadFilePanel.SetActive(false);
    }

    public void GameOver(string gameOverText)
    {
        State = GameState.GameOver;
        GameOverText.text = gameOverText + "\n" + LocalizationManager.Instance.GetLocalizedValue("YouScored");
        GameOverScoreText.text = numberOfWordsLinked.ToString();
        GameOverPanel.SetActive(true);
    }

    public void ResetGame()
    {
        foreach (var cell in Cells)
            cell.CellStyle = 0;

        wordIndex = 0; //increments in steps of 4 (SetCount)
        shownWords = 0; //increments one by one as new words get added
        numberOfWordsLinked = 0;
        currentSet.Clear();

        Debug.Log("> Resetting the game.");

        UpdateEmptyCells();

        GenerateNewCell(2);
        GenerateNewCell(8);
        GenerateNewCell(12);
        GenerateNewCell(7);

        if (Mode == PlayMode.SpawnFive)
        {
            GenerateNewCell();
            GenerateNewCell();
        }

        //reset timer (set in WordStream function
        TimeGiven = timeGiven;
        TimeCounterText.text = Mathf.RoundToInt(TimeGiven).ToString();

        GameOverPanel.SetActive(false);
        State = GameState.Playing;
    }

    public void StartClearMergeMode()
    {
        Mode = PlayMode.ClearMergedOnes;

        ClearMergedOnesButton.GetComponent<Image>().color = HighlightedModeColor;

        //reset all other colors
        InfinityButton.GetComponent<Image>().color = Color.white;
        TimeAttackButton.GetComponent<Image>().color = Color.white;
        SpawnFiveButton.GetComponent<Image>().color = Color.white;

        ResetGame();
    }

    public void StartInfinityMode()
    {
        Mode = PlayMode.Infinity;

        InfinityButton.GetComponent<Image>().color = HighlightedModeColor;

        //reset all other colors
        ClearMergedOnesButton.GetComponent<Image>().color = Color.white;
        TimeAttackButton.GetComponent<Image>().color = Color.white;
        SpawnFiveButton.GetComponent<Image>().color = Color.white;

        ResetGame();
    }

    public void StartSpawnFiveMode()
    {
        Mode = PlayMode.SpawnFive;

        SpawnFiveButton.GetComponent<Image>().color = HighlightedModeColor;

        //reset all other colors
        InfinityButton.GetComponent<Image>().color = Color.white;
        TimeAttackButton.GetComponent<Image>().color = Color.white;
        ClearMergedOnesButton.GetComponent<Image>().color = Color.white;

        ResetGame();
    }

    public void StartTimeAttackMode()
    {
        Mode = PlayMode.TimeAttack;

        SpawnFiveButton.GetComponent<Image>().color = Color.white;
        InfinityButton.GetComponent<Image>().color = Color.white;
        TimeAttackButton.GetComponent<Image>().color = HighlightedModeColor;
        ClearMergedOnesButton.GetComponent<Image>().color = Color.white;

        TimeCounterText.gameObject.SetActive(true);
        ResetGame();
    }

    public void LoadLanguageSelectScene()
    {
        LocalizationManager.Instance.Reset();
        SceneManager.LoadScene("LanguageSelectScene");
    }

    /********** WORD HANDLING ********/

    private List<Word> AddWords()
    {
        var wordList = new List<Word>();
        wordList.Add(new Word("人", "ひと", "Person"));
        wordList.Add(new Word("山", "やま", "Mountain"));
        wordList.Add(new Word("花", "はな", "Flower"));
        wordList.Add(new Word("本", "ほん", "Book"));
        wordList.Add(new Word("大きい", "おおきい", "To be big"));
        wordList.Add(new Word("行く", "いく", "To go"));
        wordList.Add(new Word("読む", "よむ", "To read"));
        wordList.Add(new Word("寝る", "ねる", "To sleep"));

        return wordList;
    }

    private void GenerateWordStream(List<Word> words)
    {
        wordStream.Clear();
        TimeGiven = 0;

        var id = 0;
        foreach (var word in words)
        {
            if (word.Kanji != " ")
                wordStream.Add(new SimpleWord(word.Kanji, id));

            if (word.Hiragana != " ")
                wordStream.Add(new SimpleWord(word.Hiragana, id));


            id++;
            TimeGiven += 2;
        }

        if (AskEnglishMeaning)
            foreach (var word in words)
            {
                wordStream.Add(new SimpleWord(word.Kanji, id));
                wordStream.Add(new SimpleWord(word.Meaning, id));

                id++;
                TimeGiven += 2;
            }

        //backup that won't be modified
        timeGiven = TimeGiven;
        totalWordCount = words.Count;
        TimeCounterText.text = Mathf.RoundToInt(TimeGiven).ToString();
    }

    private SimpleWord GetNextWord()
    {
        //Reset the list if we're at the end and in infinity mode
        if ((Mode == PlayMode.Infinity || Mode == PlayMode.TimeAttack) && wordIndex >= wordStream.Count)
        {
            wordIndex = 0;
            currentSet.Clear();
        }

        //Working in smaller sets makes sure that when dealing with large lists of words, a related word will be spawned soon.
        if (currentSet.Count == 0)
        {
            //Take 4 (SetCount public variable) starting at the right position
            currentSet = wordStream.Skip(wordIndex).Take(SetCount).ToList();

            wordIndex += SetCount;

            if (wordIndex >= wordStream.Count)
                return null;

            //randomize
            var rnd = new Random();
            currentSet.OrderBy(item => rnd.Next()).ToList();

            var debugline = "New set: ";
            foreach (var item in currentSet) debugline += item.Word + ". ";
            Debug.Log(debugline);
        }

        var w = currentSet[0];
        currentSet.RemoveAt(0);
        return w;
    }

    //Todo: make sure the first two words don't spawn on the same line.
    private void GenerateNewCell(int index = -1)
    {
        var word = GetNextWord();
        if (EmptyCells.Count > 0 && word != null)
        {
            shownWords++;
            var newCellIndex = index;

            //if we don't specify a position, generate one randomly
            if (index == -1)
                newCellIndex = UnityEngine.Random.Range(0, EmptyCells.Count);

            //random
            // int randomNum = UnityEngine.Random.Range(0, 10); //0-9
            // if(randomNum == 0)
            //     EmptyCells[newNumberIndex].Number = 2;

            EmptyCells[newCellIndex].SetText(word, 2);

            //
            EmptyCells[newCellIndex].PlayAppearAnimation();
            Debug.Log("Adding " + word.Word + " on the board.");
            EmptyCells.RemoveAt(newCellIndex);
        }

        if (word == null && GetActiveCellCount() == 0)
            //You won
            GameOver("You won!");
    }

    private int GetActiveCellCount()
    {
        var count = 0;
        foreach (var cell in Cells)
            if (cell.CellStyle > 0)
                count++;

        return count;
    }

    private bool IsWon()
    {
        return false;
    }

    private void UpdateEmptyCells()
    {
        EmptyCells.Clear();
        foreach (var cell in Cells)
            if (cell.CellStyle == 0)
                EmptyCells.Add(cell);
    }

    /********** MOVING AND MERGING TILES ********/

    private bool HasMovesLeftWhenBoardIsFull()
    {
        if (EmptyCells.Count > 0) return true;

        //check column 
        for (var i = 0; i < columns.Count; i++)
        for (var j = 0; j < rows.Count - 1; j++)
            if (Cells[j, i].CellStyle != 0 && Cells[j + 1, i].CellStyle != 0 &&
                Cells[j, i].Word.ID == Cells[j + 1, i].Word.ID)
                return true;

        //check rows
        for (var i = 0; i < rows.Count; i++)
        for (var j = 0; j < columns.Count - 1; j++)
            if (Cells[i, j].CellStyle != 0 && Cells[i, j + 1].CellStyle != 0 &&
                Cells[i, j].Word.ID == Cells[i, j + 1].Word.ID)
                return true;
        return false;
    }

    public void Move(MoveDirection direction)
    {
        if (State == GameState.WaitingForMoveToEnd)
            return;

        hasMoveMade = false;
        ResetMergeFlags();

        if (Delay > 0)
        {
            StartCoroutine(MoveCoroutine(direction));
        }
        else
        {
            //
            for (var i = 0; i < rows.Count; i++)
                switch (direction)
                {
                    case MoveDirection.Down:
                        while (MakeOneMoveUpIndex(columns[i]))
                        {
                        }

                        break;
                    case MoveDirection.Left:
                        while (MakeOneMoveDownIndex(rows[i]))
                        {
                        }

                        break;
                    case MoveDirection.Right:
                        while (MakeOneMoveUpIndex(rows[i]))
                        {
                        }

                        break;
                    case MoveDirection.Up:
                        while (MakeOneMoveDownIndex(columns[i]))
                        {
                        }

                        break;
                }

            UpdateEmptyCells();
            GenerateNewCell();

            HandleGameOvers();
        }
    }


    private IEnumerator MoveOneLineUpIndexCoroutine(Cell[] cells, int index)
    {
        lineMoveComplete[index] = false;
        while (MakeOneMoveUpIndex(cells))
        {
            hasMoveMade = true;
            yield return new WaitForSeconds(Delay);
        }

        lineMoveComplete[index] = true;
    }

    private IEnumerator MoveOneLineDownIndexCoroutine(Cell[] cells, int index)
    {
        lineMoveComplete[index] = false;
        while (MakeOneMoveDownIndex(cells))
        {
            hasMoveMade = true;
            yield return new WaitForSeconds(Delay);
        }

        lineMoveComplete[index] = true;
    }

    private IEnumerator MoveCoroutine(MoveDirection direction)
    {
        State = GameState.WaitingForMoveToEnd;

        //start moving each line with a delay
        switch (direction)
        {
            case MoveDirection.Up:
                for (var i = 0; i < columns.Count; i++)
                    StartCoroutine(MoveOneLineDownIndexCoroutine(columns[i], i));
                break;
            case MoveDirection.Left:
                for (var i = 0; i < rows.Count; i++)
                    StartCoroutine(MoveOneLineDownIndexCoroutine(rows[i], i));
                break;
            case MoveDirection.Down:
                for (var i = 0; i < columns.Count; i++)
                    StartCoroutine(MoveOneLineUpIndexCoroutine(columns[i], i));
                break;
            case MoveDirection.Right:
                for (var i = 0; i < rows.Count; i++)
                    StartCoroutine(MoveOneLineUpIndexCoroutine(rows[i], i));
                break;
        }

        while (!(lineMoveComplete[0] && lineMoveComplete[1] && lineMoveComplete[2] && lineMoveComplete[3]))
            yield return null;

        //Spawn new cells when needed
        if (hasMoveMade)
        {
            UpdateEmptyCells();

            if (UseSoundEffects)
                audioSource.PlayOneShot(Confirm);

            if (Mode == PlayMode.SpawnFive)
            {
                var toSpawn = Mathf.Max(0, 6 - GetActiveCellCount());
                for (var i = 0; i < toSpawn; i++)
                    GenerateNewCell();
            }
            else
            {
                GenerateNewCell();

                if (GetActiveCellCount() == 1)
                    GenerateNewCell();

                if (GetActiveCellCount() == 2)
                    GenerateNewCell();
            }

            HandleGameOvers();
        }
        else
        {
            Debug.Log("No move made");
        }

        KaomojiText.fontSize = UnityEngine.Random.Range(24, 120);

        //
        State = GameState.Playing;
    }

    private void HandleGameOvers()
    {
        //board full
        if (!HasMovesLeftWhenBoardIsFull())
            GameOver("Game Over");
    }

    private bool MakeOneMoveDownIndex(Cell[] line)
    {
        for (var i = 0; i < line.Length - 1; i++)
        {
            //Move
            if (line[i].CellStyle == 0 && line[i + 1].CellStyle != 0)
            {
                line[i].Word = line[i + 1].Word;
                line[i].CellStyle = line[i + 1].CellStyle;

                line[i + 1].CellStyle = 0;
                return true;
            }

            if (HandleMerge(line[i], line[i + 1]))
                return true;
        }

        return false;
    }

    private bool MakeOneMoveUpIndex(Cell[] line)
    {
        for (var i = line.Length - 1; i > 0; i--)
        {
            //Move
            if (line[i].CellStyle == 0 && line[i - 1].CellStyle != 0)
            {
                line[i].Word = line[i - 1].Word;
                line[i].CellStyle = line[i - 1].CellStyle;
                line[i - 1].CellStyle = 0;
                return true;
            }

            if (HandleMerge(line[i], line[i - 1]))
                return true;
        }

        return false;
    }

    private bool HandleMerge(Cell One, Cell Two)
    {
        if (One.Word == null || Two.Word == null)
            return false;

        var score = 0;
        //Merge (down and left moves)
        if (One.CellStyle != 0 && Two.CellStyle != 0 && One.Word.ID == Two.Word.ID && !One.HasMerged && !Two.HasMerged)
        {
            Two.CellStyle = 0;

            One.HasMerged = true;
            One.CellStyle = 0;
            score = 2;

            One.PlayMergeAnimation();

            Debug.Log("Removing " + One.Word.Word + " after merging with " + Two.Word.Word);

            //
            Two.PlayMergeAnimation();

            if (UseSoundEffects)
                audioSource.PlayOneShot(Merge);

            // 
            ScoreTracker.Instance.Score += score;

            failedToMergeCombo = 0;
            KaomojiText.text = "";

            numberOfWordsLinked++;

            return true;
        }

        failedToMergeCombo++;

        if (failedToMergeCombo > 55)
            KaomojiText.text = "	( ; ω ; )";
        else
            KaomojiText.text = "";

        return false;
    }

    private void ResetMergeFlags()
    {
        foreach (var cell in Cells)
            cell.HasMerged = false;
    }
}