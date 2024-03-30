﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EaterCake
{




    /// <summary>
    /// ModuleHP.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleHP : UserControl
    {
        public ModuleHP()
        {
            InitializeComponent();
            
        }

        public void SetDataContext(object data)
        {
            this.DataContext = data;
            
        }

        public void UpdateFontSize(double size)
        {
            MonsterName.FontSize = 8 + size;
            MonsterHP.FontSize = 8 + size;
            MonsterProgressText.FontSize = 6 + size;
        }


        

    }
}
