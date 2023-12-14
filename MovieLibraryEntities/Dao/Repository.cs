using Microsoft.EntityFrameworkCore;
using MovieLibraryEntities.Context;
using MovieLibraryEntities.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace MovieLibraryEntities.Dao
{
    public class Repository : IRepository, IDisposable
    {
        private readonly IDbContextFactory<MovieContext> _contextFactory;
        private readonly MovieContext _context;
        private readonly ILogger<Repository> _logger;

        public Repository(IDbContextFactory<MovieContext> contextFactory, ILogger<Repository> logger)
        {
            _contextFactory = contextFactory;
            _context = _contextFactory.CreateDbContext();
            _logger = logger;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IEnumerable<Movie> Search(string searchString)
        {
            _logger.LogInformation($"Searching term '{searchString}' ...");
            var yearPattern = @"\(\d\d\d\d\)"; //year pattern to match any year between 1800 and 2099.

            var allMovies = _context.Movies
                .Include(x => x.MovieGenres)
                .ThenInclude(x => x.Genre);
            var listOfMovies = allMovies.ToList();

            var temp = listOfMovies.Where(x => Regex.Replace(x.Title, yearPattern, "").Contains(searchString, StringComparison.CurrentCultureIgnoreCase));
            return temp;
        }

        public IEnumerable<Movie> SearchByReleaseYear(int releaseYear)
        {
            _logger.LogInformation($"Searching relese year {releaseYear} ...");
            var allMovies = _context.Movies
            .Include(x => x.MovieGenres)
            .ThenInclude(x => x.Genre);
            var listOfMovies = allMovies.ToList();

            var temp = listOfMovies.Where(x => x.ReleaseDate.Year == releaseYear);
            return temp;
        }

        public IEnumerable<Movie> GetTopMovies(int amount)
        {
            _logger.LogInformation($"Retrieving data for {amount} entries...");
            var allMovies = _context.Movies
                .Include(x => x.UserMovies)
                .ThenInclude(x => x.User)
                .OrderBy(x => x.Title)
                .Take(amount)
                .ToList();

            return allMovies;
        }

        public void GetTopMovieByOccupation()
        {
            _logger.LogInformation("Quering database ...");
            var userOccupations = _context.UserMovies.GroupBy(x => x.User.Occupation.Name)
                .Select(g => new
                {
                    UserOccupation = g.Key,
                    MovieTitle = g.OrderBy(x => x.Movie.Title)
                    .Where(x => x.Rating == (g.Max(x => x.Rating)))
                    .Select(x => x.Movie.Title).FirstOrDefault(),
                    Rating = g.Max(x => x.Rating)

                }
                ).ToList();

            foreach (var movie in userOccupations)
            {
                Console.WriteLine($"{movie.UserOccupation}: {movie.MovieTitle} - {movie.Rating}/5");
            }
        }

        public void GetTopMovieByGender()
        {
            _logger.LogInformation("Quering database ...");
            var userGenders = _context.UserMovies.GroupBy(x => x.User.Gender)
                .Select(g => new
                {
                    UserGender = g.Key,
                    MovieTitle = g.OrderBy(x => x.Movie.Title)
                    .Where(x => x.Rating == (g.Max(x => x.Rating)))
                    .Select(x => x.Movie.Title).FirstOrDefault(),
                    Rating = g.Max(x => x.Rating)
                }
                ).ToList();

            foreach (var movie in userGenders)
            {
                if (movie.UserGender == "M")
                {
                    Console.WriteLine($"Male: {movie.MovieTitle} - {movie.Rating}/5");
                }
                else if (movie.UserGender == "F")
                {
                    Console.WriteLine($"Female: {movie.MovieTitle} - {movie.Rating}/5");
                }
                else if (movie.UserGender == "N")
                {
                    Console.WriteLine($"Non-Binary: {movie.MovieTitle} - {movie.Rating}/5");
                }
                else
                {
                    Console.WriteLine($"{movie.UserGender} {movie.MovieTitle} - {movie.Rating}/5");
                }

            }
        }

        public void AddMovie(string movieTitle, DateTime releaseDate, string releaseYear, List<int>? movieGenres)
        {
            var movie = new Movie()
            {
                Title = movieTitle + " (" + releaseYear + ")",
                ReleaseDate = releaseDate
            };
            //check if movie exists. If it does, exit method.
            var allMovies = _context.Movies;
            var listOfMovies = allMovies.ToList();
            if(listOfMovies.Exists(x => x.Title == movie.Title))
            {
                _logger.LogError($"Error: user entry {movie.Title} already exists in database. Entry not added.");
                Console.WriteLine($"\nError: {movie.Title} already exists in the database. Entry not added.\n");
                return;
            }
            //add movie genre(s)

            //if there are no genres to add, save add movie to database.
            if (movieGenres is null || !movieGenres.Any())
            {
                _context.Movies.Add(movie);
                _context.SaveChanges();
                _logger.LogInformation($"Movie {movie.Title} added to database.");
                //send message to user indicating the movie has been added.
                Console.WriteLine($"{movie.Title} added to database.\n");
                return;
            }

            var userGenres = new List<MovieGenre>();
            foreach (var genre in movieGenres)
            {
                var movieGenre = new MovieGenre()
                {
                    Genre = _context.Genres.FirstOrDefault(x => x.Id == genre),
                    Movie = movie
                };
                userGenres.Add(movieGenre);
            }
            _context.Movies.Add(movie);

            foreach (var userGenre in userGenres)
            {
                _context.MovieGenres.Add(userGenre);
            }
            _context.SaveChanges();
            _logger.LogInformation($"Movie {movie.Title} added to database.");
            //send message to user indicating the movie has been added.
            Console.WriteLine($"{movie.Title} added to database.\n");
        }

        public void DisplayMovieDetails(int movieID)
        {
            var movie = SearchByID(movieID);
            Console.WriteLine($"\nMovie ID: {movie.Id}");
            Console.WriteLine($"Movie Title (Release Year): {movie.Title}");
            Console.WriteLine($"Release Date: {movie.ReleaseDate.ToString("MMMM dd, yyyy")}");
            if (movie.MovieGenres is not null && movie.MovieGenres.Count() > 0)
            {
                Console.Write("Movie Genre(s): ");
                for (int i = 0; i < movie.MovieGenres.Count(); i++)
                {
                    if (i != (movie.MovieGenres.Count() - 1))
                    {
                        Console.Write($"{movie.MovieGenres.ElementAt(i).Genre.Name}, ");
                    }
                    else
                        Console.WriteLine($"{movie.MovieGenres.ElementAt(i).Genre.Name}");
                }
            }
        }

        public Movie SearchByID (int movieID)
        {
            _logger.LogInformation("Querying database ...");
            var movie = _context.Movies.FirstOrDefault(x => x.Id == movieID);

            return movie;
        }

        public bool GetValidMovie(int movieID)
        {
            _logger.LogInformation("Displaying movie details to user ...");
            var movie = _context.Movies.FirstOrDefault(x => x.Id == movieID);
            if (movie is Movie)
                return true;
            else
            {
                _logger.LogError("User attempted to query an invalid movie.");
                //movie is default(Movie); FirstOrDefault returns default(Type))
                return false;
            }
        }

        //Get a listing of avaialble movie genres in the Genres table.
        public IEnumerable<Genre> GetGenres()
        {
            _logger.LogInformation("Querying database ...");
            return _context.Genres;
        }

        //update movie title
        public bool UpdateMovieTitle(int movieID, string movieTitle)
        {
            var movie = SearchByID(movieID);
            //recreate the movie title using the movie's current year to
            //match formatting.
            movie.Title = $"{movieTitle} ({movie.ReleaseDate.Year})";
            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} updated with new title: {movie.Title}.");
            return true;
        }

        //update movie release year
        public bool UpdateMovieReleaseYear(int movieID, int releaseYear)
        {
            var movie = SearchByID(movieID);
            //recreate the movie title using the movie's current title to
            //match formating.
            var yearIndex = Regex.Match(movie.Title, @"\(\d\d\d\d\)").Index;
            var titleOnly = movie.Title.Substring(0, yearIndex);
            movie.Title = $"{titleOnly.Trim()} ({releaseYear})";

            //update release date with a new year. There has to be a simpler way
            //to accomplish this ...
            movie.ReleaseDate = Convert.ToDateTime($"{Convert.ToString(releaseYear)}-{Convert.ToString(movie.ReleaseDate.Month)}-{Convert.ToString(movie.ReleaseDate.Day)}");
            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} updated with release date: {movie.ReleaseDate}.");
            return true;
        }

        //update movie release date
        public bool UpdateMovieReleaseDate(int movieID, DateTime releaseDate)
        {
            var movie = SearchByID(movieID);

            //recreate the movie title (year) using the movie's current title to
            //match formating.
            var yearIndex = Regex.Match(movie.Title, @"\(\d\d\d\d\)").Index;
            var titleOnly = movie.Title.Substring(0, yearIndex);
            movie.Title = $"{titleOnly.Trim()} ({releaseDate.Year})";

            movie.ReleaseDate = releaseDate;
            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} updated with release date: {movie.ReleaseDate}.");

            return true;
        }

        //add movie genre(s)
        public bool AddMovieGenres(int movieID, List<int> movieGenres)
        {
            var movie = SearchByID(movieID);
            var userGenres = new List<MovieGenre>();

            foreach (var genre in movieGenres)
            {
                var movieGenre = new MovieGenre()
                {
                    Genre = _context.Genres.First(x => x.Id == genre),
                    Movie = movie
                };
                userGenres.Add(movieGenre);
            }
            foreach (var userGenre in userGenres)
            {
                _context.MovieGenres.Add(userGenre);
            }
            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} updated with new genres.");
            return true;
        }

        public bool DeleteMovieGenre (int movieID, int deleteGenre)
        {
            var movie = SearchByID(movieID);
            var currentGenres = movie.MovieGenres ?? new List<MovieGenre>();

            currentGenres.Remove(movie.MovieGenres.First(x => x.Genre.Id ==
                currentGenres.ElementAt(deleteGenre - 1).Genre.Id));

            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} genre removed.");

            return true;
        }

        public void DeleteMovie(int movieID)
        {
            var movieToDelete = SearchByID(movieID);
            _context.Remove(movieToDelete);
            _context.SaveChanges();
            _logger.LogInformation($"Entry {movieID} removed from database");
            return;
        }

        public User AddUser(long age, string gender, string zipCode, int occupation)
        {
            var user = new User()
            {
                Age = age,
                Gender = gender,
                ZipCode = zipCode,
                Occupation = _context.Occupations.First(x => x.Id == occupation)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.LogInformation($"Entry {user.Id} added to database.");

            return user;
        }

        //Get a listing of avaialble movie genres in the Genres table.
        public IEnumerable<Occupation> GetUserOccupations()
        {
            return _context.Occupations;
        }

        public User GetUserByID(int userID)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userID);

            return user;
        }

        public IEnumerable<Movie> GetAll()
        {
            var allMovies = _context.Movies
                .Include(x => x.MovieGenres)
                .ThenInclude(x => x.Genre)
                .ToList();

            return allMovies;

        }

        public IEnumerable<User> GetAllUsers()
        {
            var allUsers = _context.Users.ToList();
            return allUsers;
        }

        public UserMovie AddUserMovie(int userID, int movieID, int userRating, DateTime ratedAt)
        {
            var userMovie = new UserMovie()
            {
                User = _context.Users.First(x => x.Id == userID),
                Movie = _context.Movies.First(x => x.Id == movieID),
                Rating = userRating,
                RatedAt = ratedAt
            };

            _context.UserMovies.Add(userMovie);
            _context.SaveChanges();
            _logger.LogInformation($"Entry {userMovie.Id} added to database.");

            return userMovie;
        }
    }
}