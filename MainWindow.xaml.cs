using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Configuration;
using static EaterCake.Win32;
using System.IO;
using System.Reflection;

namespace EaterCake
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        
        IntPtr foregroundWindowId;

        

        WinEventDelegate dele = null;

        public static FloatingWindowHP WindowHP = new FloatingWindowHP();



        public MainWindow()
        {

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            InitializeComponent();



            dele = new Win32.WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

            if (String.IsNullOrEmpty(Text_WindowH.Text))
            {
                InitDialog();
                
            }

            Text_WindowH.Text = ConfigurationManager.AppSettings["Window_Height"];
            Text_WindowW.Text = ConfigurationManager.AppSettings["Window_Width"];
            Text_FontSize.Text = ConfigurationManager.AppSettings["Window_FontSize"];


   
                RefreshDialog();
            

            
            WindowHP.Show();



        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {

            try
            {
                MonsterHPHelper.IsFocusGame = MonsterHPHelper.SetGameBaseAddress(GetActiveAppName());
                GetWindowThreadProcessId(foregroundWindowId, out IntPtr activeAppProcessId);
                MonsterHPHelper.CurrentProcess = Win32.OpenProcess(Win32.ProcessAccessFlags.PROCESS_ALL_ACCESS,true, (int)activeAppProcessId);
                Console.WriteLine($"Set Current Process:{MonsterHPHelper.CurrentProcess}");
            }
            catch(Exception) { 
            
                MonsterHPHelper.SetGameBaseAddress("");
                MonsterHPHelper.CurrentProcess = IntPtr.Zero;

            };

            SetDialogStatus();

        }



        private void SaveAppConfig()
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            

            WindowHP.Height = Convert.ToDouble(Text_WindowH.Text);
            WindowHP.Width = Convert.ToDouble(Text_WindowW.Text);



            config.AppSettings.Settings["Window_Height"].Value = WindowHP.Height.ToString();
            config.AppSettings.Settings["Window_Width"].Value = WindowHP.Width.ToString();

            config.AppSettings.Settings["Window_X"].Value = WindowHP.Left.ToString();
            config.AppSettings.Settings["Window_Y"].Value = WindowHP.Top.ToString();

            config.AppSettings.Settings["Window_FontSize"].Value = Text_FontSize.Text;


            config.Save(ConfigurationSaveMode.Full);

            ConfigurationManager.RefreshSection("appSettings");

        }

  

        private void RefreshDialog()
        {

            double height, width, left, top,fontsize;
            

            height = Convert.ToDouble(ConfigurationManager.AppSettings["Window_Height"]);
            width = Convert.ToDouble(ConfigurationManager.AppSettings["Window_Width"]);
            left = Convert.ToDouble(ConfigurationManager.AppSettings["Window_X"]);
            top = Convert.ToDouble(ConfigurationManager.AppSettings["Window_Y"]);

            fontsize = Convert.ToDouble(ConfigurationManager.AppSettings["Window_FontSize"]);


            WindowHP.Height = height;
            WindowHP.Width = width;
            WindowHP.Top = top;
            WindowHP.Left = left;

            //foreach(var i in WindowHP.modules)
            //{
            //    i.UpdateFontSize(fontsize);
            //}
            
            for(int i=0;i<WindowHP.modules.Length;i++)
            {
                WindowHP.modules[i].UpdateFontSize(fontsize);
            }

            


        }

        private void InitDialog()
        {

            var currentdir = Directory.GetCurrentDirectory();

            ExtractFile("EaterCake.app.config", currentdir + "\\EaterCake.exe.config");
            ConfigurationManager.RefreshSection("appSettings");




        }

        private void ExtractFile(String resource, String path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            BufferedStream input = new BufferedStream(assembly.GetManifestResourceStream(resource));
            FileStream output = new FileStream(path, FileMode.Create);
            byte[] data = new byte[1024];
            int lengthEachRead;
            while ((lengthEachRead = input.Read(data, 0, data.Length)) > 0)
            {
                output.Write(data, 0, lengthEachRead);
            }
            output.Flush();
            output.Close();
        }


        private void SetDialogStatus()
        {
            try { 

            if(MonsterHPHelper.IsFocusGame)
            {

                

                WindowHP.Visibility = Visibility.Visible;
                WindowHP.timer.Start();
                MonsterHPHelper.BusTimer.Start();
                

            }
            else
            {
                WindowHP.Visibility = Visibility.Collapsed;
                WindowHP.timer.Stop();
                MonsterHPHelper.BusTimer.Stop();
            }
            }
            catch (Exception)
            {
                WindowHP.timer.Stop();
                MonsterHPHelper.BusTimer.Stop();
            }


        }

        private string GetActiveAppName()
        {
            foregroundWindowId = GetForegroundWindow();
            
            GetWindowThreadProcessId(foregroundWindowId, out IntPtr activeAppProcessId);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, (int)activeAppProcessId);
            uint lpdwSize = 1024;
            StringBuilder lpExeName = new StringBuilder((int)lpdwSize);
            QueryFullProcessImageName(hprocess, 0, lpExeName, ref lpdwSize);
            var exePath = lpExeName.ToString();
            FileVersionInfo appInfo = FileVersionInfo.GetVersionInfo(exePath);
            return String.IsNullOrEmpty(appInfo.FileDescription) ? System.IO.Path.GetFileName(exePath) : appInfo.FileDescription;
        }


        public void SetModulesGridStyles()
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (MessageBox.Show("你要是关上了就没办法在游戏里显示悬浮窗了！", "确定吗？", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {


                WindowHP.closeLocker = false;
                WindowHP.Close();
                
                e.Cancel = false;
                return;
            }

            e.Cancel = true;
            return;



        }

        private void WindowW_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
            Console.WriteLine(e.Text);
        }



        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            SaveAppConfig();
            RefreshDialog();

        }


        private bool IsTextAllowed(string text)
        {
            foreach (char c in text)
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
