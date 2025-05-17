using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class AnkiReader
{
    public static List<Word> Words = new();

    public static void ParseWords(string source)
    {
        Words.Clear();

        //Go over each line
        foreach (var line in source.Split('\n'))
        {
            //Change all spaces to regular spaces
            var normalLine = line.Replace(' ', ' ').Replace('　', ' ').Replace('	', ' ').TrimEnd('\r', '\n');
            var word = new string[3];
            word[0] = "";
            word[1] = "";
            word[2] = "";
            var wordIndex = 0;

            for (var i = 0; i < normalLine.Count(); i++)
            {
                var cCount = normalLine.Count();

                if (i + 1 == cCount)
                {
                    //Final character, we can't look into the... future anymore by doing i+1, just add the character to the current part
                    word[wordIndex] += normalLine[i];
                    break;
                }

                var nextChar = normalLine[i + 1];
                //Word 1: expect kanji first, if next character is an Asian character, add it to the word. If next character is a space, then move onto the second word.
                if (nextChar == ' ')
                {
                    //we're currently still on a character, add it
                    word[wordIndex] += normalLine[i];

                    if (wordIndex == 0)
                        wordIndex = 1;
                    else if (wordIndex == 1)
                        wordIndex = 2;

                    continue;
                }

                //
                if (wordIndex == 2)
                {
                    //just add everything, i.e English text
                    word[wordIndex] += normalLine[i];
                }
                else
                {
                    //Don't add spaces, for wordIndex 0 and wordIndex 1
                    if (normalLine[i] != ' ')
                        word[wordIndex] += normalLine[i];
                }
            }

            var kanji = word[0].TrimStart();
            var hiragana = word[1].TrimStart();
            var meaning = word[2].TrimStart();

            Words.Add(new Word(kanji, hiragana, meaning));
        }

        //Word 2: expect Hiragana, if next character is an Asian character, add it to the word. If next character is a space, then move onto the third word. //can be expanded
        //Word 3: expect English, continue as long as until the line ends (as it might be multiple word including spaces)

        //I know this likely could've been written better
    }


    private static string ParseEnglishPart(string input)
    {
        var output = "";
        var split = input.Split(' ');

        foreach (var item in split)
            if (!CheckIfStringHasAsianCharacter(item))
            {
                if (output.Length > 0)
                    output += " ";
                output += item;
            }

        return output;
    }

    private static string ParseAsianPart(string input)
    {
        var output = "";
        var split = input.Split(' ');

        foreach (var item in split)
            if (CheckIfStringHasAsianCharacter(item))
            {
                if (output.Length > 0)
                    output += " ";
                output += item;
            }

        return output;
    }

    public static bool IsAsian(char character)
    {
        var cat = char.GetUnicodeCategory(character);
        if (cat == UnicodeCategory.OtherLetter)
            return true;
        return false;
    }

    public static bool CheckIfStringHasAsianCharacter(string word)
    {
        if (word.Count() == 0)
            return false;

        var fo = word[0];

        var cat = char.GetUnicodeCategory(fo);
        if (cat == UnicodeCategory.OtherLetter)
            return true;
        return false;
    }
}

public static class StringExtensions
{
    public static string RemoveWhitespace(this string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }
}