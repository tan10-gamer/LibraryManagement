using LibraryManagement.Components.Models;

namespace LibraryManagement.Components.Services
{
    public class LibraryService
    {
        public List<Book> Books { get; set; } = new();
        public List<User> Users { get; set; } = new();

        public Dictionary<User, List<Book>> BorrowedBooks { get; set; } = new();        // Borrowed Books tracks which books each user has borrowed

        private readonly string booksFile;
        private readonly string userFile;

        public LibraryService(IWebHostEnvironment environment)
        {
            booksFile = Path.Combine(environment.ContentRootPath, "Data", "Books.csv");
            userFile = Path.Combine(environment.ContentRootPath, "Data", "Users.csv");

            ReadBooks();
            ReadUsers();
        }

        public void ReadBooks()
        {
            Books.Clear();

            if (File.Exists(booksFile))
            { 
                foreach (string line in File.ReadAllLines(booksFile))
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 4 && int.TryParse(fields[0], out int id))           // int.TryParse() checks whether the first field is a valid book ID
                    {
                        Book book = new Book
                        {
                            Id = id,
                            Title = fields[1].Trim(),
                            Author = fields[2].Trim(),
                            ISBN = fields[3].Trim()
                        };

                        Books.Add(book);
                    }
                }
            }
        }

        public void ReadUsers()
        {
            Users.Clear();

            if (File.Exists(userFile))
            {
                foreach (string line in File.ReadAllLines(userFile))
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 3 && int.TryParse(fields[0], out int id))
                    {
                        User user = new User
                        {
                            Id = id,
                            Name = fields[1].Trim(),
                            Email = fields[2].Trim()
                        };

                        Users.Add(user);
                    }
                }
            }
        }

        // BOOK MANAGEMENT //

        public void AddBook(string title, string author, string isbn)
        {
            int id = Books.Concat(BorrowedBooks.Values.SelectMany(list => list))
                            .Select(b => b.Id)
                            .DefaultIfEmpty(0)
                            .Max() + 1; // checks whether the list already contains books, including borrowed books and generates the next book Id

            Book book = new Book      // creates new book object
            {
                Id = id,
                Title = title,
                Author = author,
                ISBN = isbn,
            };

            Books.Add(book);      // adds it to the in-memory list
            SaveBooks();          // saves the updated list to Books.csv

        }


        // converts the books in the list into CSV rows and writes them to Book.csv
        public void SaveBooks()
        {
            // ensures borrowed books remain in permanent storage while they are unavailable
            List<Book> allBooks = new List<Book>(Books);

            foreach (List<Book> borrowedList in BorrowedBooks.Values)
            {
                allBooks.AddRange(borrowedList);
            }

            List<string> lines = new();

            foreach (Book book in Books)
            {
                string line = $"{book.Id},{book.Title},{book.Author},{book.ISBN}";
                lines.Add(line);
            }

            File.WriteAllLines(booksFile, lines);
        }


        public void EditBook(int bookId, string title, string author, string isbn)
        {
            Book? book = Books.FirstOrDefault(b => b.Id == bookId);

            // checks whether the book exists
            if (book != null)
            {
                // Updates the Information
                if (!string.IsNullOrEmpty(title))
                    book.Title = title;

                if (!string.IsNullOrEmpty(author))
                    book.Author = author;

                if (!string.IsNullOrEmpty(isbn))
                    book.ISBN = isbn;

                SaveBooks();

            }
        }

        public void DeleteBook(int bookId)
        {
            // finds the books using the book's id
            Book? book = Books.FirstOrDefault(b => b.Id == bookId);

            if (book != null)
            {
                // remove the book and updates the csv file
                Books.Remove(book);
                SaveBooks();
            }
        }
        // USER MANAGEMENT
        public void AddUser(string name, string email)
        {
            int id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;

            User user = new User
            {
                Id = id,
                Name = name,
                Email = email
            };

            Users.Add(user);
            SaveUsers();
        }

        public void SaveUsers()
        {
            List<string> lines = new();

            foreach (User user in Users)
            {
                string line = $"{user.Id},{user.Name},{user.Email}";
                lines.Add(line);
            }

            File.WriteAllLines(userFile, lines);    // converts each User object into a CSV row
        }

        public void EditUser(int userId, string name, string email)
        {
            User? user = Users.FirstOrDefault(u => u.Id == userId);

            // checks whether the book exists
            if (user != null)
            {
                // Updates the Information
                if (!string.IsNullOrEmpty(name))
                    user.Name = name;

                if (!string.IsNullOrEmpty(email))
                    user.Email = email;

                SaveUsers();

            }
        }
        public void DeleteUser(int userId)
        {
            // finds the user using the user's id
            User? user = Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                // don't delete users who still have borrowed books
                if (BorrowedBooks.ContainsKey(user) && BorrowedBooks[user].Count > 0)
                    return;

                // remove the user and updates the csv file
                Users.Remove(user);
                BorrowedBooks.Remove(user);
                SaveUsers();
            }
        }

        // Borrowing Books
        public bool BorrowBook(int bookId, int userId)
        {
            // finds the book and the user
            Book? book = Books.FirstOrDefault(b => b.Id == bookId);
            User? user = Users.FirstOrDefault(u => u.Id == userId);

            if (book != null && user != null)           // searchs whether both exist
            {
                // adds the book to the user's borrowed list
                if (!BorrowedBooks.ContainsKey(user))
                {
                    BorrowedBooks[user] = new List<Book>();
                }

                BorrowedBooks[user].Add(book);
                Books.Remove(book);             // removes the book from available books

                return true;
            }

            return false;
        }

        // Returning Books
        public bool ReturnBook(int userId, int bookId)
        {
            // find the user
            User? user = Users.FirstOrDefault(u => u.Id == userId);

            // checks if the user has borrowed books
            if (user != null && BorrowedBooks.ContainsKey(user))
            {
                Book? book = BorrowedBooks[user].FirstOrDefault(b => b.Id == bookId);

                if (book != null)
                {
                    BorrowedBooks[user].Remove(book);            // removes the book from the user's borrowed list
                    Books.Add(book);                             // Adds the book back to available books

                    return true;
                }
            }
            return false;
        }

        public List<Book> GetBorrowedBooks (int userId)
        {
            User? user = Users.FirstOrDefault(u => u.Id == userId);

            if (user != null && BorrowedBooks.ContainsKey(user))
            {
                return BorrowedBooks[user];
            }

            return new List<Book>();
        }
    }

}
