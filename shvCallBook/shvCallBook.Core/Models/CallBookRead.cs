using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace shvCallBook.Core.Models
{
    public static class CallBookRead
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string file_radio_station;
        private static string file_not_found;

        private static List<CallBookItem> _radiostation_serial;
        private static List<string> _radiostation_not_found;

        public static async ValueTask<List<CallBookItem>> FetchNewCallsignAsync(string callsign)
        {
            string uricall = @"https://www.tele.soumu.go.jp/musen/SearchServlet?SC=1&pageID=3&SelectID=1&CONFIRM=0&SelectOW=01" + @"&IT=&HC=&HV=&FF=&TF=&HZ=3&NA=&DFY=&DFM=&DFD=&DTY=&DTM=&DTD=&SK=2&DC=100&MA=" + callsign;

            System.Uri url = new Uri(uricall);
            List<CallBookItem> items;

            using (var response =
                await httpClient.
                    GetAsync(uricall).
                    ConfigureAwait(false))
            {
                using (var stream =
                    await response.Content.ReadAsStreamAsync().
                        ConfigureAwait(false))
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var tr = new StreamReader(stream, Encoding.GetEncoding("Shift_JIS"), true);

                    string body = await tr.ReadToEndAsync();

                    items = CompleteDownloadProcSerial(body, callsign);
                }
            }
            return items;
        }

        public static List<CallBookItem> CompleteDownloadProcSerial(string result, string call)
        {
            List<CallBookItem> callInfos = new List<CallBookItem>();
            file_not_found = @"file_callbook_notfound.csv";

            MatchCollection zeromatch = Regex.Matches(result, "検索結果が0件です。");

            if (0 < zeromatch.Count)    // Result ZERO
            {
                string m = "zeromatch.Count = " + zeromatch.Count + " " + call;
                DateTime dt_now = DateTime.Now;
                string[] aStrings = { call, dt_now.ToString("yyyy-MM-dd") };
                string strSeparator = ",";
                string strLine = string.Join(strSeparator, aStrings);

                using (StreamWriter sw = new StreamWriter(file_not_found, true))
                {
                    sw.WriteLine(strLine);
                    sw.Close();
                }

                return null;
            }
            else    // Call Info Found
            {

                MatchCollection matches = Regex.Matches(result, ".*[都道府県].*<br>");

                foreach (Match m in matches)
                {
                    string address = m.Value.Replace("<br>", "").Trim();
                }

                string HRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";
                MatchCollection mat2 = Regex.Matches(result, HRefPattern);

                foreach (Match m in mat2)
                {
                    if (m.Value.Contains("pageID=4"))
                    {
                        //-------------------------------------------------------------------------------------parse subpage 
                        string href = m.Value;

                        Debug.WriteLine("CompleteDownloadProcSerial: address = " + href + " " + call);

                        string url = href.Remove(0, 7);
                        string url2 = "https://www.tele.soumu.go.jp/musen" + url.Remove(url.Length - 1, 1);
                        HttpClient client = new HttpClient();
                        Stream htmlStream = client.GetStreamAsync(url2).Result;
                        TextReader reader = new StreamReader(htmlStream, Encoding.GetEncoding("Shift_JIS"), true);
                        string source = reader.ReadToEndAsync().Result;
                        //----
                        string[] deli = { "\n" };
                        string[] lines = source.Split(deli, StringSplitOptions.None);

                        //------------------------------------------------------------------------------識別信号
                        int index0 = Array.FindIndex(lines, ContainsCallsign);
                        string callsign = lines[index0 + 4].Replace("<BR>", "").Trim();
                        //------------------------------------------------------------------------------氏名又は名称 = ＊＊＊＊＊
                        int index1 = Array.FindIndex(lines, ContainsKeyword);
                        string op_name = lines[index1 + 4].Trim();
                        if (op_name.Contains("＊")) op_name = string.Empty;
                        //------------------------------------------------------------------------------免許の有効期間 = 令5.4.1まで
                        int index4 = Array.FindIndex(lines, s => s.Contains("免許の有効期間"));
                        string license_expire = lines[index4 + 4].Trim() + lines[index4 + 3].Trim();
                        string license_expire_seireki = ConvertToSeireki(license_expire);
                        //------------------------------------------------------------------------------移動範囲
                        string type_station = "";
                        if (source.Contains("陸上、海上及び上空"))
                        {
                            type_station = "陸上、海上及び上空";
                        }
                        else if (source.Contains("移動しない"))
                        {
                            type_station = "移動しない";
                        }
                        //------------------------------------------------------------------------------無線設備の設置場所／常置場所
                        string sta_place = "";
                        string address_code = "";
                        int index2 = Array.FindIndex(lines, ContainsPlace);
                        for (int i = 0; i < 40; i++)
                        {
                            if (lines[index2 + i].Contains("<br>"))
                            {
                                sta_place = lines[index2 + i].Trim().Replace("<br>", "");
                            }
                        }
                        //-------------------------------------14175 MHz   電波の型式、周波数及び空中線電力
                        int index3 = Array.FindIndex(lines, Contains14175);
                        string power = "";
                        if (0 < index3)
                        {
                            power = lines[index3 + 22].Replace("&nbsp;", "").Replace(@"<td><pre class=""defpre"">", "").Replace(@"</pre></td>", "").Trim();
                        }
                        else
                        {
                            //bool14mhz = "NO";
                        }
                        //---------------------------------------------------電波の型式、周波数及び空中線電力
                        string[] arr = Array.FindAll(lines, s => s.Contains("defpre")).Select(s => s.Trim()).ToArray();
                        string[] strTypeFreqPower = arr.Select(s => s.Replace("&nbsp;", "")).ToArray()
                            .Select(s => s.Replace("<td><pre class=\"defpre\">", "")).ToArray().Select(s => s.Replace("</pre></td>", "")).ToArray();
                        string[] freqpower = new string[strTypeFreqPower.Length / 3];
                        string[] freq = { "136.75kHz", "1910kHz", "3537.5kHz", "3798kHz", "7100kHz", "10125kHz", "14175kHz", "18118kHz", "21225kHz", "24940kHz", "28.85MHz", "52MHz", "145MHz", "435MHz", "1280MHz", "2425MHz", "5750MHz", "10.125GHz", "10.475GHz", "24.025GHz", "47.1GHz", "4630kHz" };
                        string[] powers = new string[freq.Length];
                        bool isMatchFreq = false;

                        for (int i = 0; i < strTypeFreqPower.Length / 3; i++)
                        {
                            string s = strTypeFreqPower[i * 3] + "\t" + strTypeFreqPower[i * 3 + 1] + "\t" + strTypeFreqPower[i * 3 + 2];
                            //Debug.WriteLine(s);
                            freqpower[i] = s;
                            for (int j = 0; j < freq.Length; j++)
                            {
                                if (s.Contains(freq[j]))
                                {
                                    powers[j] = strTypeFreqPower[i * 3 + 2];
                                    isMatchFreq = true;
                                }
                            }
                            if (isMatchFreq)
                            {
                                isMatchFreq = false;
                            }
                            else
                            {
                                Debug.WriteLine("ERROR freq NOT found ---> " + s);
                            }
                        }
                        //-------------------------------------------------------------------------------item data Add
                        DateTime dt_now = DateTime.Now;
                        string date_today = dt_now.ToString("yyyy-MM-dd");

                        CallBookItem newItem = new CallBookItem(
                            0, callsign, op_name, license_expire_seireki, type_station, sta_place, address_code, address_code, address_code, date_today, powers[0], powers[1], powers[2], powers[3], powers[4], powers[5], powers[6], powers[7], powers[8], powers[9], powers[10], powers[11], powers[12], powers[13], powers[14], powers[15], powers[16], powers[17], powers[18], powers[19], powers[20], powers[21]
                        );
                        callInfos.Add(newItem);
                    }
                }
            }
            return callInfos;
        }

        public static string ConvertToSeireki(string strWareki0)
        {
            // 令5.4.1まで
            string strWareki = strWareki0.Replace("まで", "").Replace(".", "/").Replace("令", "令和");
            //string era;   // 元号の文字列
            //int eraYear;  // 和暦の年

            CultureInfo culture = new CultureInfo("ja-JP", true);
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();

            string warekiDate = strWareki;  //era + eraYear.ToString() + "/12/31";
            DateTime date = DateTime.Parse(warekiDate, culture.DateTimeFormat);

            int year = date.Year;   // 西暦の年
            return date.ToShortDateString();
        }

        private static bool ContainsCallsign(String s)
        {
            if (s.Contains("識別信号")) return true;
            else return false;
        }

        private static bool ContainsKeyword(String s)
        {
            if (s.Contains("氏名又は名称")) return true;
            else return false;
        }

        private static bool ContainsPlace(String s)
        {
            if (s.Contains("無線設備の設置場所／常置場所") || s.Contains("無線設備の設置／常置場所")) return true;
            else return false;
        }

        private static bool Contains14175(String s)
        {
            if (s.Contains("14175")) return true;
            else return false;
        }

        public static async Task Read_SoumuGoJpSerial(List<string> requested_call_signs, int parseNumber)
        {
            file_radio_station = @"file_callbook.csv";
            file_not_found = @"file_callbook_notfound.csv";
            int loop_counter = 0;
            int loop_end = parseNumber;
            int NthCall = -1;
            var sWatch = new System.Diagnostics.Stopwatch();
            var sWatch_total = new System.Diagnostics.Stopwatch();
            sWatch.Start();
            sWatch_total.Start();

            foreach (string call1 in requested_call_signs)
            {
                NthCall++;
                //----------------------------------------------------------------------------Delete Portable Info
                string call;
                if (call1.Contains("/"))
                {
                    call = call1.Substring(0, call1.IndexOf("/"));
                    string m = "removed / " + call1 + " -> " + call;
                    Debug.WriteLine(m);
                }
                else
                {
                    call = call1;
                }
                //-------------------------------------------------------------------------------------Already ?
                int indexcall = _radiostation_serial.FindIndex(m => m.Call_sign == call);
                if (-1 < indexcall)
                {
                    string m = "already " + call + " index(" + indexcall + ")";
                    continue;
                }

                int indexcall2 = _radiostation_not_found.FindIndex(m => m == call);
                if (-1 < indexcall2)
                {
                    string m = "not found " + call + " index(" + indexcall2 + ")";
                    continue;
                }

                string uricall = @"https://www.tele.soumu.go.jp/musen/SearchServlet?SC=1&pageID=3&SelectID=1&CONFIRM=0&SelectOW=01&IT=&HC=&HV=&FF=&TF=&HZ=3&NA=&DFY=&DFM=&DFD=&DTY=&DTM=&DTD=&SK=2&DC=100&MA=" + call;

                System.Uri url = new Uri(uricall);
                Task<string> taskA = Task.Run(() => ReadFromUrlAsync2(url));
                TimeSpan ts_between = sWatch.Elapsed;
                sWatch.Restart();
                await taskA.ContinueWith(work => CompleteDownloadProcSerial(work.Result, call));
                TimeSpan ts_download = sWatch.Elapsed;
                sWatch.Restart();
                await Task.Delay(3000);
                DateTime dt = DateTime.Now;
                Debug.WriteLine(call + " Ended-----------------------------------   " + dt + " --- "
                    + (loop_counter + 1).ToString() + " / " + loop_end.ToString() + "\n");
                loop_counter++;
                if (loop_end <= loop_counter) break;
            }
            sWatch.Stop();
        }

        public static async Task<string> ReadFromUrlAsync2(Uri url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (Stream htmlStream = await client.GetStreamAsync(url))
                {
                    string htmlString;
                    TextReader reader = new StreamReader(htmlStream, Encoding.GetEncoding("Shift_JIS"), true);
                    htmlString = await reader.ReadToEndAsync();
                    return htmlString;
                }
            }
        }
    }
}
