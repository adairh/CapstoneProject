using System.Collections.Generic;
using System.Linq;
using _QuestionAnswersModule.Scripts.Base;
using QuestBase._QuestionAnswersModule.Scripts.Static;
using UnityEngine;

namespace _QuestionAnswersModule.Scripts.SimpleRealization
{
    [CreateAssetMenu(fileName = "QuestionSet", menuName = "QASample/QuestionSet", order = 0)]
    public class QuestionSet : ScriptableObject
    {
        [SerializeField] private string _setName;
        
        [TextArea]
        [SerializeField] private string _setDescription;

        [SerializeField] public List<ImageQuestionData> _list;

        [Space]
        [SerializeField] public bool _isShuffleQuest = true;
         
    }
}