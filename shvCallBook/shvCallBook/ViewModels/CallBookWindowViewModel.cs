////////////////////////////////////////////////////////////////////////////
//
// Epoxy template source code.
// Write your own copyright and note.
// (You can use https://github.com/rubicon-oss/LicenseHeaderManager)
//
////////////////////////////////////////////////////////////////////////////

using shvCallBook.Core.Models;
using Epoxy;
using Epoxy.Synchronized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace shvCallBook.ViewModels
{
    [ViewModel]
    public sealed class CallBookWindowViewModel
    {
        //------------------------------------------------------------------------------------------------------
        public static string filename_folder = @"callbook_data@";
        public static string filename_filedefinition = filename_folder + @"\filename_filedefinition.csv";
        public Dictionary<string, string> gOpenFileName;
        public List<ItemJccJcg> gItemJccJcg;
        public List<CallBookViewModel> gCallBookViewModel;
        //------------------------------------------------------------------------------------------------------
        public CallBookWindowViewModel()
        {
            int lastNumber = 0;
            this.Items = new ObservableCollection<CallBookViewModel>();
            this.WindowLoaded = Command.Factory.CreateSync(() =>
            {
                this.IsEnabledStart = true;
                gOpenFileName = ReadFileDefinition();
                gCallBookViewModel = Read_Radios_File();
                lastNumber = gCallBookViewModel.Count + 1;
                this.Items.Clear();
                this.Items = new ObservableCollection<CallBookViewModel>(gCallBookViewModel);
                message += "Window Loaded\n";
            });

            this.cmdStart = CommandFactory.Create(async () =>
            {
                this.IsEnabledStart = false;
                selectedIndex = 0;
                List<string> calllist = new List<string>();

                if (System.IO.File.Exists(callsignFile))
                {
                    using (StreamReader sr = new StreamReader(callsignFile, Encoding.UTF8, true))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#")) continue;
                            if (line == string.Empty) continue;
                            string callReq = line.Trim().Split("/")[0].Split(' ')[0];
                            if (!calllist.Contains(callReq)) calllist.Add(callReq);
                        }
                    }
                }

                foreach (string call1 in calllist)
                {
                    message += call1 + "\n";
                    oneCallSign = call1;
                    try
                    {
                        var callinfos = await CallBookRead.FetchNewCallsignAsync(call1);
                        if (callinfos == null)
                        {
                            message += call1 + " not found\n";
                            continue;
                        }

                        foreach (var callinfo in callinfos)
                        {
                            this.Items.Insert(0, new CallBookViewModel    //this.Items.Add(new CallBook...
                            {
                                No = lastNumber++,
                                Call_sign = callinfo.Call_sign,
                                Clubname = callinfo.Clubname,
                                License_expire = callinfo.License_expire,
                                Station_type = callinfo.Station_type,
                                Station_address = callinfo.Station_address,
                                Code_ken = callinfo.Code_ken,
                                Code_city = callinfo.Code_city,
                                Code_ku = callinfo.Code_ku,
                                Checked_date = callinfo.Checked_date,
                                F136kHz = callinfo.F136kHz,
                                F1910kHz = callinfo.F1910kHz,
                                F3537kHz = callinfo.F3537kHz,
                                F3798kHz = callinfo.F3798kHz,
                                F7100kHz = callinfo.F7100kHz,
                                F10125kHz = callinfo.F10125kHz,
                                F14175kHz = callinfo.F14175kHz,
                                F18118kHz = callinfo.F18118kHz,
                                F21225kHz = callinfo.F21225kHz,
                                F24940kHz = callinfo.F24940kHz,
                                F28850kHz = callinfo.F28850kHz,
                                F52MHz = callinfo.F52MHz,
                                F145MHz = callinfo.F145MHz,
                                F435MHz = callinfo.F435MHz,
                                F1280MHz = callinfo.F1280MHz,
                                F2425MHz = callinfo.F2425MHz,
                                F5750MHz = callinfo.F5750MHz,
                                F10125MHz = callinfo.F10125MHz,
                                F10475MHz = callinfo.F10475MHz,
                                F24025MHz = callinfo.F24025MHz,
                                F47100MHz = callinfo.F47100MHz,
                                F4630kHz = callinfo.F4630kHz
                            });
                        }
                    }
                    finally
                    {
                        IsEnabledStart = true;
                    }
                    await Task.Delay(3000);
                }
                gCallBookViewModel = this.Items.ToList();
            });

            this.cmdOpenFolder = Command.Factory.CreateSync(() =>
            {
                string s = Directory.GetCurrentDirectory();
                Process.Start("explorer.exe", s);
                message += s + "\n";
            });

            this.cmdSetCode = Command.Factory.CreateSync(() =>
            {
                gItemJccJcg = Read_JCCJCG_Data();
                gCallBookViewModel = SetJccJcgCode();
                Write_Radios_File(gCallBookViewModel);
                this.Items.Clear();
                this.Items = new ObservableCollection<CallBookViewModel>(gCallBookViewModel);
            });

            this.cmdSelectFile = Command.Factory.CreateSync(() =>
            {
                SelectCallsignFile();
            });
        }
        public List<ItemJccJcg> Read_JCCJCG_Data()
        {
            gItemJccJcg = new List<ItemJccJcg>();

            string[] fileNames = {
                GetFilenameFrom("jarl_jccjcg_jcc"),
                GetFilenameFrom("jarl_jccjcg_jcg"),
                GetFilenameFrom("jarl_jccjcg_ku")
            };

            foreach (string fileName1 in fileNames)
            {
                if (System.IO.File.Exists(fileName1))
                {
                    using (StreamReader sr = new StreamReader(fileName1, Encoding.UTF8, true))
                    {
                        //Debug.WriteLine(@"Read_JCCJCG_Data = " + fileName1);

                        string line;
                        int no_data = 0;
                        string[] results = new string[9];

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#")) continue;
                            if (line == string.Empty) continue;

                            string line1 = Regex.Replace(line, @"\s+", " ");
                            string line2 = Regex.Replace(line1, @"※", " ");
                            line2 = line2.Trim();

                            string[] fields = line2.Split(' ');

                            if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcc")) || fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcg")))    //JCC or JCG
                            {
                                if (fields[0] == "*")    // * 0704   Taira                平        "Sep.30,1966"   
                                {
                                    results[2] = fields[1];//0704
                                    results[3] = fields[2];//Taira
                                    results[4] = fields[3];//平
                                    results[5] = "";
                                    results[6] = "";
                                    results[7] = "";
                                    if (line1.Contains('"'))
                                    {
                                        string[] strDate = line1.Split('"');
                                        results[8] = strDate[1];//  "Sep.30,1966"
                                    }
                                    else
                                    {
                                        results[8] = string.Empty;
                                    }
                                    continue;//登録しない コメント
                                }

                                else if (fields[0].Length < 4 && fields[fields.Length - 1].Length == 2)  // 福 島  07    
                                {
                                    if (fields.Length.Equals(2))
                                    {
                                        results[0] = fields[1];//07
                                        results[1] = fields[0];//福島
                                    }
                                    else if (fields.Length.Equals(3))
                                    {
                                        results[0] = fields[2];//07
                                        results[1] = fields[0] + fields[1];//福+島
                                    }
                                    results[2] = string.Empty;
                                    results[3] = string.Empty;
                                    results[4] = string.Empty;
                                    results[5] = string.Empty;
                                    results[6] = string.Empty;
                                    results[7] = string.Empty;
                                    results[8] = string.Empty;
                                    continue;
                                }

                                else if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcc")) && fields[0].Length == 4 && fields.Length.Equals(3)) //  JCC  // 1501   Utsunomiya           宇都宮
                                {
                                    results[2] = fields[0];//1501
                                    results[3] = fields[1];//Utsunomiya
                                    results[4] = fields[2] + "市";//宇都宮
                                    results[5] = "";
                                    results[6] = "";
                                    results[7] = "";
                                    results[8] = string.Empty;
                                }

                                else if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcg")) && fields[0].Length == 5 && fields.Length.Equals(3)) //  JCG  //   15008   Haga                芳賀
                                {
                                    results[2] = fields[0];//15008
                                    results[3] = fields[1];//Haga
                                    results[4] = fields[2] + "郡";//芳賀
                                    results[5] = "";
                                    results[6] = "";
                                    results[7] = "";
                                    results[8] = string.Empty;
                                }

                                else if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcg")) && fields.Length.Equals(2)) //  JCG  //   Ozoracho            大空町
                                {
                                    /*
                                            01005   Abashiri            網走
                                                    Ozoracho            大空町
                                                    Bihorochou          美幌町
                                                    Tubetuchou          津別町
                                            01006   Abuta(Shiribeshi)   虻田(後志)
                                                    Kimobetsucho        喜茂別町
                                                    Kutchancho          倶知安町
                                    */
                                    results[5] = "";//Numberなし
                                    results[6] = fields[0];//Ozoracho
                                    results[7] = fields[1];//大空町
                                    results[8] = string.Empty;
                                }

                                else if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_jcc")) && fields[0].Length == 6 && fields.Length.Equals(3)) //  JCCの中にKU情報がある
                                {//  100104 Shinjuku        新宿
                                    results[2] = fields[0];//100104
                                    results[3] = fields[1];//Shinjuku
                                    results[4] = fields[2] + "区";//新宿
                                    results[5] = "";
                                    results[6] = "";
                                    results[7] = "";
                                    results[8] = string.Empty;
                                }

                                else
                                {
                                    Debug.WriteLine("Invalid Data:" + line2 + "\t" + fileName1);
                                    continue;
                                }
                            }

                            if (fileName1.Contains(GetFilenameFrom("jarl_jccjcg_ku")))    //KU
                            {
                                if (fields.Length.Equals(1)) //熊本市(4301)  
                                {
                                    string[] strCity = fields[0].Split('(');
                                    results[4] = strCity[0];
                                    results[2] = strCity[1].Replace(")", "");

                                    results[0] = gItemJccJcg.Find(x => x.citygun_no.Equals(results[2])).ken_no;
                                    results[1] = gItemJccJcg.Find(x => x.citygun_no.Equals(results[2])).ken_name;

                                    results[5] = String.Empty;
                                    results[6] = String.Empty;
                                    results[7] = String.Empty;
                                    results[8] = String.Empty;
                                    continue;//市のみは登録しない
                                }
                                else if (fields.Length.Equals(3)) //430101  Chuo 中央 //KU
                                {
                                    results[5] = fields[0];//430101   
                                    results[6] = fields[1];//Chuo 
                                    results[7] = fields[2] + "区";//中央
                                    results[8] = String.Empty;
                                }
                                else if (fields.Length.Equals(4)) //250105 Higashi 東 平成元年2月12日以前   //KU
                                {
                                    results[5] = fields[0];//250105   
                                    results[6] = fields[1];//Higashi 
                                    results[7] = fields[2];//東
                                    results[8] = fields[3];//平成元年2月12日以前
                                    continue;   //登録しない
                                }
                                else
                                {
                                    Debug.WriteLine("Invalid Data:" + line2 + "\t" + fileName1);
                                    continue;
                                }
                            }

                            no_data++;

                            ItemJccJcg logItem = new ItemJccJcg()
                            {
                                ken_no = results[0],
                                ken_name = results[1],
                                citygun_no = results[2],
                                citygun_name_roman = results[3],
                                citygun_name = results[4],
                                kutown_no = results[5],
                                kutown_name_roman = results[6],
                                kutown_name = results[7],
                                date_expired = results[8]
                            };

                            gItemJccJcg.Add(logItem);

                            string join = string.Join(", ", results);
                            //Debug.WriteLine(join);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File not found:" + fileName1);
                }
            }
            return gItemJccJcg;
        }
        public void SelectCallsignFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.PathSelectCallsignFile;
            dialog.Filter = "txt file (*.txt)|*.txt|All file (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                callsignFile = dialog.FileName;
            }  
            string file = System.IO.Path.GetFileName(callsignFile);
            string dirName = Path.GetDirectoryName(callsignFile);
            Properties.Settings.Default.PathSelectCallsignFile = dirName;
            Properties.Settings.Default.Save();
        }
        public List<CallBookViewModel> Read_Radios_File()
        {
            List<CallBookViewModel> Radiostation_Prev = new List<CallBookViewModel>();
            string file_radio_station = GetFilenameFrom("soumu_radio_station");

            if (File.Exists(file_radio_station))
            {
                using (StreamReader sr = new StreamReader(file_radio_station))
                {
                    string line;
                    int no_data = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');
                        no_data++;
                        CallBookViewModel logItem = new CallBookViewModel()
                        {
                            No = no_data,
                            Call_sign = fields[0],
                            Clubname = fields[1],
                            License_expire = fields[2],
                            Station_type = fields[3],
                            Station_address = fields[4],
                            Code_ken = fields[5],
                            Code_city = fields[6],
                            Code_ku = fields[7],
                            Checked_date = fields[8],
                            F136kHz = fields[9],
                            F1910kHz = fields[10],
                            F3537kHz = fields[11],
                            F3798kHz = fields[12],
                            F7100kHz = fields[13],
                            F10125kHz = fields[14],
                            F14175kHz = fields[15],
                            F18118kHz = fields[16],
                            F21225kHz = fields[17],
                            F24940kHz = fields[18],
                            F28850kHz = fields[19],
                            F52MHz = fields[20],
                            F145MHz = fields[21],
                            F435MHz = fields[22],
                            F1280MHz = fields[23],
                            F2425MHz = fields[24],
                            F5750MHz = fields[25],
                            F10125MHz = fields[26],
                            F10475MHz = fields[27],
                            F24025MHz = fields[28],
                            F47100MHz = fields[29],
                            F4630kHz = fields[30]
                        };
                        Radiostation_Prev.Add(logItem);
                    }
                    for (int renumber = 0; renumber < Radiostation_Prev.Count; renumber++)
                    {
                        Radiostation_Prev[Radiostation_Prev.Count - renumber - 1].No = renumber + 1;
                    }
                }
                string m = Radiostation_Prev.Count() + " Read from file.";
            }
            else
            {
                MessageBox.Show("File not found in Read_Prev_Data:" + file_radio_station);
            }
            return Radiostation_Prev;
        }
        public void Write_Radios_File(List<CallBookViewModel> radios)
        {
            string file_radio_station = GetFilenameFrom("soumu_radio_station");
            message += "written to " + file_radio_station + "\n";
            using (StreamWriter sw = new StreamWriter(file_radio_station, false))    //append false
            {
                foreach (CallBookViewModel radio in radios)
                {
                    string[] aStrings =
                        {
                        radio.Call_sign ,
                        radio.Clubname ,
                        radio.License_expire ,
                        radio.Station_type ,
                        radio.Station_address,
                        radio.Code_ken ,
                        radio.Code_city ,
                        radio.Code_ku ,
                        radio.Checked_date ,
                        radio.F136kHz ,
                        radio.F1910kHz,
                        radio.F3537kHz ,
                        radio.F3798kHz ,
                        radio.F7100kHz ,
                        radio.F10125kHz ,
                        radio.F14175kHz ,
                        radio.F18118kHz ,
                        radio.F21225kHz ,
                        radio.F24940kHz ,
                        radio.F28850kHz ,
                        radio.F52MHz ,
                        radio.F145MHz ,
                        radio.F435MHz ,
                        radio.F1280MHz ,
                        radio.F2425MHz ,
                        radio.F5750MHz ,
                        radio.F10125MHz ,
                        radio.F10475MHz ,
                        radio.F24025MHz,
                        radio.F47100MHz ,
                        radio.F4630kHz
                    };
                    string strSeparator = ",";
                    string strLine = string.Join(strSeparator, aStrings);
                    sw.WriteLine(strLine);
                }
                sw.Close();
            }
        }
        public Dictionary<string, string> ReadFileDefinition()
        {
            Dictionary<string, string> openFileName = new Dictionary<string, string>();
            string st = string.Empty;

            if (File.Exists(filename_filedefinition))
            {
                st = "File Exists: " + filename_filedefinition;
            }
            else
            {
                st = "File not found: " + filename_filedefinition;
            }

            using (StreamReader sr = new StreamReader(filename_filedefinition))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    openFileName.Add(fields[0], fields[1]);
                }

                foreach (KeyValuePair<string, string> kvp in openFileName)
                {
                    st = String.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                }
            }
            return openFileName;
        }
        public string GetFilenameFrom(string fileVariable)
        {
            string fileName = string.Empty;
            string s = string.Empty;
            if (gOpenFileName.TryGetValue(fileVariable, out fileName))
            {
                fileName = filename_folder + @"\" + fileName;
                s = "For key =" + fileName;
            }
            else
            {
                s = "Key = " + fileVariable + " is not found.";
            }
            return fileName;
        }
        public List<CallBookViewModel> SetJccJcgCode()
        {
            List<CallBookViewModel> newRadioStation = new List<CallBookViewModel>();
            List<CallBookViewModel> oldRadioStation = gCallBookViewModel; //Read_Radios_File();

            foreach (CallBookViewModel radio1 in oldRadioStation)
            {
                string address1 = radio1.Station_address;
                string address2 = string.Empty;
                string address3 = string.Empty;
                string radio_code_ken = radio1.Code_ken;
                string radio_code_city = radio1.Code_city;
                string radio_code_ku = radio1.Code_ku;
                string radio_ken = string.Empty;
                string radio_city = string.Empty;
                string radio_ku = string.Empty;
                string radio_add = string.Empty;
                Match match1, match2;
                //---------------------------------------------------------------------------address1
                if (address1.Contains("府中"))   //東京都府中市  京都府　広島県府中市
                {
                    string address1CUT = address1.Replace("府中", "");
                    match1 = Regex.Match(address1CUT, "^.{2,3}(県|都|道|府)");
                }
                else
                {
                    match1 = Regex.Match(address1, "^.{2,3}(県|都|道|府)");
                }
                radio_ken = match1.Value;
                address2 = address1.Replace(match1.Value, "");
                //---------------------------------------------------------------------------address2

                if (address2.Contains("市") && address2.Contains("区"))   //愛知県名古屋市千種区
                {
                    string address2CUT = address2.Replace("区", "");
                    match2 = Regex.Match(address2CUT, "^.{1,6}(市|郡|区)");
                }
                else
                {
                    match2 = Regex.Match(address2, "^.{1,6}(市|郡|区|島)");
                }
                radio_city = match2.Value;
                address3 = address2.Replace(match2.Value, "");
                radio_add = radio_ken + " " + radio_city;
                //---------------------------------------------------------------------------address3

                if (address3.Length > 1) //まだデータがあるなら
                {
                    radio_ku = address3;
                    radio_add = radio_ken + " " + radio_city + " " + radio_ku;                    //Debug.WriteLine(radio_add);
                }        

                List<ItemJccJcg> JccJcgKen = gItemJccJcg.FindAll(x => radio_ken.StartsWith(x.ken_name));
                List<ItemJccJcg> JccJcgCity = JccJcgKen.FindAll(x => radio_city.StartsWith(x.citygun_name));
                if (JccJcgCity.Count == 1)
                {
                    radio1.Code_ken = JccJcgCity[0].ken_no;
                    radio1.Code_city = JccJcgCity[0].citygun_no;
                    radio1.Code_ku = JccJcgCity[0].kutown_no;
                }
                List<ItemJccJcg> JccJcgTown = JccJcgCity.FindAll(x => radio_ku.StartsWith(x.kutown_name));
                if (JccJcgTown.Count == 1)
                {
                    radio1.Code_ken = JccJcgTown[0].ken_no;
                    radio1.Code_city = JccJcgTown[0].citygun_no;
                    radio1.Code_ku = JccJcgTown[0].kutown_no;
                }

                foreach (ItemJccJcg code in JccJcgKen)
                {
                    string code_ken = code.ken_name;
                    string code_city = code.citygun_name;
                    string code_town = code.kutown_name;

                    if (radio_ken.StartsWith(code_ken)) radio1.Code_ken = code.ken_no;
                    if (radio_city.StartsWith(code_city)) radio1.Code_city = code.citygun_no;
                    if (radio_ku.StartsWith(code_town)) radio1.Code_ku = code.kutown_no;
                }
                newRadioStation.Add(radio1);
            }
            gItemJccJcg.Sort((a, b) => a.ken_no.CompareTo(b.ken_no));
            return newRadioStation;
        }
        public int selectedIndex { get; set; }
        public bool IsEnabledStart { get; private set; }
        public ObservableCollection<CallBookViewModel>? Items { get; set; }
        public string fetchNumber { get; set; }
        public string oneCallSign { get; set; }
        public string callsignFile { get; set; }
        public string message { get; set; }
        public Command? WindowLoaded { get; private set; }
        public Command? cmdStart { get; private set; }
        public Command? cmdSetCode { get; private set; }
        public Command? cmdSelectFile { get; private set; }
        public Command? cmdOpenFolder { get; private set; }
    }
}
