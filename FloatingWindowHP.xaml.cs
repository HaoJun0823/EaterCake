using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EaterCake
{
    /// <summary>
    /// FloatingWindowHP.xaml 的交互逻辑
    /// </summary>
    public partial class FloatingWindowHP : Window
    {


        public bool closeLocker = true;

        public IntPtr windowHandle;
        public DispatcherTimer timer;


        public ModuleHP[] modules = new ModuleHP[16];

        public FloatingWindowHP()
        {
            InitializeComponent();

            int count = 0;
            for (int x = 0; x < 4; x++)
            {

                for (int y = 0; y < 4; y++)
                {
                    ModuleHP module = new ModuleHP();
                    modules[count++] = module;
                    Console.WriteLine($"Set {count} HP Module.");
                    RootLayout.Children.Add(module);

                    Grid.SetRow(module, x);
                    Grid.SetColumn(module, y);

                }


            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);

            timer.Tick += TimerUpdateEvent;

            //Win32.SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010 | 0x0080);


            

        }


        public void TimerUpdateEvent(object sender, EventArgs e)
        {

            UpdateWindowLayout();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.MouseDown += WindowMouseDown;

            windowHandle = new WindowInteropHelper(this).Handle;

            Win32.SetWindowPos(windowHandle, -1, 0, 0, 400, 100, 0x0001 | 0x0002 | 0x0010 | 0x0080);
            Win32.SetForegroundWindow(windowHandle);
            SetTopMost();
            int windowStyle = Win32.GetWindowLong(windowHandle,Win32.GWL_EXSTYLE);

            windowStyle |= Win32.WS_EX_TOPMOST;

            Win32.SetWindowLong(windowHandle,Win32.GWL_EXSTYLE, windowStyle);


        }

        private void SetTopMost()
        {

            
            
            Win32.BringWindowToTop(windowHandle);


        }


        private void WindowMouseDown(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }



        }

        private void WindowMousePress(object sender, MouseEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = closeLocker;




        }

        private void UpdateWindowLayout()
        {

            //this.Topmost = true;
            //SetTopMost();
            int layoutsize = UpdateModuleVisiable();

            Console.WriteLine($"Current Layout:{layoutsize}");

            switch (layoutsize)
            {
                case 0:
                    Layout4();
                    break;
                case 1:
                    Layout4();
                    break;
                case 2:
                    Layout8();
                    break;
                //case 3:
                //    break;
                //case 4:
                //    break;
                default:
                    Layout16();
                    break;
            }



        }

        private void Layout4()
        {


            for (int x = 0; x < 4; x++)
            {
                Grid.SetRow(modules[x], x);
                Grid.SetColumn(modules[x], 0);
                Grid.SetColumnSpan(modules[x], 4);
            }



        }
        private void Layout8()
        {
            for (int x = 0; x < 4; x++)
            {
                Grid.SetRow(modules[x], x);
                Grid.SetColumn(modules[x], 0);
                Grid.SetColumnSpan(modules[x], 2);
            }

            for (int x = 4; x < 8; x++)
            {
                Grid.SetRow(modules[x], x);
                Grid.SetColumn(modules[x], 2);
                Grid.SetColumnSpan(modules[x], 2);
            }

        }

        private void Layout16()
        {

            int count = 0;

            for (int x = 0; x < 4; x++)
            {

                for (int y = 0; y < 4; y++)
                {
                    Grid.SetRow(modules[count], x);
                    Grid.SetColumn(modules[count], y);

                    Grid.SetColumnSpan(modules[count],1);
                    count++;


                }


            }

        }


        private int UpdateModuleVisiable()
        {
            int available = 0;

            for (int index = 0; index < modules.Length; index++)
            {
                //Console.WriteLine($"UI Update:{index}={MonsterHPHelper.models[index].address}");
                if (MonsterHPHelper.models[index].address != IntPtr.Zero)
                {

                    

                    available++;
                    modules[index].Visibility = Visibility.Visible;

                    modules[index].MonsterHP.Text = $"{MonsterHPHelper.models[index].HP}/{MonsterHPHelper.models[index].maxHP}";
                    modules[index].MonsterName.Text = $"{MonsterHPHelper.models[index].getMonsterNameById()}";

                    double percent = Convert.ToDouble(MonsterHPHelper.models[index].HP) / Convert.ToDouble(MonsterHPHelper.models[index].maxHP);
                    percent *= 100;
                    percent = Math.Round(percent, 2);




                    modules[index].MonsterProgressText.Text = $"{percent}%";
                    modules[index].MonsterProgressBar.Value = percent;


                }
                else
                {
                    modules[index].Visibility = Visibility.Collapsed;
                }


            }


            return available;

        }

    }
}
