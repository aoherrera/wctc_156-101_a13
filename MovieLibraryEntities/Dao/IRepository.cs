using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Dao
{
    public interface IRepository
    {
        IEnumerable<Movie> GetAll();
        IEnumerable<Movie> GetTopMovies(int amount);
        void GetTopMovieByOccupation();
        void GetTopMovieByGender();
        IEnumerable<Movie> Search(string searchString);
        IEnumerable<Movie> SearchByReleaseYear(int releaseYear);
        Movie SearchByID(int movieID);
        void AddMovie(string movieTitle, DateTime releaseDate, string releaseYear, List<int>? movieGenres);
        bool GetValidMovie(int movieID);
        IEnumerable<Genre> GetGenres();
        bool UpdateMovieTitle(int movieID, string movieTitle);
        bool UpdateMovieReleaseYear(int movieID, int releaseYear);
        bool UpdateMovieReleaseDate(int movieID, DateTime releaseDate);
        bool AddMovieGenres(int movieID, List<int> movieGenres);
        bool DeleteMovieGenre(int movieID, int movieGenreGenreID);
        void DisplayMovieDetails(int movieID);
        void DeleteMovie(int movieID);
        User AddUser(long age, string gender, string zipCode, int occupationID);
        IEnumerable<Occupation> GetUserOccupations();
        User GetUserByID(int userID);
        IEnumerable<User> GetAllUsers();
        UserMovie AddUserMovie(int userID, int movieID, int userRating, DateTime ratedAt);
    }
}
