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
        List<DocMongo> listPub;

        public MainWindow()
        {
            InitializeComponent();

            client = new MyMongoClient();

            listDoc = new List<DocMongo>();
            listAuth = new List<DocMongo>();
            listPub = new List<DocMongo>();

            question_431.Click += Question_1_Click;
            open_browser.Click += Open_browser_Click;
            question_authors.Click += Question_authors_Click;
            show_stats.Click += Show_stats_Click;
            question_publisher.Click += Question_publisher_Click;
            show_publisher_stats.Click += Show_publisher_stats_Click;
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
            try
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

                if (cites.Count <= 0)
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
                        listBoxVueArticle.Items.Add(cites[i] + " : " + curDoc.title + "(" + ((curDoc.authors != null) ? curDoc.authors[0] : "no authors") + ")");
                        if (count == 10)
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region authors

        private void Question_authors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string authorName = authors_name.Text;
                listAuth = client.FindArticlesByAuthor(authorName);
                MessageBox.Show("Successfully find " + listAuth.Count + " articles");

                int[] years = listAuth
                    .Where(x => x.year != 0)
                    .Select(x => x.year)
                    .Distinct()
                    .ToArray();
                for (int i = 0; i < years.Length; i++)
                    years_combo_box.Items.Add(years[i]);
                years_combo_box.SelectionChanged += Years_combo_box_SelectionChanged;
                authors_listview.Items.Clear();
                for (int i = 0; i < listAuth.Count; i++)
                {
                    string line = formatItem(listAuth[i]);
                    authors_listview.Items.Add(line);
                }
                show_co_authors.Click += Show_co_authors_Click;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Show_co_authors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                authors_listview.Items.Clear();
                Dictionary<string, int> coAuthors = new Dictionary<string, int>();

                for (int i = 0; i < listAuth.Count; i++)
                {
                    for (int j = 0; j < listAuth[i].authors.Count; j++)
                    {
                        if (!coAuthors.ContainsKey(listAuth[i].authors[j]))
                        {
                            coAuthors.Add(listAuth[i].authors[j], 1);
                        }
                        else
                        {
                            coAuthors[listAuth[i].authors[j]]++;
                        }
                    }
                }
                coAuthors.OrderByDescending(x => x.Value);
                authors_listview.Items.Clear();
                foreach(var item in coAuthors)
                {
                    authors_listview.Items.Add(item.Key + ": " + item.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                for(int i = 0; i < listAuth.Count; i++)
                {
                    if (listAuth[i].year == a)
                        authors_listview.Items.Add(formatItem(listDoc[i]));
                }
            }
        }


        private async void Show_stats_Click(object sender, RoutedEventArgs e)
        {
            List<BsonDocument> stats = new List<BsonDocument>();
            try
            {
                string text = publisher_name.Text;
                await Task.Run(() =>
                {
                    stats = client.StatsByPublisher(text);

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                new GraphPage(stats).Show();
            }
        }

        #endregion

        #region publishers

        private void Question_publisher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string authorName = publisher_name.Text;
                listPub = client.FindArticlesByPublisher(authorName);
                MessageBox.Show("Successfully find " + listPub.Count + " articles");

                int[] years = listPub
                    .Where(x => x.year != 0)
                    .Select(x => x.year)
                    .Distinct()
                    .ToArray();
                for (int i = 0; i < years.Length; i++)
                    years_box_publisher_combo.Items.Add(years[i]);
                years_box_publisher_combo.SelectionChanged += Years_box_publisher_combo_SelectionChanged;

                publisher_listview.Items.Clear();
                for (int i = 0; i < listPub.Count; i++)
                {
                    string line = formatItem(listPub[i]);
                    publisher_listview.Items.Add(line);
                }
                show_publisher_authors.Click += Show_publisher_authors_Click;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Show_publisher_authors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                publisher_listview.Items.Clear();
                Dictionary<string, int> coAuthors = new Dictionary<string, int>();

                for (int i = 0; i < listPub.Count; i++)
                {
                    for (int j = 0; j < listPub[i].authors.Count; j++)
                    {
                        if (!coAuthors.ContainsKey(listPub[i].authors[j]))
                        {
                            coAuthors.Add(listPub[i].authors[j], 1);
                        }
                        else
                        {
                            coAuthors[listPub[i].authors[j]]++;
                        }
                    }
                }
                coAuthors.OrderByDescending(x => x.Value);
                publisher_listview.Items.Clear();
                foreach (var item in coAuthors)
                {
                    publisher_listview.Items.Add(item.Key + ": " + item.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Years_box_publisher_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int a = 0;
            int.TryParse(cb.SelectedValue.ToString(), out a);
            if (a != 0)
            {
                publisher_listview.Items.Clear();
                for (int i = 0; i < listPub.Count; i++)
                {
                    if (listPub[i].year == a)
                        publisher_listview.Items.Add(formatItem(listPub[i]));
                }
            }
        }


        private async void Show_publisher_stats_Click(object sender, RoutedEventArgs e)
        {
            List<BsonDocument> stats = new List<BsonDocument>();
            try
            {
                string text = publisher_name.Text;
                await Task.Run(() =>
                {
                    stats = client.StatsByPublisher(text);
                    
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                new GraphPage(stats).Show();
            }
        }

        #endregion
    }
}
