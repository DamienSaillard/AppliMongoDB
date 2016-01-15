using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppliMongoDB
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyMongoClient client;
        List<DocMongo> listDoc;
        List<DocMongo> listAuth;

        public MainWindow()
        {
            InitializeComponent();

            client = new MyMongoClient();

            listDoc = new List<DocMongo>();
            listAuth = new List<DocMongo>();

            question_431.Click += Question_1_Click;
            open_browser.Click += Open_browser_Click;
            question_authors.Click += Question_authors_Click;
            show_stats.Click += Show_stats_Click;
        }

        #region  articles

        private void Open_browser_Click(object sender, RoutedEventArgs e)
        {
            foreach (DocMongo doc in listDoc)
            {
                if (doc.url != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(doc.url);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    MessageBox.Show("Successfully connected to "+doc.url);
                    break;
                }
            }
        }

        private void Question_1_Click(object sender, RoutedEventArgs e)
        {
            string title = titleBox.Text;
            
            listDoc = client.FindArticles(title);
            MessageBox.Show("Successfully find " + listDoc.Count + " articles");

            List<string> cites = new List<string>();
            for (int i = 0; i < listDoc.Count; i++)
            {
                if (listDoc[i].cites != null)
                    for (int j = 0; j < listDoc[i].cites.Count; j++)
                    {
                        string fgbhsghlisehgmroseighielshgsielg = listDoc[i].title;
                        cites.Add(listDoc[i].cites[j]);
                       // listBoxVueArticle.Items.Add(list[i].cites[j]);
                    }
            }

            if(cites.Count <= 0)
            {
                MessageBox.Show("No citation for the articles");
            }

            int count = 0;
            //var dico = client.FindAllTitlesById();
            for (int i = 0; i < cites.Count; i++)
            {
               
                DocMongo curDoc = client.FindById(cites[i]);
                if (curDoc != null && curDoc.title != null)
                {
                    count++;
                    listBoxVueArticle.Items.Add(cites[i] + " : " + curDoc.title + "(" + ((curDoc.authors != null)? curDoc.authors[0] : "no authors")+ ")");
                    if (count == 10)
                        break;
                }
            }

        }
        #endregion

        #region authors

        private void Question_authors_Click(object sender, RoutedEventArgs e)
        {
            string authorName = authors_name.Text;
            listDoc = client.FindArticlesByAuthor(authorName);
            MessageBox.Show("Successfully find " + listDoc.Count + " articles");

            int[] years = listDoc
                .Where(x => x.year != 0)
                .Select(x => x.year)
                .Distinct()
                .ToArray();
            for (int i = 0; i < years.Length; i++)
                years_combo_box.Items.Add(years[i]);
            years_combo_box.SelectionChanged += Years_combo_box_SelectionChanged;

            for (int i = 0; i < listDoc.Count; i++)
            {
                string line = formatItem(listDoc[i]);
                authors_listview.Items.Add(line);
            }
            show_co_authors.Click += Show_co_authors_Click;
        }

        private void Show_co_authors_Click(object sender, RoutedEventArgs e)
        {
            authors_listview.Items.Clear();
            List<string> coAuthors = new List<string>();

            for(int i = 0; i < listDoc.Count; i++)
            {
                for(int j = 0; j < listDoc[i].authors.Count; j++)
                {
                    if(coAuthors.IndexOf(listDoc[i].authors[j]) == -1)
                    {
                        coAuthors.Add(listDoc[i].authors[j]);
                    }
                }
            }
            coAuthors.Sort();
            authors_listview.Items.Clear();
            for (int i = 0; i < coAuthors.Count; i++)
            {
                authors_listview.Items.Add(coAuthors[i]);
            }
        }

        public string formatItem(DocMongo doc)
        {
            string authors = "";
            for (int j = 0; j < doc.authors.Count; j++)
                authors += doc.authors[j] + ", ";
            if (authors.Length > 0)
                authors = authors.Substring(0, authors.Length - 2);
            string pages = "";
            if (doc.pages != null && !(doc.pages is string))
            {
                pages += ((ExpandoObject)doc.pages).First().Value + " - " + ((ExpandoObject)doc.pages).First().Value;
            }
            string line = String.Format("{0} ({1}): [{2}], publisher:{3}, type:{4}, pages:{5}",
                doc.title, doc.year, authors, doc.publisher, doc.type, pages);
            return line;
        }

        private void Years_combo_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int a = 0;
            int.TryParse(cb.SelectedValue.ToString(), out a);
            if(a != 0)
            {
                authors_listview.Items.Clear();
                for(int i = 0; i < listDoc.Count; i++)
                {
                    if (listDoc[i].year == a)
                        authors_listview.Items.Add(formatItem(listDoc[i]));
                }
            }
        }


        private void Show_stats_Click(object sender, RoutedEventArgs e)
        {
            List<BsonDocument> stats = client.StatsByAuthor(authors_name.Text);

            new GraphPage(stats).Show();

            //authors_listview.Items.Clear();
            
        }

        #endregion
    }
}
