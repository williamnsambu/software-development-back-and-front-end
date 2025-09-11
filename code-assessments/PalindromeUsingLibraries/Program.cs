using System;
using System.Text.RegularExpressions;

Console.WriteLine(Palindrome.IsPalindrome("Racecar"));                 // True (default ignores case)
Console.WriteLine(Palindrome.IsPalindrome("A man, a plan, a canal!")); // True (ignores punctuation)
Console.WriteLine(Palindrome.IsPalindrome("Hello"));                   // False
Console.WriteLine(Palindrome.IsPalindrome("!!", ignoreNonAlnum: false)); // True (two punctuation chars only)
Console.WriteLine(Palindrome.IsPalindrome("😊a😊", ignoreNonAlnum: true));  // False (emoji stripped, becomes "a")
Console.WriteLine(Palindrome.IsPalindrome("😊a😊", ignoreNonAlnum: false)); // True (keep emoji)

public static class Palindrome
{
    // Configurable version: choose whether to ignore case and non-alphanumeric chars
    public static bool IsPalindrome(string input, bool ignoreCase = true, bool ignoreNonAlnum = true)
    {
        if (input == null) return false; // clarify with interviewer if null should be false or throw

        string text = input;
        if (ignoreNonAlnum)
        {
            // \p{L} = any kind of letter in any language; \p{Nd} = decimal digit
            text = Regex.Replace(text, @"[^\p{L}\p{Nd}]", string.Empty);
        }
        if (ignoreCase)
        {
            text = text.ToLowerInvariant();
        }

        // Simple reverse comparison
        var chars = text.ToCharArray();
        Array.Reverse(chars);
        var reversed = new string(chars);
        return text == reversed;
    }
}
