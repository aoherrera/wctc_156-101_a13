using MovieLibraryEntities.Models;
using System;
using System.Linq;
using MovieLibraryEntities.Dao;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MovieApp.Services;

public class MainService : IMainService
{
    private readonly IRepository _repository;
    private readonly ILogger<MainService> _logger;
    private readonly Helper _helper;

    public MainService(IRepository repository, Helper helper, ILogger<MainService> logger)
    {
        _repository = repository;
        _helper = helper;
        _logger = logger;
    }

    public void Invoke()
    {
        string choice;
        int choice_num = 0;
        do
        {
            Console.WriteLine("What action would you like to perform?");
            Console.WriteLine("\n01.) Search for movie by title\n02.) Search for movie by release year\n03.) Add a movie\n04.) List movies\n05.) Update movie\n06.) Delete movie\n07.) Add User\n08.) Rate a movie\n09.) See top movie by occupation or gender\n10.) Exit\n");
            choice = Console.ReadLine();
            try
            {
                choice_num = Convert.ToInt32(choice);
            }
            catch (FormatException e)
            {
                _logger.LogError($"Error: an invalid value has been entered. {e.Message}");
                Console.WriteLine($"Error: You must enter a valid selection");
                continue;
            }
            if (choice_num == 10)
                break;
            else if (choice_num == 1)
            {
                {
                    string userMovie;
                    do
                    {
                        Console.WriteLine("Search a movie title: ");
                        userMovie = Console.ReadLine();
                        if (String.IsNullOrWhiteSpace(userMovie))
                        {
                            _logger.LogError("User did not enter a value.");
                            Console.WriteLine("Error: You must enter a valid movie title.");
                            break;
                        }
                        else
                        {
                            var movies = _repository.Search(userMovie);
                            if (movies.Any())
                            {
                                for (int i = 0; i < movies.Count(); i++)
                                {
                                    var movieGenres = movies.ElementAt(i)?.MovieGenres ?? new List<MovieGenre>();

                                    if (i <= (movies.Count() - 1))
                                    {
                                        _logger.LogInformation("Entry found.");
                                        Console.WriteLine($"\nMovie ID: {movies.ElementAt(i).Id}");
                                        Console.WriteLine($"Title (Release Year): {movies.ElementAt(i).Title}");
                                    }
                                    else if (i == (movies.Count() - 1) && movieGenres.Count == 0)
                                    {
                                        _logger.LogInformation("Entry found.");
                                        Console.WriteLine($"\nMovie ID: {movies.ElementAt(i).Id}");
                                        Console.WriteLine($"Title (Release Year): {movies.ElementAt(i).Title}\n");
                                    }

                                    //display genre(s)
                                    if (movieGenres.Count() != 0)
                                    {
                                        Console.Write("Genre(s): ");
                                        for (int j = 0; j < movieGenres.Count(); j++)
                                        {
                                            if (j != (movieGenres.Count() - 1))
                                            {
                                                Console.Write($"{movieGenres.ElementAt(j).Genre.Name}, ");
                                            }
                                            else
                                                Console.WriteLine($"{movieGenres.ElementAt(j).Genre.Name}\n");
                                        }
                                    }

                                }
                            }
                            else
                            {
                                _logger.LogInformation($"No entries matched search criteria of '{userMovie}'.");
                                Console.WriteLine("\nNo movies matched the text entered.");
                                Console.WriteLine("\nNote: you may only search by movie title.\nIf you would like to search by release year, please select another option.\n");
                            }
                        }
                    } while (String.IsNullOrWhiteSpace(userMovie));
                }
            }
            else if (choice_num == 2)
            {
                string userYear;
                do
                {
                    userYear = _helper.GetYear("Enter the movie release year to search.");
                    int releaseYear;
                    try
                    {
                        releaseYear = Convert.ToInt32(userYear);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error: a valid year was not entered {e.Message}");
                        Console.WriteLine("Error: You must enter a valid year.");
                        break;
                    }
                    var movies = _repository.SearchByReleaseYear(releaseYear);
                    if (movies.Any())
                    {
                        foreach (var movie in movies)
                        {
                            _logger.LogInformation($"Entry Found.");
                            _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        _logger.LogInformation($"No entries matched search criteria of {releaseYear}");
                        Console.WriteLine("\nNo movies matched the year entered.");
                        Console.WriteLine("\nNote: you may only search by movie release year.\nIf you would like to search by movie title, please select another option.\n");
                    }
                } while (String.IsNullOrWhiteSpace(userYear));
            }
            else if (choice_num == 3)
            {
                string userMovie;
                do
                {
                    Console.WriteLine("Please enter the movie title:");
                    userMovie = _helper.ConvertTitle(Console.ReadLine());
                    if (String.IsNullOrWhiteSpace(userMovie))
                    {
                        _logger.LogError("User entry empty.");
                        Console.WriteLine("Error: You must enter a valid movie title.\n");
                        break;
                    }
                    else
                    {
                        var releaseYear = _helper.GetYear("Please enter the release year:");

                        var releaseMonth = _helper.GetIntInRange("Please enter the month (number) the movie was released", 1, 12);

                        int releaseDay;

                        //allow for up to 29 days for February.
                        if (releaseMonth == 2)
                            releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 29);
                        //allow for up to 30 days for months of April, June, September and November.
                        else if (releaseMonth == 4 || releaseMonth == 6 || releaseMonth == 9 || releaseMonth == 11)
                            releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 30);
                        //allow up to 31 days for the other months.
                        else
                            releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 31);

                        string movieRelease = releaseYear + "-" + Convert.ToString(releaseMonth) + "-" + Convert.ToString(releaseDay);
                        var movieReleaseDate = Convert.ToDateTime(movieRelease);

                        //add genre(s) to movie.
                        var addGenre = _helper.YesNo($"Would you like to enter genre(s) for {userMovie} (Y/N)?");
                        if (addGenre == 'N')
                        {
                            _repository.AddMovie(userMovie, movieReleaseDate, releaseYear, null);
                        }
                        else
                        {
                            List<int> userGenres = new List<int>();

                            var genres = _repository.GetGenres();
                            foreach (var genre in genres)
                            {
                                if (genre.Id >= 10)
                                {
                                    Console.WriteLine($"ID: {genre.Id} Genre: {genre.Name}");
                                }
                                else
                                    Console.WriteLine($"ID: 0{genre.Id} Genre: {genre.Name}");
                            }
                            var userGenre = _helper.GetIntInRange("\nPlease enter the ID for the genre you would like to add:", (int)genres.Min(x => x.Id), (int)genres.Max(x => x.Id));
                            userGenres.Add(userGenre);

                            char addAnotherGenre;
                            do
                            {
                                addAnotherGenre = _helper.YesNo("Would you like to add another genre (Y/N)?");
                                if (addAnotherGenre == 'Y')
                                {
                                    userGenre = _helper.GetIntInRange("\nPlease enter the ID for the genre you would like to add:", (int)genres.Min(x => x.Id), (int)genres.Max(x => x.Id));
                                    if (userGenres.Contains(userGenre))
                                    {
                                        var duplicateGenre = _repository.GetGenres().FirstOrDefault(x => x.Id == userGenre);
                                        Console.WriteLine($"Error: Genre ID {duplicateGenre.Id} - {duplicateGenre.Name} is already associated with this movie.");
                                        continue;
                                    }

                                    userGenres.Add(userGenre);
                                }
                                else
                                {
                                    _repository.AddMovie(userMovie, movieReleaseDate, releaseYear, userGenres);
                                    break;
                                }
                            } while (addAnotherGenre == 'Y');
                        }


                    }
                } while (String.IsNullOrWhiteSpace(userMovie));

            }
            else if (choice_num == 4)
            {
                var seeAll = _helper.YesNo("Would you like to view all movie records (Y/N)?");
                if (seeAll == 'Y')
                {
                    var movies = _repository.GetAll();
                    foreach (var movie in movies)
                    {
                        _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                    }
                    Console.WriteLine();
                }
                else
                {
                    var amountToSee = _helper.GetIntInRange("How many movie records would you like to see (sorted alphabetically)?", 1, _repository.GetAll().Count());
                    var movies = _repository.GetTopMovies(amountToSee);
                    var counter = movies.Count();
                    foreach (var movie in movies)
                    {
                        _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                    }
                    Console.WriteLine();
                }
            }
            else if (choice_num == 5)
            {
                var knowsID = _helper.YesNo("Do you know the movie ID for the movie ID you would like to update (Y/N)?");
                while (knowsID == 'N')
                {
                    Console.WriteLine("Search a movie title: ");
                    var user_movie = Console.ReadLine().ToUpper();
                    var movies = _repository.Search(user_movie);

                    if (movies.Any())
                    {
                        foreach (var movie in movies)
                        {
                            _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                        }
                        Console.WriteLine();

                        var searchAgain = _helper.YesNo("\nWould you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            knowsID = 'Y';
                    }
                    else
                    {
                        Console.WriteLine("No movies matched the text entered.");
                        var searchAgain = _helper.YesNo("\nWould you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            break;
                    }
                }

                //flag variable to check if movie has been updated.
                bool movieUpdated = false;
                do
                {
                    Console.WriteLine("\nEnter the movie ID for the movie you woud like to update.");
                    var userMovie = Console.ReadLine();
                    int movieID;
                    try
                    {
                        movieID = Convert.ToInt32(userMovie);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Error: you must enter a valid ID.");
                        break;
                    }
                    if (_repository.GetValidMovie(movieID) != true)
                    {
                        Console.WriteLine("Error: You must enter a valid movie ID.");
                        break;
                    }
                    int changeChoice;
                    _repository.DisplayMovieDetails(movieID);
                    Console.WriteLine();
                    do
                    {
                        Console.WriteLine("Which element of the selected movie would you like to update?");
                        changeChoice = _helper.GetIntInRange("\n01.) Movie Title\n02.) Release Year\n03.) Release Date\n04.) Genre(s)\n05.) Never mind", 1, 5);
                        if (changeChoice == 5)
                        {
                            //set movieUpdated flag to true to break out of
                            //update workflow completely.
                            movieUpdated = true;
                            break;
                        }
                        //update movie title
                        else if (changeChoice == 1)
                        {
                            Console.WriteLine("Please enter the new movie title:");
                            var newTitle = Console.ReadLine();
                            if (String.IsNullOrWhiteSpace(newTitle))
                            {
                                Console.WriteLine("Error: You must enter a valid movie title:");
                                //set movieUpdated flag to true to break out of
                                //update workflow completely.
                                movieUpdated = true;
                                break;
                            }
                            movieUpdated = _repository.UpdateMovieTitle(movieID, _helper.ConvertTitle(newTitle));
                        }
                        //update movie release year
                        else if (changeChoice == 2)
                        {
                            var newYear = _helper.GetYear("Please enter the new release year.");
                            movieUpdated = _repository.UpdateMovieReleaseYear(movieID, Convert.ToInt32(newYear));
                        }
                        else if (changeChoice == 3)
                        {
                            var releaseYear = _helper.GetYear("Please enter the release year:");

                            var releaseMonth = _helper.GetIntInRange("Please enter the month (number) the movie was released", 1, 12);

                            int releaseDay;

                            //allow for up to 29 days for February.
                            if (releaseMonth == 2)
                                releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 29);
                            //allow for up to 30 days for months of April, June, September and November.
                            else if (releaseMonth == 4 || releaseMonth == 6 || releaseMonth == 9 || releaseMonth == 11)
                                releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 30);
                            //allow up to 31 days for the other months.
                            else
                                releaseDay = _helper.GetIntInRange("Please enter the day the movie was released", 1, 31);

                            string movieRelease = releaseYear + "-" + Convert.ToString(releaseMonth) + "-" + Convert.ToString(releaseDay);
                            var movieReleaseDate = Convert.ToDateTime(movieRelease);
                            movieUpdated = _repository.UpdateMovieReleaseDate(movieID, movieReleaseDate);
                        }
                        else if (changeChoice == 4)
                        {
                            int genreChoice;
                            do
                            {
                                Console.WriteLine("\nWould you like to add or remove a genre?");
                                genreChoice = _helper.GetIntInRange("01.) Add Genre\n02.) Remove Genre\n03.) Never mind", 1, 3);
                                if (genreChoice == 3)
                                {
                                    //kick user out of entire update worklow.
                                    movieUpdated = true;
                                    break;
                                }
                                else if (genreChoice == 1)
                                {
                                    List<int> userGenres = new List<int>();

                                    var genres = _repository.GetGenres();
                                    foreach (var genre in genres)
                                    {
                                        if (genre.Id <= 9)
                                        {
                                            Console.WriteLine($"ID: 0{genre.Id} Genre: {genre.Name}");
                                        }
                                        else
                                            Console.WriteLine($"ID: {genre.Id} Genre: {genre.Name}");
                                    }
                                    var userGenre = _helper.GetIntInRange("\nPlease enter the ID for the genre you would like to add:", (int)genres.Min(x => x.Id), (int)genres.Max(x => x.Id));
                                    if (userGenres.Contains(userGenre) || _repository.SearchByID(movieID).MovieGenres.Any(x => x.Genre == _repository.GetGenres().FirstOrDefault(x => x.Id == userGenre)))
                                    {
                                        var duplicateGenre = _repository.GetGenres().FirstOrDefault(x => x.Id == userGenre);
                                        Console.WriteLine($"Error: Genre ID {duplicateGenre.Id} - {duplicateGenre.Name} is already associated with this movie.");
                                        continue;
                                    }
                                    userGenres.Add(userGenre);

                                    char addAnotherGenre;
                                    do
                                    {
                                        addAnotherGenre = _helper.YesNo("\nWould you like to add another genre (Y/N)?");
                                        if (addAnotherGenre == 'Y')
                                        {
                                            userGenre = _helper.GetIntInRange("\nPlease enter the ID for the genre you would like to add:", (int)genres.Min(x => x.Id), (int)genres.Max(x => x.Id));
                                            if (userGenres.Contains(userGenre) || _repository.SearchByID(movieID).MovieGenres.Any(x => x.Genre == _repository.GetGenres().FirstOrDefault(x => x.Id == userGenre)))
                                            {
                                                var duplicateGenre = _repository.GetGenres().FirstOrDefault(x => x.Id == userGenre);
                                                Console.WriteLine($"Error: Genre ID {duplicateGenre.Id} - {duplicateGenre.Name} is already associated with this movie.");
                                                continue;
                                            }
                                            userGenres.Add(userGenre);
                                        }
                                        else
                                        {
                                            _repository.AddMovieGenres(movieID, userGenres);
                                            movieUpdated = true;
                                            genreChoice = 3;
                                            break;
                                        }
                                    } while (addAnotherGenre == 'Y');
                                }
                                else if (genreChoice == 2)
                                {
                                    bool userDelete = false;
                                    do
                                    {
                                        var movie = _repository.SearchByID(movieID);
                                        var currentGenres = movie.MovieGenres ?? new List<MovieGenre>();
                                        if (!currentGenres.Any())
                                        {
                                            Console.WriteLine("Error: there are no genres associated with this movie.");
                                            movieUpdated = true;
                                            break;
                                        }
                                        else if (currentGenres.Any())
                                        {
                                            //create a menu of the current genres to display to the user. Allow the user to quit this workflow.
                                            //this option should be ICollection<MovieGenre>.Count() + 1 
                                            string menuCurrentGenres = "";

                                            for (int i = 0; i < currentGenres.Count(); i++)
                                            {
                                                menuCurrentGenres += $"{i + 1}.) {currentGenres.ElementAt(i).Genre.Name}\n";
                                                if (i == currentGenres.Count() - 1)
                                                {
                                                    menuCurrentGenres += $"{i + 2}.) Never mind\n";
                                                }
                                            }
                                            Console.WriteLine("\nWhich genre would you like to delete?");
                                            var deleteGenre = _helper.GetIntInRange(menuCurrentGenres, 1, movie.MovieGenres.Count() + 1);
                                            if (deleteGenre == currentGenres.Count() + 1)
                                            {
                                                userDelete = true;
                                                //kick user out of update workflow.
                                                movieUpdated = true;
                                                break;
                                            }
                                            var userVerification = _helper.YesNo($"Are you sure you want to delete {currentGenres.ElementAt(deleteGenre - 1).Genre.Name} (Y/N)?");
                                            if (userVerification == 'Y')
                                            {
                                                movieUpdated = _repository.DeleteMovieGenre(Convert.ToInt32(movie.Id), deleteGenre);
                                            }

                                            else if (userVerification == 'N')
                                            {
                                                userDelete = true;
                                                genreChoice = 3;
                                                break;
                                            }
                                            if (currentGenres.Any())
                                            {
                                                var deleteAnother = _helper.YesNo("Would you like to delete another genre (Y/N)?");
                                                if (deleteAnother == 'Y')
                                                {
                                                    userDelete = false;
                                                    continue;
                                                }
                                                else
                                                {
                                                    genreChoice = 3;
                                                    movieUpdated = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                userDelete = true;
                                                genreChoice = 3;
                                                break;
                                            }
                                        }

                                    } while (!userDelete);
                                }

                            } while (genreChoice != 3);
                        }
                        //display changes to user.
                        _repository.DisplayMovieDetails(movieID);
                        Console.WriteLine();

                        var editAnother = _helper.YesNo("Would you like to edit another element (Y/N)?");
                        if (editAnother == 'Y')
                        {
                            movieUpdated = false;
                            continue;
                        }
                        else if (editAnother == 'N')
                        {
                            break;
                        }
                    } while (changeChoice != 5);

                } while (movieUpdated == false);
            }

            else if (choice_num == 6)
            {
                var knowsID = _helper.YesNo("Do you know the ID of the movie you would like to delete (Y/N)?");
                while (knowsID == 'N')
                {
                    Console.WriteLine("Search a movie title: ");
                    var user_movie = Console.ReadLine().ToUpper();
                    var movies = _repository.Search(user_movie);

                    if (movies.Any())
                    {
                        foreach (var movie in movies)
                        {
                            _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                        }
                        Console.WriteLine();
                        var searchAgain = _helper.YesNo("Would you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            knowsID = 'Y';
                    }
                    else
                    {
                        Console.WriteLine("No movies matched the text entered.");
                        var searchAgain = _helper.YesNo("\nWould you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            break;
                    }

                    //tracks if the user has changed their mind regarding the erasure of a movie.
                    bool stopDeletionWorkflow = false;
                    do
                    {
                        Console.WriteLine("Enter the movie ID for the movie you woud like to delete.");
                        var userMovie = Console.ReadLine();
                        int movieID;
                        try
                        {
                            movieID = Convert.ToInt32(userMovie);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Error: you must enter a valid ID.");
                            //kick user out of erasure workflow.
                            stopDeletionWorkflow = true;
                            break;
                        }
                        if (_repository.GetValidMovie(movieID) != true)
                        {
                            Console.WriteLine("Error: You must enter a valid movie ID.");
                            //kick user out of erasure workflow.
                            stopDeletionWorkflow = true;
                            break;
                        }
                        var movieToDelete = _repository.SearchByID(movieID);
                        var verifyDelete = _helper.YesNo($"\nAre you sure you want to delete {movieToDelete.Title} (Y/N)?");
                        if (verifyDelete == 'N')
                        {
                            Console.WriteLine("No changes have been made to the database.");
                            stopDeletionWorkflow = true;
                            break;
                        }
                        else if (verifyDelete == 'Y')
                        {
                            _repository.DeleteMovie(movieID);
                            Console.WriteLine($"{movieToDelete.Title} deleted from database.");
                            stopDeletionWorkflow = true;
                        }
                    } while (stopDeletionWorkflow == false);
                }
            }
            else if (choice_num == 7)
            {
                var userAge = _helper.GetIntInRange("Enter the user's age:", 0, 120);
                var userZipCode = _helper.GetZipCode("\nEnter the user's zip code:");
                var userGender = _helper.GetValidGender("\nEnter the user's gender:");
                Console.WriteLine("\nA user may have one of the following occupations:\n");
                var validOccupations = _repository.GetUserOccupations();
                foreach(var validOccupation in validOccupations)
                {
                    if (validOccupation.Id < 10)
                    {
                        Console.WriteLine($"0{validOccupation.Id}. {validOccupation.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"{validOccupation.Id}. {validOccupation.Name}");
                    }
                }
                var userOccupation = _helper.GetIntInRange("\nEnter the ID for the new user's occupation:", 1, validOccupations.Count());

                var newUser = _repository.AddUser(userAge, userGender, userZipCode, userOccupation);

                Console.WriteLine("\nUser added to the database:\n");
                Console.WriteLine($"User ID: {newUser.Id}");
                Console.WriteLine($"Age: {newUser.Age}");
                Console.WriteLine($"Gender: {newUser.Gender}");
                Console.WriteLine($"Zip Code: {newUser.ZipCode}");
                Console.WriteLine($"Occupation: {newUser.Occupation.Name}\n");
            }
            else if (choice_num == 8)
            {
                var currentUser = _helper.GetIntInRange("Which user is entering a rating?", 1, _repository.GetAllUsers().ToList().Count());

                var knowsID = _helper.YesNo("\nDo you know the movie ID for the movie ID you would like to rate (Y/N)?");
                while (knowsID == 'N')
                {
                    Console.WriteLine("\nSearch a movie title: ");
                    var user_movie = Console.ReadLine().ToUpper();
                    var movies = _repository.Search(user_movie);

                    if (movies.Any())
                    {
                        foreach (var movie in movies)
                        {
                            _repository.DisplayMovieDetails(Convert.ToInt32(movie.Id));
                        }

                        var searchAgain = _helper.YesNo("\nWould you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            knowsID = 'Y';
                    }
                    else
                    {
                        Console.WriteLine("No movies matched the text entered.");
                        var searchAgain = _helper.YesNo("\nWould you like to search again (Y/N)?");
                        if (searchAgain == 'Y')
                            continue;
                        else
                            break;
                    }
                }
                var currentMovie = _helper.GetIntInRange("\nPlease enter the ID for the movie being rated:", 1, _repository.GetAll().Count());

                var userRating = _helper.GetIntInRange("\nWhat rating would you like to assign to this movie (1 = worst, 5 = best)?", 1, 5);

                DateTime ratedAt;
                Console.WriteLine("\nEach user rating must contain a rating date (the date the user rated the movie).");
                var useToday = _helper.YesNo("Would you like to use today's date (Y/N)");
                if (useToday == 'Y')
                {
                    ratedAt = DateTime.Today;
                }
                else
                {
                    var yearRated = _helper.GetYear("Please enter the year for the rating:");

                    var monthRated = _helper.GetIntInRange("Please enter the month (number) for the rating", 1, 12);

                    int dayRated;

                    //allow for up to 29 days for February.
                    if (monthRated == 2)
                        dayRated = _helper.GetIntInRange("Please enter the day for the rating", 1, 29);
                    //allow for up to 30 days for months of April, June, September and November.
                    else if (monthRated == 4 || monthRated == 6 || monthRated == 9 || monthRated == 11)
                        dayRated = _helper.GetIntInRange("Please enter the day for the rating", 1, 30);
                    //allow up to 31 days for the other months.
                    else
                        dayRated = _helper.GetIntInRange("Please enter the day for the rating", 1, 31);

                    string userRatingDate = monthRated + "-" + Convert.ToString(monthRated) + "-" + Convert.ToString(dayRated);
                    ratedAt = Convert.ToDateTime(userRatingDate);
                }

                var userMovie = _repository.AddUserMovie(currentUser, currentMovie, userRating, ratedAt);

                Console.WriteLine($"\nUser rating added to the database:\n");
                Console.WriteLine($"User: User {userMovie.User.Id} | Age: {userMovie.User.Age} | Gender: {userMovie.User.Gender} | Profession: {userMovie.User.Occupation.Name}");
                Console.WriteLine($"Movie: {userMovie.Movie.Title}");
                Console.WriteLine($"Rating: {userMovie.Rating} of 5 | Date Rated: {userMovie.RatedAt:MMMM dd, yyyy}\n");
            }
            else if (choice_num == 9)
            {
                int filterChoice;
                do
                {
                    Console.WriteLine("Select the element you would like to filter the highest rated movie by:");
                    filterChoice = _helper.GetIntInRange("\n01.) User Occupation\n02.) User Gender\n03.) Never mind", 1, 5);
                    if (filterChoice == 3)
                    {
                        Console.WriteLine();
                        break;
                    }
                    else if (filterChoice == 1)
                    {
                        Console.WriteLine();
                        _repository.GetTopMovieByOccupation();
                        Console.WriteLine("\nResults show the top rated movie, alphabetized, by user occupation.");
                    }
                    else if (filterChoice == 2)
                    {
                        Console.WriteLine();
                        _repository.GetTopMovieByGender();
                        Console.WriteLine("\nResults show the top rated movie, alphabetized, by user gender.");
                    }

                } while (filterChoice != 3);
            }

            else
                Console.WriteLine("Error: Please enter a valid selection.\n");

        } while (choice_num != 10);
    }
}