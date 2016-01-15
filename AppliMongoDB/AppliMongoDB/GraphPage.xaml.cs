using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppliMongoDB
{
    /// <summary>
    /// Logique d'interaction pour GraphPage.xaml
    /// </summary>
    public partial class GraphPage : Window
    {
        List<BsonDocument> stats;
        public GraphPage(List<BsonDocument> list)
        {
            InitializeComponent();
            stats = list;
        }
    }
}
