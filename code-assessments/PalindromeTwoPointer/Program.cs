using System;
using System.Globalization;
using System.Text.RegularExpressions;

Console.WriteLine(PalindromeTwoPointer.IsPalindrome("Racecar"));                 // True (default ignores case)
Console.WriteLine(PalindromeTwoPointer.IsPalindrome("A man, a plan, a canal!")); // True (ignores punctuation)
Console.WriteLine(PalindromeTwoPointer.IsPalindrome("Hello"));                   // False
Console.WriteLine(PalindromeTwoPointer.IsPalindrome("!!", ignoreNonUnicodeAlphanumeric: false)); // True (two punctuation chars only)
Console.WriteLine(PalindromeTwoPointer.IsPalindrome("😊a😊", ignoreNonUnicodeAlphanumeric: true));  // False (emoji stripped, becomes "a")
Console.WriteLine(PalindromeTwoPointer.IsPalindrome("😊a😊", ignoreNonUnicodeAlphanumeric: false)); // True (keep emoji)

public static class PalindromeTwoPointer
{
    public static bool IsPalindrome(string input, bool ignoreCase = true, bool ignoreNonUnicodeAlphanumeric = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        int i = 0, j = input.Length - 1;
        while (i < j)
        {
            char left = input[i];
            char right = input[j];

            if (ignoreNonUnicodeAlphanumeric)
            {
                // Advance i until alphanumeric or pointers cross
                if (!IsUnicodeAlphanumeric(left))
                {
                    i++; continue;
                }

                // Retreat j until alphanumeric or pointers cross
                if (!IsUnicodeAlphanumeric(right))
                {
                    j--; continue;
                }
            }

            if (ignoreCase)
            {
                left = char.ToLower(left, CultureInfo.InvariantCulture);
                right = char.ToLower(right, CultureInfo.InvariantCulture);
            }

            if (left != right) return false;
            i++; j--;
        }
        return true;
    }

    // Checks if a character is a Unicode letter or digit (alphanumeric)
    private static bool IsUnicodeAlphanumeric(char c)
    {
        return char.IsLetterOrDigit(c);
    }
}