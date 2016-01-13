using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();

            client = new MyMongoClient(); 

            question_431.Click += Question_1_Click;
        }

        private void Question_1_Click(object sender, RoutedEventArgs e)
        {
            string title = titleBox.Text;
            
            List<DocMongo> list = client.FindArticles(title);
            //MessageBox.Show("Successfully find " + list.Count + " articles");
            string url = "";
            foreach(DocMongo doc in list)
            {
                if(doc.url != null)
                {
                    try
                    {
                        //System.Diagnostics.Process.Start(doc.url);
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                    //MessageBox.Show("Successfully connected to "+doc.url);
                    break;
                }
            }

            List<string> cites = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].cites != null)
                    for (int j = 0; j < list[i].cites.Count; j++)
                    {
                        cites.Add(list[i].cites[j]);
                       // listBoxVueArticle.Items.Add(list[i].cites[j]);
                    }
            }

            int count = 0;
            //var dico = client.FindAllTitlesById();
            for (int i = 0; i < cites.Count; i++)
            {
               
                DocMongo curDoc = client.FindById(cites[i]);
                if (curDoc != null && curDoc.title != null)
                {
                    count++;
                    listBoxVueArticle.Items.Add(cites[i] + " : " + curDoc.title);
                    if (count == 10)
                        break;
                }
            }

        }
    }
}
