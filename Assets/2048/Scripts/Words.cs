using UnityEngine;

public class SimpleWord
{
    public int ID;
    public string Word;

    public SimpleWord(string word, int id)
    {
        Word = word;
        ID = id;
    }
}

public class Word : MonoBehaviour
{
    public string Hiragana;
    public string Kanji;
    public string Meaning;

    public Word(string kanji, string hiragana, string meaning)
    {
        Kanji = kanji;
        Hiragana = hiragana;
        Meaning = meaning;
    }
}