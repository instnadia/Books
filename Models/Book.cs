using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Books.Models
{
    public class Book
    {
        [Key]
        public int BookId {get;set;}
        [MinLength(2)]
        [Required]
        public string title {get;set;}
        public string desc {get;set;}
        public int AuthorId {get;set;}
        public Author author {get;set;}

        public override string ToString(){
            return "title: "+title+", desc: "+desc+" author: "+AuthorId;
        }
    }

    public class Author
    {
        [Key]
        public int AuthorId {get; set;}
        public string name {get;set;}
        public List<Book> BooksWritten {get;set;} 
    }
    [NotMapped]
    public class Together {
        public Author author {get;set;}
        public Book book {get;set;}
    }
}