using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace desktop
{
    class Five_tuples
    {
        public static Data.Code Limb_data;
        public static void get_data()
        {
            string text = File.ReadAllText("data/data.json");
            Limb_data = JsonConvert.DeserializeObject<Data.Code>(text);
        }
        //获取下标
        public static int get_poi_limb(string name, List<Data.Code.LimbsItem> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].name.IndexOf(name) != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int get_poi_dir(string name, List<Data.Code.DirectionItem> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].name.IndexOf(name) != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int get_poi_dis(string name, List<Data.Code.DescriptionItem> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].name.IndexOf(name) != -1)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int get_poi_by_code(string code)
        {
            for (var i = 0; i < Limb_data.Limbs.Count; i++)
            {
                if (Limb_data.Limbs[i].code == code)
                {
                    return i;
                }
            }
            return -1;
        }
        //计算五元组
        public static string get_four_tuples(int id, string dir, double dis, string time)
        {
            string split_dir = dir.Split(',')[1];
            if (dir.Split(',')[0] == "0")
            {
                //平移
                string four_tuples_res = Limb_data.Limbs[id].code + ',' + dir + ',' + dis.ToString() + ',' + time;
                return four_tuples_res;
            }
            else
            {
                //旋转
                string four_tuples_res = Limb_data.Limbs[id].code + ',' + dir + ',' + dis.ToString() + ',' + time;
                return four_tuples_res;
            }
        }
        //判断是否为数字字符串
        public static bool IsNumeric(string str) //接收一个string类型的参数,保存到str里
        {
            if (str == null || str.Length == 0)    
                return false;                           
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] bytestr = ascii.GetBytes(str.Replace(".",""));         

            foreach (byte c in bytestr)                   
            {
                if (c < 48 || c > 57)                          
                {
                    return false;                              
                }
            }
            return true;                                        
        }
        //计算入口
        public static List<List<List<string>>> start_cal()
        {
            get_data();
            List<List<List<string>>> four_tuples = new List<List<List<string>>>();
            foreach (var potential in Data.cut_sentence_result.potentials)
            {
                List<List<string>> four_tuples_command = new List<List<string>>();
                foreach (var command in potential)
                {
                    List<string> four_tuples_sen = new List<string>();
                    string time = "30";
                    foreach (var sen in command)
                    {
                        if (sen.sen.IndexOf('缓') != -1)
                        {
                            time = "45";
                        }
                    }
                    foreach (var sen in command)
                    {
                        Data.Sen_res temp_res = Data.sen_res[sen.sen_res_poi];
                        List<List<string>> remake_sens = temp_res.remake_res;
                        for (int i = 0; i < remake_sens.Count; i++)
                        {
                            int limb_i = get_poi_limb(remake_sens[i][0], Limb_data.Limbs);
                            string dir = "";
                            var dir_i = get_poi_dir(remake_sens[i][1], Limb_data.direction);
                            if (limb_i == -1 || dir_i == -1)
                            {
                                continue;
                            }
                            if (Limb_data.direction[dir_i].code.IndexOf("detail") != -1)
                            {
                                if (Limb_data.direction[dir_i].code.IndexOf("before") != -1)
                                {
                                    if (remake_sens[i][0].IndexOf('左') != -1)
                                    {
                                        dir = Limb_data.direction[dir_i].code.Replace("detail_before", "11");
                                    }
                                    else
                                    {
                                        dir = Limb_data.direction[dir_i].code.Replace("detail_before", "10");
                                    }
                                }
                                if (Limb_data.direction[dir_i].code.IndexOf("after") != -1)
                                {
                                    if (remake_sens[i][2].IndexOf('左') != -1)
                                    {
                                        dir = Limb_data.direction[dir_i].code.Replace("detail_after", "11");
                                    }
                                    else
                                    {
                                        dir = Limb_data.direction[dir_i].code.Replace("detail_after", "10");
                                    }
                                }
                            }
                            else
                            {
                                if (Limb_data.direction[dir_i].code.IndexOf('#') != -1)
                                {
                                    string[] sps = Limb_data.direction[dir_i].code.Split('#');
                                    string temp_code = "";
                                    if (remake_sens[i][0].IndexOf('左') != -1)
                                    {
                                        temp_code = "20";
                                    }
                                    else
                                    {
                                        temp_code = "21";
                                    }
                                    dir = sps[0] + ',' + temp_code;
                                }
                                else
                                {
                                    dir = Limb_data.direction[dir_i].code;
                                }
                            }
                            double dis = -1.0;
                            if (remake_sens[i][2].IndexOf('度')!=-1)
                            {
                                dis = Double.Parse(remake_sens[i][2].Replace("度",""));
                            }
                            else
                            {
                                if (IsNumeric(remake_sens[i][2]))
                                {
                                    dis = Double.Parse(remake_sens[i][2]);
                                }
                                else
                                {
                                    int des_i = get_poi_dis(remake_sens[i][2], Limb_data.description);
                                    if (des_i == -1)
                                    {
                                        if (dir.Split(',')[0] == "0")
                                        {
                                            dis = 0.1;
                                        }
                                        else
                                        {
                                            dis = 15;
                                        }
                                    }
                                    else
                                    {
                                        dis = Limb_data.description[des_i].code;
                                    }
                                }
                            }
                            if (Limb_data.Limbs[limb_i].id != -1)
                            {
                                if (dir.IndexOf("|") != -1)
                                {
                                    string[] dirs = dir.Split('|');
                                    foreach (string temp_dir in dirs)
                                    {
                                        four_tuples_sen.Add(get_four_tuples(limb_i, dir, dis, time));
                                    }
                                }
                                else
                                {
                                    four_tuples_sen.Add(get_four_tuples(limb_i, dir, dis, time));
                                }
                            }
                            else
                            {
                                List<int> ids = new List<int>();
                                string[] codes = Limb_data.Limbs[limb_i].code.Split('|');
                                for (int item = 0; item < codes.Length; item++)
                                {
                                    ids.Add(get_poi_by_code(codes[item]));
                                }
                                for (int item = 0; item < ids.Count; item++)
                                {
                                    four_tuples_sen.Add(get_four_tuples(ids[item], dir, dis, time));
                                }
                            }
                        }
                    }
                    four_tuples_command.Add(four_tuples_sen);
                }
                four_tuples.Add(four_tuples_command);
            }
            return four_tuples;
        }
    }
}
