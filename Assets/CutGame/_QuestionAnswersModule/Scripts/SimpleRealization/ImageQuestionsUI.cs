using System.Collections.Generic;
using _QuestionAnswersModule.Scripts.Base;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace _QuestionAnswersModule.Scripts.SimpleRealization
{
    public class ImageQuestionsUI : MonoBehaviour
    {
        private ImageQuestionData[] _questions;
        
        [Space]
        [SerializeField] private TextMeshProUGUI _questionNameText;
        [SerializeField] private TextMeshProUGUI _questionDescrText;
        [SerializeField] private Image _questionImage;
        [SerializeField] private Transform _answersRoot;
        [SerializeField] private Button _answerButtonPrefab;
        
        private List<Button> _currentButtons;
        private IQuestion<string> _currentQuestion;
        private int _currentQuestionIndex = -1;
        
        private void Awake()
        {
            
            
            Assert.IsNotNull(_questionNameText, "_questionNameText != null");
            Assert.IsNotNull(_questionDescrText, "_questionDescrText != null");
            Assert.IsNotNull(_questionImage, "_questionImage != null");
            Assert.IsNotNull(_answersRoot, "_answersRoot != null");
            Assert.IsNotNull(_answerButtonPrefab, "_answerButtonPrefab != null");

            _currentButtons = new List<Button>(5);
        }

        private void Start()
        {
            _questions = LevelLoader.levelToLoad.QuestionDatas._list.ToArray();
            Assert.IsTrue(_questions.Length > 0, "_questions.Length > 0");
            GoToNextQuestion(LevelLoader.levelToLoad.QuestionDatas._isShuffleQuest);
        }
 
        
        public void GoToNextQuestion(bool rand = false)
        {
            _currentQuestionIndex++;

            if (_currentQuestionIndex > _questions.Length - 1)
            {
                _currentQuestionIndex = 0;
            }

            _currentButtons.ForEach(b => Destroy(b.gameObject));
            _currentButtons.Clear();

            if (rand)
            {
                // key là bộ câu hỏi hiện tại
                var set = LevelLoader.levelToLoad.QuestionDatas;

                // 1) Lấy hoặc khởi tạo list đã hiện
                if (!LevelData.appearedQuestion.TryGetValue(set, out var appearedList))
                {
                    appearedList = new List<int>();
                    LevelData.appearedQuestion[set] = appearedList;
                }

                // 2) Nếu đã thử hết, reset để bắt đầu vòng mới
                if (appearedList.Count >= _questions.Length)
                    appearedList.Clear();

                // 3) Chọn index ngẫu nhiên chưa từng xuất hiện
                int nextIndex;
                do
                {
                    nextIndex = UnityEngine.Random.Range(0, _questions.Length);
                }
                while (appearedList.Contains(nextIndex));

                // 4) Gán và đánh dấu đã xuất hiện
                _currentQuestionIndex = nextIndex;
                appearedList.Add(nextIndex);
            }

            
            var questionData = _questions[_currentQuestionIndex];
            _currentQuestion = questionData.ConvertToQuestion();
            var answers = _currentQuestion.GetAnswers();
            foreach (var answer in answers)
            {
                CreateButtonForAnswer(answer);
            }

            _currentQuestion.OnAnswerFailed += OnAnswerFailed;
            _currentQuestion.OnAnswerSuccess += OnAnswerSuccess;

            _questionNameText.text = _currentQuestion.QuestName;
            _questionDescrText.text = _currentQuestion.QuestDescription;
            _questionImage.sprite = questionData.QuestSprite;
        }

        private void OnAnswerFailed(IAnswer<string> answer)
        {
            Debug.Log("<color=red>You are wrong!</color>");
            
            // Get the current question and answer details
            string questionText = _currentQuestion.QuestName;
            string correctAnswer = _currentQuestion.GetCorrectAnswer().GetAnswerData();
            string userAnswer = answer.GetAnswerData();

            // Handle wrong answer in GameManager
            GameManager.instance.HandleWrongAnswer(questionText, correctAnswer, userAnswer);

            // Close the quiz panel
            GameManager.instance.Quiz.gameObject.SetActive(false);
        }

        private void OnAnswerSuccess(IAnswer<string> answer)
        {
            Debug.Log("<color=green>GOOD JOB!</color>");
            
            GameManager gm = GameManager.instance;
            
            GoToNextQuestion();
            gm.Quiz.gameObject.SetActive(false);
            gm.DeleteVirus();
            
            // Handle correct answer in GameManager
            gm.HandleCorrectAnswer();
        }

        private void CreateButtonForAnswer(IAnswer<string> answer)
        {
            var btnInstance = Instantiate(_answerButtonPrefab, _answersRoot);
            btnInstance.gameObject.SetActive(true);
            var btnText = btnInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = answer.GetAnswerData();
            }
            
            btnInstance.onClick.AddListener(() =>
            {
                _currentQuestion.CheckAnswer(answer);
            });
            
            _currentButtons.Add(btnInstance);
        }
    }
}
