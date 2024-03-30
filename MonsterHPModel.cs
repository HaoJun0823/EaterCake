using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EaterCake
{
    public class MonsterHPModel
    {

        public IntPtr address;

        public byte id;
        public int maxHP;

        public int HP;

 

        public MonsterHPModel(IntPtr address)
        {
            this.address = address;

        }


        public void Update(IntPtr address)
        {

            this.address = address;

            Update();




        }

        public void Update()
        {
            Task.Factory.StartNew(() =>
            {
                UpdateTaskFunction();
            });

            
        }

        public void UpdateTaskFunction()
        {

            //Console.WriteLine($"Update {MonsterHPHelper.CurrentGameType}.({MonsterHPHelper.CurrentGameType})");
            switch (MonsterHPHelper.CurrentGameType)
            {
                case GameType.EATERCAKE:

                    address = new IntPtr(0xFFFFFFF);
                    SetErrorStatus();
                    break;
                case GameType.GE1:
                    GetGEData();
                    break;
                case GameType.GE2:
                    GetGEData();
                    break;
                default:
                    address = IntPtr.Zero;
                    break;
            }
        }

        public void SetErrorStatus()
        {
            this.id = 0;
            this.maxHP = 1;
            this.HP = 0;
        }

        public string getMonsterNameById()
        {
            var map = MonsterHPHelper.Game1MonsterIDMap;

            if(MonsterHPHelper.CurrentGameType == GameType.GE2)
            {
                map = MonsterHPHelper.Game2MonsterIDMap;
            }
            string monster_name = "??";
            if (map.ContainsKey(this.id))
            {
                monster_name = map[this.id];
            }


            return $"{this.id.ToString("X2")}:{monster_name}";

        }

        public void GetGEData()
        {
            if(address == IntPtr.Zero)
            {
                return;
            }


            //+134
            IntPtr currentHP = address + 0x134;

            IntPtr p0;

            int ret = 0;

            Console.WriteLine($"Current Base Address:{address.ToString("X")}");
            //+114
            Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, address + 0x114, out p0, 4, out ret);
            Console.WriteLine($"Current P0 Address:{p0.ToString("X")},Read Number:{ret}");
            IntPtr p1;
            //+C
            Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, p0 +0xC, out p1, 4, out ret);
            Console.WriteLine($"Current P1 Address:{p1.ToString("X")},Read Number:{ret}");
            IntPtr currentID = p1 + 0x3;

            

            IntPtr currentMaxHP = p1 + 0xD8;

            if(MonsterHPHelper.CurrentGameType  == GameType.GE2)
            {
                currentMaxHP = p1 + 0x104;
                Console.WriteLine($"God Eater 2 + 0x104={currentMaxHP.ToString("X")}");
            }

            Console.WriteLine($"Get Address HP:{currentHP.ToString("X")},ID:{currentID.ToString("X")},MaxHP:{currentMaxHP.ToString("X")}");
            ReadDataFromMemory(currentHP, currentMaxHP, currentID);
        }

        private void ReadDataFromMemory(IntPtr hp,IntPtr maxhp,IntPtr id)
        {

            int ret;


            Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, hp, out this.HP, 4, out ret);
            Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, maxhp, out this.maxHP, 4, out ret);
            Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, id, out this.id, 4, out ret);

            Console.WriteLine($"Set HP={this.HP} MaxHP={this.maxHP} Id={this.id}");


        }

        //public void GetGE2Data()
        //{
        //    if (address == IntPtr.Zero)
        //    {
        //        return;
        //    }

        //    //+134
        //    IntPtr currentHP = address += 0x134;

        //    IntPtr p0;

        //    int ret = 0;

        //    //+114
        //    Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, address + 0x114, out p0, 4, out ret);

        //    IntPtr p1;
        //    //+C
        //    Win32.ReadProcessMemory(MonsterHPHelper.CurrentProcess, p0 + 0xC, out p1, 4, out ret);

        //    IntPtr currentID = p1 + 0x3;
        //    IntPtr currentMaxHP = p1 + 0x104;

        //    ReadDataFromMemory(currentHP, currentMaxHP, currentID);
        //}


    }
}
