using System;
using System.Collections.Generic;
using System.Text;

namespace shvCallBook.Core.Models
{
    public sealed class CallBookItem
    {        
        public CallBookItem(int no, string call_sign, string clubname, string license_expire, string station_type, string station_address, string code_ken, string code_city, string code_ku, string checked_date, string f136kHz, string f1910kHz, string f3537kHz, string f3798kHz, string f7100kHz, string f10125kHz, string f14175kHz, string f18118kHz, string f21225kHz, string f24940kHz, string f28850kHz, string f52MHz, string f145MHz, string f435MHz, string f1280MHz, string f2425MHz, string f5750MHz, string f10125MHz, string f10475MHz, string f24025MHz, string f47100MHz, string f4630kHz)
        {
            this.No = no;
            this.Call_sign = call_sign;
            this.Clubname = clubname;
            this.License_expire = license_expire;
            this.Station_type = station_type;
            this.Station_address = station_address;
            this.Code_ken = code_ken;
            this.Code_city = code_city;
            this.Code_ku = code_ku;
            this.Checked_date = checked_date;
            this.F136kHz = f136kHz;
            this.F1910kHz = f1910kHz;
            this.F3537kHz = f3537kHz;
            this.F3798kHz = f3798kHz;
            this.F7100kHz = f7100kHz;
            this.F10125kHz = f10125kHz;
            this.F14175kHz = f14175kHz;
            this.F18118kHz = f18118kHz;
            this.F21225kHz = f21225kHz;
            this.F24940kHz = f24940kHz;
            this.F28850kHz = f28850kHz;
            this.F52MHz = f52MHz;
            this.F145MHz = f145MHz;
            this.F435MHz = f435MHz;
            this.F1280MHz = f1280MHz;
            this.F2425MHz = f2425MHz;
            this.F5750MHz = f5750MHz;
            this.F10125MHz = f10125MHz;
            this.F10475MHz = f10475MHz;
            this.F24025MHz = f24025MHz;
            this.F47100MHz = f47100MHz;
            this.F4630kHz = f4630kHz;
        }        

        public readonly int No;
        public readonly string Call_sign;
        public readonly string Clubname;
        public readonly string License_expire;
        public readonly string Station_type; 
        public readonly string Station_address;
        public readonly string Code_ken; 
        public readonly string Code_city; 
        public readonly string Code_ku; 
        public readonly string Checked_date; 
        public readonly string F136kHz;
        public readonly string F1910kHz;
        public readonly string F3537kHz;
        public readonly string F3798kHz;
        public readonly string F7100kHz;
        public readonly string F10125kHz;
        public readonly string F14175kHz;
        public readonly string F18118kHz;
        public readonly string F21225kHz;
        public readonly string F24940kHz;
        public readonly string F28850kHz;
        public readonly string F52MHz;
        public readonly string F145MHz;
        public readonly string F435MHz;
        public readonly string F1280MHz;
        public readonly string F2425MHz;
        public readonly string F5750MHz;
        public readonly string F10125MHz;
        public readonly string F10475MHz;
        public readonly string F24025MHz;
        public readonly string F47100MHz;
        public readonly string F4630kHz;        
    }
}