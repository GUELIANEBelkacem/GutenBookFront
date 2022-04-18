using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Book_List
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListPage : ContentPage
    {
        public List<Book> books = new List<Book>();
        public List<string> bookTitles = new List<string> { "Fuck", "Your", "Mother"};

    public ListPage()
        {
            InitializeComponent();

            foreach (var t in bookTitles) {
                books.Add(new Book(t, "https://www.placecage.com/c/200/500"));
            }

            mylist.ItemsSource = books;
        }
    }

    public class Book
    {
        public string Name { get; set; }
        public string ImageSource { get; set; }

        public Book(string t, string img) {
            Name = t;
            ImageSource = img;
        }
    }
}