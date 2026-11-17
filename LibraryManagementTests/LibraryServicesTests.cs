using LibraryManagement.Components.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace LibraryManagementTests
{
    [TestClass]
    public sealed class LibraryServicesTests
    {
        private LibraryService service = null!;
        private string testFolder = "";

        [TestInitialize]
        public void Setup()
        {
            // create a temp folder for testing
            testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(testFolder, "Data"));

            // create a test environment
            var environment = new TestEnvironment
            {
                ContentRootPath = testFolder
            };

            service = new LibraryService(environment);
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(testFolder))
            {
                Directory.Delete(testFolder, true);
            }

        }

        // -------- BOOK MANAGEMENT ---------------


        [TestMethod]
        public void AddBookTest()
        {
            // create book to test

            string title = "The Hunger Games: Mockingbird";
            string author = "Suzanne Collins";
            string isbn = "74829204982";

            // calls AddBook() 

            service.AddBook(title, author, isbn);

            // verifies the book was added with the correct information

            Assert.AreEqual(1, service.Books.Count);
            Assert.AreEqual(title, service.Books[0].Title);
            Assert.AreEqual(author, service.Books[0].Author);
            Assert.AreEqual(isbn, service.Books[0].ISBN);
        }

        [TestMethod]
        public void SaveBooksTest()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            string filePath = Path.Combine(testFolder, "Data", "Books.csv");

            service.SaveBooks();

            // verify the CSV file exists and contains the book's information

            Assert.IsTrue(File.Exists(filePath));
            string content = File.ReadAllText(filePath);

            // *used ChatGPT to implement the lines below*
            StringAssert.Contains(content, "The Hunger Games");
            StringAssert.Contains(content, "Suzanne Collins");
            StringAssert.Contains(content, "74829204982");

        }

        [TestMethod]
        public void EditBookTest()
        {
            // create a book with old information with the id

            service.AddBook("Old Title", "Old Author", "123456");
            int bookId = service.Books[0].Id;

            // call EditBook()

            service.EditBook(bookId, "New Title", "New Author", "789101112");


            // verify

            Assert.AreEqual("New Title", service.Books[0].Title);
            Assert.AreEqual("New Author", service.Books[0].Author);
            Assert.AreEqual("789101112", service.Books[0].ISBN);
        }

        [TestMethod]
        public void EditBookInvalidTest()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");

            // attempt to edit a book that does not exist

            service.EditBook(2873, "New Title", "New Author", "789903");

            // verify that the original book remains unchanged

            Assert.AreEqual(1, service.Books.Count);
            Assert.AreEqual("The Hunger Games: Mockingbird", service.Books[0].Title);
            Assert.AreEqual("Suzanne Collins", service.Books[0].Author);
            Assert.AreEqual("74829204982", service.Books[0].ISBN);
        }
        [TestMethod]
        public void DeleteBookTest()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            int bookId = service.Books[0].Id;

            service.DeleteBook(bookId);

            Assert.AreEqual(0, service.Books.Count);
        }

        [TestMethod]
        public void DeleteBookInvalidId()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");

            // attempt to delete book id 4839, which does not exist

            service.DeleteBook(4839);

            // verify book is still in the library

            Assert.AreEqual(1, service.Books.Count);

        }

        [TestMethod]
        public void ReadBooksTest()
        {
            // create a temp Books.csv file containing one book

            string filePath = Path.Combine(testFolder, "Data", "Books.csv");
            File.WriteAllText(filePath, "1, The Hunger Games: Mockingbird,Suzanne Collins,74829204982");

            service.ReadBooks();

            Assert.AreEqual(1, service.Books.Count);
            Assert.AreEqual(1, service.Books[0].Id);
            Assert.AreEqual("The Hunger Games: Mockingbird", service.Books[0].Title);
            Assert.AreEqual("Suzanne Collins", service.Books[0].Author);
            Assert.AreEqual("74829204982", service.Books[0].ISBN);
        }

        [TestMethod]
        public void ReadBooksEmptyFile()
        {
            string filePath = Path.Combine(testFolder, "Data", "Books.csv");
            // create an empty CSV file

            File.WriteAllText(filePath, "");

            service.ReadBooks();

            Assert.AreEqual(0, service.Books.Count);
        }

        // --------- USER MANAGEMENT ---------------

        [TestMethod]
        public void AddUserTest()
        {
            string name = "Jon Snow";
            string email = "kingofthenorth@gmail.com";

            service.AddUser(name, email);

            Assert.AreEqual(1, service.Users.Count);
            Assert.AreEqual(1, service.Users[0].Id);
            Assert.AreEqual(name, service.Users[0].Name);
            Assert.AreEqual(email, service.Users[0].Email);
        }

        [TestMethod]
        public void SaveUserTest()
        {
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");
            string filePath = Path.Combine(testFolder, "Data", "Users.csv");

            service.SaveUsers();

            // verify that the file exists and contains the correct information
            Assert.IsTrue(File.Exists(filePath));
            string content = File.ReadAllText(filePath);
            StringAssert.Contains(content, "Jon Snow");
            StringAssert.Contains(content, "kingofthenorth@gmail.com");
        }

        [TestMethod]
        public void EditUserTest()
        {
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");
            int userId = service.Users[0].Id;

            service.EditUser(userId, "Jon Snow", "kingofthenorth@gmail.com");

            // verifies if both fields were updated

            Assert.AreEqual("Jon Snow", service.Users[0].Name);
            Assert.AreEqual("kingofthenorth@gmail.com", service.Users[0].Email);
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");
            int userId = service.Users[0].Id;

            service.DeleteUser(userId);

            // verifies that the user list is now empty

            Assert.AreEqual(0, service.Users.Count);

        }

        [TestMethod]
        public void DeleteUserInvalidTest()
        {
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            service.DeleteUser(6767);

            // verifies that the user list is now empty

            Assert.AreEqual(1, service.Users.Count);
            Assert.AreEqual("Jon Snow", service.Users[0].Name);
            Assert.AreEqual("kingofthenorth@gmail.com", service.Users[0].Email);

        }

        [TestMethod]
        public void ReadUserTest()
        {
            string filePath = Path.Combine(testFolder, "Data", "Users.csv");
            File.WriteAllText(filePath, "1,Jon Snow, kingofthenorth@gmail.com");

            service.ReadUsers();

            Assert.AreEqual(1, service.Users.Count);
            Assert.AreEqual(1, service.Users[0].Id);
            Assert.AreEqual("Jon Snow", service.Users[0].Name);
            Assert.AreEqual("kingofthenorth@gmail.com", service.Users[0].Email);
        }

        [TestMethod]
        public void ReadUserEmptyFileTest()
        {
            string filePath = Path.Combine(testFolder, "Data", "Users.csv");
            File.WriteAllText(filePath, "");

            service.ReadUsers();

            // verifies that no users were loaded
            Assert.AreEqual(0, service.Users.Count);

        }

        // ----------- BORROWING AND RETURNING MANAGEMENT ---------

        [TestMethod]
        public void BorrowBookTest()
        {
            // adds a book and user 

            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            int bookId = service.Books[0].Id;
            int userId = service.Users[0].Id;

            // user borrows book

            bool result = service.BorrowBook(bookId, userId);

            // verify that borrowing was successful

            Assert.IsTrue(result);
            Assert.AreEqual(0, service.Books.Count);
            Assert.AreEqual(1, service.GetBorrowedBooks(userId).Count);
        }

        [TestMethod]
        public void BorrowBookInvalidBookId()
        {
            // create a user without adding any books

            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");
            int userId = service.Users[0].Id;

            // attempt to borrow a book with an Id that does not exist

            bool result = service.BorrowBook(3839, userId);

            // borrowing should fail

            Assert.IsFalse(result);
            Assert.AreEqual(0, service.GetBorrowedBooks(userId).Count);
        }

        [TestMethod]
        public void BorrowBookInvalidUserId()
        {
            // add a book without a user

            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            int bookId = service.Books[0].Id;

            // invalid user Id
            bool result = service.BorrowBook(bookId, 7392);

            // should fail

            Assert.IsFalse(result);
            Assert.AreEqual(1, service.Books.Count);

        }

        [TestMethod]
        public void ReturnBookTest()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            int bookId = service.Books[0].Id;
            int userId = service.Users[0].Id;

            service.BorrowBook(bookId, userId);

            bool result = service.ReturnBook(userId, bookId);

            // verify the return was successfull and book becomes available again

            Assert.IsTrue(result);
            Assert.AreEqual(1, service.Books.Count);
            Assert.AreEqual(0, service.GetBorrowedBooks(userId).Count);
        }

        [TestMethod]
        public void ReturnBookNotBorrowed()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            int bookId = service.Books[0].Id;
            int userId = service.Users[0].Id;

            bool result = service.ReturnBook(userId, bookId);

            // verify the return was unsuccessfull and book stays available

            Assert.IsFalse(result);
            Assert.AreEqual(1, service.Books.Count);
        }

        [TestMethod]
        public void GetBorrowedBooksTest()
        {

            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            int bookId = service.Books[0].Id;
            int userId = service.Users[0].Id;

            service.BorrowBook(bookId, userId);

            // retrieve the user's borrowed books

            var borrowedBooks = service.GetBorrowedBooks(userId);


            // verify the list contains the correct book

            Assert.AreEqual(1, borrowedBooks.Count);
            Assert.AreEqual("The Hunger Games: Mockingbird", borrowedBooks[0].Title);
        }

        [TestMethod]
        public void DeleteUserWithBorrowedBooks()
        {
            service.AddBook("The Hunger Games: Mockingbird", "Suzanne Collins", "74829204982");
            service.AddUser("Jon Snow", "kingofthenorth@gmail.com");

            int bookId = service.Books[0].Id;
            int userId = service.Users[0].Id;

            service.BorrowBook(bookId, userId);

            // attempt to delete a user
            service.DeleteUser(userId);

            // verify the user remains in the system if they have borrowed books not returned

            Assert.AreEqual(1, service.Users.Count);
            Assert.AreEqual(1, service.GetBorrowedBooks(userId).Count);
        }

        private class TestEnvironment : IWebHostEnvironment
        {
            public string ContentRootPath { get; set; } = "";
            public string WebRootPath { get; set; } = "";
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "";
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
            public IFileProvider WebRootFileProvider { get; set; } = null!;
        }

    }
}
