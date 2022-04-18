using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App1.Book_List
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        public static string API = "http://10.0.2.2:8080";
        public static string REALAPI = "http://1c26-2a01-e34-ec19-c730-c019-fd7e-1eea-124e.ngrok.io";
        public ObservableCollection<Booky> books = new ObservableCollection<Booky>();
        public ObservableCollection<Booky> BookList = new ObservableCollection<Booky>();
        public ObservableCollection<string> bookTitles = new ObservableCollection<string> { "Welcome", "To", "GutenBook" };
        public ObservableCollection<string> BookIDs = new ObservableCollection<string>();
        public Button CurButton = null;
        public string CurResponse = "";
        public string CurWord = "";

        public SearchPage()
        {
            InitializeComponent();
			
		

			foreach (var t in bookTitles)
            {
                books.Add(new Booky(t, "https://www.placecage.com/c/200/500", "https://www.placecage.com/c/200/500"));
            }

            mylist.FlowItemsSource = books;

            var b1 = new Button { Text = "Simple Search", BackgroundColor = Color.FromHex("#2196F3"), TextColor = Color.White, WidthRequest=100  };
            var b2 = new Button { Text = "Multiple Search", BackgroundColor = Color.FromHex("#2196F3"), TextColor = Color.White, WidthRequest = 100 };
            var b3 = new Button { Text = "RegEx Search", BackgroundColor = Color.FromHex("#2196F3"), TextColor = Color.White, WidthRequest = 100 };
            b1.Clicked += B_Clicked;
            b2.Clicked += B_Clicked;
            b3.Clicked += B_Clicked;
            b1.BackgroundColor = Color.FromHex("#1476c4");
            CurButton = b1;

            segments.Children.Clear();
            segments.Children.Add(b1);
            segments.Children.Add(b2);
            segments.Children.Add(b3);
  
        }

        private void B_Clicked(object sender, EventArgs e)
        {
            var b = (Button)sender;
            if (b != CurButton) {
                CurButton.BackgroundColor = Color.FromHex("#2196F3");
                b.BackgroundColor = Color.FromHex("#1476c4");
                CurButton = b;
            }
            
        }

        private async Task<List<List<string>>> GutenSearch(string word) {
            //var url = API+"/search/?word=" + word;
            var url = GetRoot() + word;
            var uri = new Uri(url);
            HttpClient myClient = new HttpClient();
            List<List<string>> results = new List<List<string>>();
            List<string> resultb = new List<string>();
            List<string> resultr = new List<string>();
            try
            {
                var response = await myClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    ResponseModel data = JsonConvert.DeserializeObject<ResponseModel>(result);


                    var temp1 = data.books.Replace("[", "");
                    temp1 = temp1.Replace("]", "");
                    temp1 = temp1.Replace(" ", "");

                    List<string> uris = GetURLS("https://gutendex.com/books/?ids=", temp1);
                    
                    foreach(var u in uris) {
                        var gutenuri = new Uri(u);
                        response = await myClient.GetAsync(gutenuri);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                            resultb.Add(result);
                            
                        }
                        
                    }


                    var temp2 = data.suggestions.Replace("[", "");
                    temp2 = temp2.Replace("]", "");
                    temp2 = temp2.Replace(" ", "");

                    List<string> uris2 = GetURLS("https://gutendex.com/books/?ids=", temp2);
                   
                    foreach (var u in uris2)
                    {
                        var gutenuri = new Uri(u);
                        response = await myClient.GetAsync(gutenuri);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                            resultr.Add(result);
                        

                        }

                    }
                    results.Add(resultb);
                    results.Add(resultr);
                    return results;
                    

                    
              


                    
                }
                else { return new List<List<string>> { new List<string>(), new List<string>() }; }
                
            }
            catch {

                return new List<List<string>> { new List<string>(), new List<string>() };
            }
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            this.Title = "Loading ...";
            this.IsEnabled = false;
            var s = (SearchBar)sender;
            var word = s.Text;
            s.Text = "";
            CurWord = word;
            Task.Factory.StartNew(() =>
            { return GutenSearch(word); })
           .Unwrap()
           .ContinueWith(task =>
           {
               ReloadAffichage(task.Result);
           }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void ReloadAffichage(List<List<string>> response) {

            List<string> listb = response[0];
            List<string> listr = response[1];
            int count = 0;
       
            List<Booky> bs = new List<Booky> ();
            List<Booky> rs = new List<Booky>();

            foreach (var r in listb) {
                var jsonData = (JObject)JsonConvert.DeserializeObject(r);
                var list = (JArray)jsonData["results"];
                foreach (JObject bb in list) {
                    count++;
                    if (bs.Count < 20) { 
                        string title = (string)bb["title"];
                        string img = (string)bb["formats"]["image/jpeg"];
                        string href = (string)bb["formats"]["text/html"];
                        bs.Add(new Booky(title, img, href));
                    }
                }
            }

            foreach (var r in listr)
            {
                var jsonData = (JObject)JsonConvert.DeserializeObject(r);
                var list = (JArray)jsonData["results"];
                foreach (JObject bb in list)
                {
                   
                    if (rs.Count < 20)
                    {
                        string title = (string)bb["title"];
                        string img = (string)bb["formats"]["image/jpeg"];
                        string href = (string)bb["formats"]["text/html"];
                        rs.Add(new Booky(title, img, href));
                    }
                }
            }
            this.Title = "Search ("+count+")";
            this.IsEnabled = true;
            resultlabel.Text = "Results ("+ CurWord+"): ";
            mylist.FlowItemsSource = bs;
            myrlist.FlowItemsSource = rs;
        }

        private List<string> GetURLS(string root, string l) { 
            List<string> list = new List<string>(l.Split(','));
            List<string> result= new List<string>();
            int i = 0;
            string s = root;
            if (list.Count <11) {
                foreach (var item in list)
                {
                    if (i == list.Count-1)
                    {
                        s += item;
                        result.Add(s);
                        s = root;
                        i = 0;
                    }
                    else
                    {
                        s += item + ",";
                        i++;
                    }

                }

            }
            foreach (var item in list) {
                if (i == 9) {
                    s += item;
                    result.Add(s);
                    s = root;
                    i=0;
                }
                else { 
                s += item + ",";
                i++;
                }

            }
            return result;
        }

        private string GetRoot() {
            string root = "";
            if (DeviceInfo.DeviceType == DeviceType.Physical) root = REALAPI;
            else root = API;
            string but = CurButton.Text;
            if (but.Equals("Simple Search")) root += "/simplesearch/?word=";
            if (but.Equals("Multiple Search")) root += "/multiplesearch/?words=";
            if (but.Equals("RegEx Search")) root += "/regexsearch/?regex=";
            return root;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            string but = CurButton.Text;
            if (but.Equals("Simple Search") || but.Equals("RegEx Search")) { 
                var s = (SearchBar)sender;
                var st = e.NewTextValue;
                List<string> list = new List<string>(st.Split(' '));
                s.Text = list[0];
            }

        }

   
    }


    public class Booky
    {
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public string HRef { get; set; }

        public Booky(string t, string img, string href)
        {
            Name = t;
            ImageSource = img;
            HRef = href;
        }
        public ICommand ClickCommand => new Command<string>((url) =>
        {
            if(!string.IsNullOrWhiteSpace(url))
                Device.OpenUri(new System.Uri(url));
        });
    }

    public class ResponseModel
    {
        [JsonProperty("books")]
        public string books { get; set; }
        [JsonProperty("suggestions")]
        public string suggestions { get; set; }
    }



    public class Author
    {
        public string name { get; set; }
        public int? birth_year { get; set; }
        public int? death_year { get; set; }
    }
    public class Translator
    {
        public string name { get; set; }
        public int? birth_year { get; set; }
        public int? death_year { get; set; }
    }

    public class Formats
    {
        [JsonProperty("text/plain; charsetutf-8")]
        public string TextPlainCharsetutf8 { get; set; }

        [JsonProperty("application/zip")]
        public string ApplicationZip { get; set; }

        [JsonProperty("image/jpeg")]
        public string ImageJpeg { get; set; }

        [JsonProperty("text/html; charsetutf-8")]
        public string TextHtmlCharsetutf8 { get; set; }

        [JsonProperty("text/html")]
        public string TextHtml { get; set; }

        [JsonProperty("application/x-mobipocket-ebook")]
        public string ApplicationXMobipocketEbook { get; set; }

        [JsonProperty("application/rdf+xml")]
        public string ApplicationRdfXml { get; set; }

        [JsonProperty("application/epub+zip")]
        public string ApplicationEpubZip { get; set; }

        [JsonProperty("text/plain")]
        public string TextPlain { get; set; }

        [JsonProperty("text/plain; charsetus-ascii")]
        public string TextPlainCharsetusAscii { get; set; }

        [JsonProperty("application/octet-stream")]
        public string ApplicationOctetStream { get; set; }

        [JsonProperty("text/html; charsetus-ascii")]
        public string TextHtmlCharsetusAscii { get; set; }

        [JsonProperty("text/html; charsetiso-8859-1")]
        public string TextHtmlCharsetiso88591 { get; set; }
    }


    public class Result
    {
        public int id { get; set; }
        public string title { get; set; }
        public List<Author> authors { get; set; }
        public List<Translator> translators { get; set; }
        public List<string> subjects { get; set; }
        public List<string> bookshelves { get; set; }
        public List<string> languages { get; set; }
        public bool? copyright { get; set; }
        public string media_type { get; set; }
        public Formats formats { get; set; }
        public int download_count { get; set; }
    }

    public class GutenResponse
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<Result> results { get; set; }
    }
}