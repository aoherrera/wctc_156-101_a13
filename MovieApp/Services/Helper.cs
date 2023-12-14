using System;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MovieApp.Services
{

	public class Helper
	{
        private readonly ILogger<Helper> _logger;

        public Helper(ILogger<Helper> logger)
        {
            _logger = logger;
        }

        //Get a year in YYYY format from a user.
        public string GetYear(string prompt)
        {
            var yearPattern = @"^(19|20)\d{2}$";
            var thisYear = DateTime.UtcNow.Year;

            while (true)
            {
                Console.WriteLine(prompt);
                var userInput = Console.ReadLine();
                int userYear = 0;

                try
                {
                    userYear = Int32.Parse(userInput);

                }
                catch (FormatException e)
                {
                    _logger.LogError(e.Message);
                    Console.WriteLine("\nPlease enter a valid year in the " +
                        "following format: YYYY (e.g., 1999).\n");
                    continue;
                }
                if (Regex.Match(userInput, yearPattern).Success && userYear >= 1900 && (userYear < thisYear + 1))
                {
                    _logger.LogInformation("Valid format received via user input.");
                    return userInput;
                }
                else
                {
                    _logger.LogError($"Invalid format received via user input of {userInput}");
                    Console.WriteLine("\nPlease enter a valid year between 1900 and " +
                        $"{thisYear} in the following format: YYYY (e.g., 1999).\n");
                }
            }

        }

        //Get a US Zip Code in ##### format.
        public string GetZipCode(string prompt)
        {
            var zipcodePattern = @"^\d\d\d\d\d$";
            while (true)
            {
                Console.WriteLine(prompt);
                var userInput = Console.ReadLine().Trim();

                if (Regex.Match(userInput, zipcodePattern).Success)
                {
                    return userInput;
                }
                else
                {
                    _logger.LogError($"Invalid user entry of {userInput}.");
                    Console.WriteLine($"Please enter a valid 5-digit zip code in the following format, where # is a digit: #####");
                }
            }
        }

        //Get a valid gender
        public string GetValidGender(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                string userInput = Console.ReadLine();
                try
                {
                    userInput = Convert.ToString(userInput[0]).ToUpper();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error: {e.Message}");
                    Console.WriteLine("Please enter a valid gender. Enter 'M' for male, 'F' for female, or 'N' for non-binary.");
                    continue;
                }

                string[] genders = {"M", "F", "N"};

                if (genders.Contains(userInput))
                {
                    return userInput;
                }
                else
                {
                    _logger.LogError($"Invalid user entry of {userInput}.");
                    Console.WriteLine("Please enter a valid gender. Enter 'M' for male, 'F' for female, or 'N' for non-binary.");
                    continue;
                }
            }
        }

        //Convert a string into Title Case, in which the first letter of each word is capitalized.
        public string ConvertTitle(string text)
        {
            _logger.LogInformation($"Converting user entry to title case ...");
            string finalText = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i == 0 || text[i - 1] == ' ') //capitalizes first letter in string and any letter following a blank space.
                    finalText += Char.ToUpper(text[i]);
                else
                    finalText += Char.ToLower(text[i]);
            }
            return finalText;
        }

        //Get an integer within a certain range.
        public int GetIntInRange(string prompt, int bottom, int top)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine(prompt);
                    var userNumber = Convert.ToInt32(Console.ReadLine());
                    _logger.LogInformation($"Verifying user integer valid within range of {bottom} - {top} ...");
                    if (userNumber < bottom || userNumber > top)
                    {
                        _logger.LogError($"User integer not within range of {bottom} - {top}");
                        Console.WriteLine($"Please enter a valid number.");
                        continue;
                    }
                    return userNumber;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error: {e.Message}");
                    Console.WriteLine($"Please enter a valid number between {bottom} and {top}.");
                }
            }
        }

        public char YesNo(string prompt)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var userEntry = Console.ReadLine();
                try
                {
                    var userAnswer = Char.ToUpper(Convert.ToChar(userEntry));
                    if (userAnswer == 'Y' || userAnswer == 'N')
                        return userAnswer;
                    else
                        _logger.LogError($"User entry of {userAnswer} is invalid.");
                }
                catch (Exception e)
                {
                    _logger.LogError($"User entry invalid: {e.Message}");
                    Console.WriteLine("You must enter either 'Y' for yes or 'N' for no.");
                }
            }
        }

    }
}