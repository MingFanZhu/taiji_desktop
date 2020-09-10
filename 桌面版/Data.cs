using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace desktop
{
    class Data
    {
        /// <summary>
        /// 全局变量
        /// </summary>
        public static Data.Cut_sentence_result cut_sentence_result = new Cut_sentence_result();
        public static List<Data.Sen_res> sen_res = new List<Data.Sen_res>();
        public static List<Data.Cut_word_result> cut_word_result = new List<Data.Cut_word_result>();
        public static List<Data.Part_of_speech_analysis_result> part_of_speech_analysis_results = new List<Data.Part_of_speech_analysis_result>();
        public static List<Data.Syntactic_analysis_result> syntactic_analysis_results = new List<Data.Syntactic_analysis_result>();

        public static Dictionary<string, string> config = new Dictionary<string, string>();

        public static void load_config()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("data/config.xml");
            XmlNode root = xmlDocument.SelectSingleNode("root");
            foreach(XmlNode node in root.ChildNodes)
            {
                Data.config.Add(node.Name, node.InnerText);
            }
        }

        public static void save_config()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("data/config.xml");
            XmlNode root = xmlDocument.SelectSingleNode("root");
            foreach (XmlNode node in root.ChildNodes)
            {
                node.InnerText = Data.config[node.Name];
            }
            xmlDocument.Save("data/config.xml");
        }

        /// <summary>
        /// 解析json数据需要的类
        /// </summary>
        public class Once
        {
            public List<List<List<Cut_sentence_result.Sentence>>> potentials{ get; set; }
            public List<Sen_res> sen_res{ get; set; }
        }
        
        public class Cut_sentence_result
        {
            public List<List<List<Sentence>>> potentials { get; set; }
            public class Sentence
            {
                public string sen { get; set; }
                public int sen_res_poi { get; set; }
            }
        }

        public class Cut_word_result
        {
            public List<string> words { get; set; }
        }

        public class Part_of_speech_analysis_result
        {
            public List<string> postags { get; set; }
        }

        public class Syntactic_analysis_result
        {
            public List<List<string>> relation { get; set; }
        }

        public class Main_component_result
        {
            public List<List<string>> main_component { get; set; }
        }

        public class Sen_res
        {
            public int potential_poi { get; set; }
            public int command_poi { get; set; }
            public int sentence_poi { get; set; }
            public List<List<string>> remake_res { get; set; }

            public Sen_res(int i,int j,int k, List<List<string>> main_Component_Result)
            {
                this.potential_poi = i;
                this.command_poi = j;
                this.sentence_poi = k;
                this.remake_res = main_Component_Result;
            }
        }

        public static void save_main_data(string file_path)
        {
            Once save_data = new Once();
            save_data.potentials = Data.cut_sentence_result.potentials;
            save_data.sen_res = Data.sen_res;
            File.WriteAllText(file_path, JsonConvert.SerializeObject(save_data));
        }

        public static void load_main_data(string file_path)
        {
            string text = File.ReadAllText(file_path);
            Once once = JsonConvert.DeserializeObject<Data.Once>(text);
            Data.cut_sentence_result.potentials = once.potentials;
            Data.sen_res = once.sen_res;
        }

        /// <summary>
        /// 读取字典需要的类
        /// </summary>
        public class Code
        {
            public List<LimbsItem> Limbs { get; set; }
            public List<DirectionItem> direction { get; set; }
            public List<DescriptionItem> description { get; set; }

            public class LimbsItem
            {
                public int id { get; set; }
                public List<string> name { get; set; }
                public string code { get; set; }
            }

            public class DirectionItem
            {
                public List<string> name { get; set; }
                public string code { get; set; }
            }

            public class DescriptionItem
            {
                public List<string> name { get; set; }
                public double code { get; set; }
            }
        }
    }
}
