using _QuestionAnswersModule.Scripts.Base;

namespace _QuestionAnswersModule.Scripts.SimpleRealization
{
    public class StringAnswer : IAnswer<string>
    {
        private readonly string _data;

        public StringAnswer(string data)
        {
            _data = data;
        }

        public string GetAnswerData()
        {
            return _data;
        }

        public bool IsEqualsTo(IAnswer<string> anotherAnswer)
        {
            return anotherAnswer.GetAnswerData().Equals(_data);
        }
    }
}