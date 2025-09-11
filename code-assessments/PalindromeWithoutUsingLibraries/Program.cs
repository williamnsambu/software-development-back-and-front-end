using System;

// Palindrome Checker with Ambiguity Handling
// - Normalizes input to lowercase ASCII alphanumeric characters
// - Treats '0' and 'o', and '1', 'l', 'i' as equivalent (ambiguous matches)
// - Reports first mismatch with debug info

// STEP 1--- INPUT ---
Console.Write("Enter text: ");
string userInput = Console.ReadLine() ?? "";

// STEP 2 --- NORMALIZE (lowercase, strip non-alphanumeric) ---
char[] normalizedBuffer = new char[userInput.Length];

// 2.1 Two-pointer approach to build normalized string
int normalizedLength = 0;

// 2.2 Iterate through input characters
for (int inputIndex = 0; inputIndex < userInput.Length; inputIndex++)
{
    // 2.3. Normalize to lowercase
    char currentChar = char.ToLower(userInput[inputIndex]);

    // 2.4. Check if alphanumeric (ASCII letters and digits only)
    bool isAsciiLetter = currentChar >= 'a' && currentChar <= 'z';
    bool isAsciiDigit = currentChar >= '0' && currentChar <= '9';

    // 2.5. If valid, add to normalized buffer
    if (isAsciiLetter || isAsciiDigit)
    {
        // 2.6. Add to normalized buffer
        normalizedBuffer[normalizedLength++] = currentChar;
    }
}

// 2.7. Create normalized string from buffer
string normalizedString = new string(normalizedBuffer, 0, normalizedLength);

// STEP 3 --- DEBUG OUTPUT ---
// 3.1 Handle empty normalized string (invalid input)
if (normalizedString.Length == 0)
{
    Console.WriteLine("Normalized: \"\"");
    Console.WriteLine("Result: INVALID input (no alphanumeric characters)");
    return;
}

// 3.2 Print normalized string
Console.WriteLine($"Normalized: \"{normalizedString}\"");

// STEP 4 --- PALINDROME CHECK WITH AMBIGUITY HANDLING ---
int leftIndex = 0, rightIndex = normalizedString.Length - 1;
bool isPalindrome = true;
int mismatchLeftIndex = -1, mismatchRightIndex = -1; 
char mismatchLeftChar = '\0', mismatchRightChar = '\0';

// 4.1 Two-pointer scan from both ends
while (leftIndex < rightIndex)
{
    // 4.2 Get characters at both pointers
    char leftChar = normalizedString[leftIndex], rightChar = normalizedString[rightIndex];

    // 4.3 Check for exact or ambiguous matches
    bool isExactMatch = leftChar == rightChar;
    bool isAmbiguousMatch =
        (leftChar == '0' && rightChar == 'o') || (leftChar == 'o' && rightChar == '0') ||
        (leftChar == '1' && (rightChar == 'l' || rightChar == 'i')) ||
        (rightChar == '1' && (leftChar == 'l' || leftChar == 'i')) ||
        (leftChar == 'l' && rightChar == 'i') || (leftChar == 'i' && rightChar == 'l');

    // 4.4 If match, move inward; else record mismatch and break
    if (isExactMatch || isAmbiguousMatch)
    {
        leftIndex++;
        rightIndex--;
    }
    else
    {
        isPalindrome = false;
        mismatchLeftIndex = leftIndex;
        mismatchRightIndex = rightIndex;
        mismatchLeftChar = leftChar;
        mismatchRightChar = rightChar;
        break;
    }
}

// STEP 5 --- OUTPUT RESULT ---
if (isPalindrome)
{
    Console.WriteLine("Result: PALINDROME ✅");
}
else
{
    Console.WriteLine($"Result: NOT palindrome ❌ (first mismatch '{mismatchLeftChar}' at {mismatchLeftIndex} vs '{mismatchRightChar}' at {mismatchRightIndex})");
}

/*
How to explain (briefly):
- Validation: "If nothing remains after cleaning, I treat it as invalid input."
- Transformation: "Lowercase, strip non-alphanumeric characters."
- Ambiguity: "I allow 0~o and 1~l~i equivalence."
- Verification: "Two-pointer scan from both ends."
- Debugging: "Print first mismatch indices and chars."
- Complexity: O(n) time, O(n) space.
*/