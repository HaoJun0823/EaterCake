using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EaterCake
{
    /// <summary>
    /// FloatingWindowHP.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingWindowHP : Window
    {


        public List<ModuleHP> modules = new List<ModuleHP>();

        public FloatingWindowHP()
        {
            InitializeComponent();

            for(int x = 0; x < 4; x++)
            {

                for(int y=0; y < 4; y++)
                {
                    ModuleHP module = new ModuleHP();
                    modules.Add(module);
                    RootLayout.Children.Add(module);

                    Grid.SetRow(module,x);
                    Grid.SetColumn(module,y);

                }


            }
            

        }
    }
}
