using EaterCake.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EaterCake
{
    public static class MonsterHPHelper
    {
        public static bool IsFocusGame = false;

        public static GameType CurrentGameType = GameType.EATERCAKE;

        public static IntPtr CurrentProcess;

        public static IntPtr CurrentMonsterAddress;

        public static IntPtr BaseAddress;

        public static Stopwatch sw = new Stopwatch();


        public static readonly IntPtr DEFAULT_GE2_BASEADDRESS = new IntPtr(0x400000 + 0x12A6E78);

        public static readonly IntPtr DEFAULT_GE1_BASEADDRESS = new IntPtr(0x400000 + 0x131C7C8);

        public static MonsterHPModel[] models;

        public static DispatcherTimer BusTimer;

        public static Dictionary<byte,string> Game1MonsterIDMap = new Dictionary<byte,string>();
        public static Dictionary<byte, string> Game2MonsterIDMap = new Dictionary<byte, string>();


        private static int GlobalAddressRefreshCount = 0;


        static MonsterHPHelper()
        {

            models = new MonsterHPModel[16];

            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new MonsterHPModel(CurrentMonsterAddress);



            }


            BuildMonsterIDMap();


            BusTimer = new DispatcherTimer();
            BusTimer.Interval = TimeSpan.FromMilliseconds(1);
            BusTimer.Tick += BusTimerEvent;

            sw.Start();

        }



        private static void BuildMonsterIDMap()
        {

            string[] ge1_str = Resources.Monster_ID_Name_1_CN.Split(new[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ge2_str = Resources.Monster_ID_Name_2_CN.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var i in ge1_str)
            {

                string[] kv_str = i.Split(':');

                Console.WriteLine($"GE1:\nKEY={kv_str[0]}\nValue={kv_str[1]}");
                byte id_byte = Convert.ToByte(kv_str[0],16);
                Game1MonsterIDMap.Add(id_byte, kv_str[1]);
                

            }

            foreach (var i in ge2_str)
            {

                string[] kv_str = i.Split(':');
                Console.WriteLine($"GE2:\nKEY={kv_str[0]}\nValue={kv_str[1]}");
                byte id_byte = Convert.ToByte(kv_str[0],16);
                Game2MonsterIDMap.Add(id_byte, kv_str[1]);
                

            }


        }



        private static void BusTimerEvent(object sender, EventArgs e)
        {



            Console.WriteLine($"Memory Bus Update. Cost:{sw.ElapsedMilliseconds}");
            sw.Restart();
            if (++GlobalAddressRefreshCount >= 5)
            {
                //Console.WriteLine($"Update Bus Address.");
                GlobalAddressRefreshCount = 0;
                UpdateMonsterGroupAddress();

                if (CurrentMonsterAddress != IntPtr.Zero)
                {
                    if (SetModelsAddress())
                    {

                    }
                }
            }
            Parallel.ForEach<MonsterHPModel>(models, i =>
            {

                i.Update();

            });
            //Parallel.ForEach<MonsterHPModel>(models, i => {

            //    try { 
            //    i.Update();
            //    }catch(Exception) {
            //        i.SetErrorStatus();
            //    }
            //});


        }


        private static bool SetModelsAddress()
        {

            byte[] bytes = new byte[64];
            int readnumber = 0;

            try
            {

                Console.WriteLine($"Read Area From :{CurrentMonsterAddress.ToString("X")}");
                Win32.ReadProcessMemory(CurrentProcess, CurrentMonsterAddress, bytes, 64, out readnumber);
                Console.WriteLine($"Read Done.");
                if (readnumber != bytes.Length)
                {
                    Console.WriteLine($"readnumber {readnumber} != {bytes.Length}");
                    return false;
                }
                else
                {

                    for (int i = 0; i < 16; i++)
                    {

                        int data = BitConverter.ToInt32(bytes, i * 4);

                        Console.WriteLine($"Set {i} Address = {data.ToString("X")}");

                        models[i].Update(new IntPtr(data));
                    }



                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }

        }


        private static void UpdateMonsterGroupAddress()
        {


            if (CurrentGameType == GameType.GE0 || CurrentGameType == GameType.EATERCAKE || CurrentGameType == GameType.GE3)
            {
                Console.WriteLine($"Unknown Game.");
                return;
            }
            else
            {
                Console.WriteLine($"Update Game Address:{CurrentGameType}");
            }


            IntPtr p0, p1, p2, p3;
            p0 = BaseAddress;

            try
            {

                Console.WriteLine($"Base Monster Address:{p0.ToString("X")}.");

                int readnumber = 0;

                Win32.ReadProcessMemory(CurrentProcess, p0, out p1, 4, out readnumber);

                Console.WriteLine($"Current P1 Monster Address:{p1.ToString("X")},Read Number:{readnumber}");

                p1 += 0x28;

                Console.WriteLine($"Current P1+0X28 Monster Address:{p1.ToString("X")},Read Number:{readnumber}");

                Win32.ReadProcessMemory(CurrentProcess, p1, out p2, 4, out readnumber);
                Console.WriteLine($"Current P2 Monster Address:{p2.ToString("X")},Read Number:{readnumber}");
                p2 += 0x48;
                Console.WriteLine($"Current P2+0X48 Monster Address:{p2.ToString("X")},Read Number:{readnumber}");
                Win32.ReadProcessMemory(CurrentProcess, p2, out p3, 4, out readnumber);




                CurrentMonsterAddress = p3;

                Console.WriteLine($"Set Last Monster Address:{CurrentMonsterAddress.ToString("X")},Read Number:{readnumber}");

            }
            catch (Exception)
            {


                CurrentMonsterAddress = IntPtr.Zero;


            }


        }

        private static void SetGameBaseAddress(GameType game)
        {
            CurrentGameType = game;

            switch (CurrentGameType)
            {
                case GameType.GE1:
                    BaseAddress = DEFAULT_GE1_BASEADDRESS;
                    SetGameNoTopMost();
                    break;
                case GameType.GE2:
                    BaseAddress = DEFAULT_GE2_BASEADDRESS;
                    SetGameNoTopMost();
                    break;
                //case GameType.GE0:
                //    BaseAddress = new IntPtr(0x0);
                //    break;
                //case GameType.EATERCAKE:
                //    BaseAddress = new IntPtr(0x0);
                //    break;
                default:
                    BaseAddress = IntPtr.Zero;
                    break;
            }



        }

        public static void SetGameNoTopMost()
        {
            Console.WriteLine($"Get Game Window Handle:{MainWindow.foregroundWindowId}");

            Win32.SetWindowPos(MainWindow.foregroundWindowId, -2, 0, 0, 0, 0, 0x01 | 0x02 | 0x08 | 0x10);
            int windowStyle = Win32.GetWindowLong(MainWindow.foregroundWindowId, Win32.GWL_EXSTYLE);

            Console.WriteLine($"Get Game Window Style:{windowStyle}");

            windowStyle &= Win32.WS_EX_TOPMOST;

            Win32.SetWindowLong(MainWindow.foregroundWindowId, Win32.GWL_EXSTYLE, windowStyle);


        }

        public static bool SetGameBaseAddress(string filename)
        {

            switch (filename.ToLower())
            {
                case "ger.exe":
                    SetGameBaseAddress(GameType.GE1);
                    Console.WriteLine($"Set Game {filename}(God Eater 1)");
                    return true;
                case "ge2rb.exe":
                    SetGameBaseAddress(GameType.GE2);
                    Console.WriteLine($"Set Game {filename}(God Eater 2)");
                    return true;
                case "eatercake":
                    SetGameBaseAddress(GameType.EATERCAKE);
                    Console.WriteLine($"Set Game {filename} (Eater Cake)");
                    return true;
                default:
                    SetGameBaseAddress(GameType.GE0);
                    return false;
            }

        }






    }



}
