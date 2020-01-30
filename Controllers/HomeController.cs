using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Books.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Books.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            List<Book> all_books = dbContext.Books.ToList();
            return View(all_books);
        }

        [HttpGet("book/create")]
        public IActionResult create()
        {
            ViewBag.all_authors = dbContext.Authors.ToList();
            return View();
        }

        [HttpPost("book/new")]
        public IActionResult new_book(Book newBook, string other)
        {
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(other) == false && newBook.AuthorId == 0) //if dropdown is empty and others full
                {
                    other = char.ToUpper(other[0]) + other.Substring(1);
                    var does_exist = dbContext.Authors.FirstOrDefault(i=>i.name==other);
                    if(does_exist==null){
                        Author new_auth = new Author { name = other };
                        dbContext.Add(new_auth);
                    }
                    newBook.author = does_exist;
                    dbContext.Add(newBook);
                    dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
                else if(string.IsNullOrEmpty(other) == true && newBook.AuthorId == 0){ // if both empty
                    ViewBag.error = "Select an author!";
                    ViewBag.all_authors = dbContext.Authors.ToList();
                    return View("create");
                }else if(string.IsNullOrEmpty(other) == false && newBook.AuthorId != 0){ // if both full
                    ViewBag.error = "Cannot select author and add other!";
                    ViewBag.all_authors = dbContext.Authors.ToList();
                    return View("create");
                }
            }
            Author auth = dbContext.Authors.FirstOrDefault(i => i.AuthorId == newBook.AuthorId);
            newBook.author = auth;
            dbContext.Books.Add(newBook);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet("book/all")]
        public IActionResult all(){
            List<Book> all_books = dbContext.Books.Include(auth => auth.author).ToList();
            return View(all_books);
        }

        [HttpGet("book/edit/{id}")]
        public IActionResult edit(int id){
            Book bookToEdit = dbContext.Books.FirstOrDefault(book => book.BookId==id);
            if(bookToEdit==null){
                return RedirectToAction("Index");
            }
            ViewBag.all_authors = dbContext.Authors.ToList();
            return View(bookToEdit);
        }

        [HttpPost("book/update")]
        public IActionResult update(Book book, string other){
            Console.WriteLine(other);
            Console.WriteLine(book.AuthorId);
            if(!ModelState.IsValid){
                if(book.AuthorId==0 && string.IsNullOrEmpty(other)==true){ //both empty
                    ViewBag.error = "Select an author!";
                }
                ViewBag.all_authors = dbContext.Authors.ToList();
                return View("edit", book);
            }
            if(book.AuthorId!=0 && string.IsNullOrEmpty(other)==false){ //both filled
                ViewBag.error = "Cannot select author and add other!"; 
                ViewBag.all_authors = dbContext.Authors.ToList();
                return View("edit",book);
            }
            Book bookFromDb = dbContext.Books.FirstOrDefault(b=> b.BookId==book.BookId);
            if (book.AuthorId==0 && string.IsNullOrEmpty(other)==false){ // drop empty other full
                other = char.ToUpper(other[0]) + other.Substring(1);
                var does_exist = dbContext.Authors.FirstOrDefault(i=>i.name==other);
                if(does_exist==null){
                    Author new_auth = new Author { name = other };
                    dbContext.Add(new_auth);
                    bookFromDb.AuthorId=new_auth.AuthorId;
                    bookFromDb.author = new_auth;
                }else{
                    bookFromDb.AuthorId=does_exist.AuthorId;
                    bookFromDb.author = does_exist;
                }
            }
            if (book.AuthorId!=0 && string.IsNullOrEmpty(other)==true){ // drop full other empty
                Author authorFromDb = dbContext.Authors.FirstOrDefault(i=>i.AuthorId==book.AuthorId);
                Console.WriteLine(authorFromDb);
                bookFromDb.author = authorFromDb;
                bookFromDb.AuthorId = authorFromDb.AuthorId;
            }
            bookFromDb.title = book.title;
            bookFromDb.desc = book.desc;
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [HttpPost("search")]
        public IActionResult search(string query){
            Author authResult = dbContext.Authors.Include(t=>t.BooksWritten).FirstOrDefault(a=>a.name==query);
            Book bookResult = dbContext.Books.Include(o=>o.author).FirstOrDefault(b=>b.title==query);
            if (authResult!=null){
                TempData["auth"] = JsonConvert.SerializeObject(authResult, Formatting.Indented, new JsonSerializerSettings() {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });
            }
            if(bookResult!=null){
                TempData["book"] = JsonConvert.SerializeObject(bookResult, Formatting.Indented, new JsonSerializerSettings() {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                });
            }
            return RedirectToAction("diaplay", new {quary = query});
        }

        [HttpGet("result")]
        public IActionResult diaplay(string quary){
            Author auth = null;
            Book book = null;
            if(TempData.Any(i=>i.Key=="auth")){
                auth = JsonConvert.DeserializeObject<Author>((string)TempData["auth"]);
            }
            if(TempData.Any(i=>i.Key=="book")){
                book = JsonConvert.DeserializeObject<Book>((string)TempData["book"]);
            }
            Together tog = new Together();
            tog.author = auth;
            tog.book = book;
            ViewBag.quary = quary;
            return View("result", tog);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
