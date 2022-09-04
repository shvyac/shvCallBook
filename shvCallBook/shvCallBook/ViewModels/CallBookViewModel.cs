////////////////////////////////////////////////////////////////////////////
//
// Epoxy template source code.
// Write your own copyright and note.
// (You can use https://github.com/rubicon-oss/LicenseHeaderManager)
//
////////////////////////////////////////////////////////////////////////////

using Epoxy;

namespace shvCallBook.ViewModels
{
    [ViewModel]
    public sealed class CallBookViewModel
    {
        public  int No { get; set; }
        public  string? Call_sign { get; set; }
        public  string? Clubname { get; set; }
        public  string? License_expire { get; set; }
        public  string? Station_type { get; set; }
        public  string? Station_address { get; set; }
        public  string? Code_ken { get; set; }
        public  string? Code_city { get; set; }
        public  string? Code_ku { get; set; }
        public  string? Checked_date { get; set; }
        public  string? F136kHz { get; set; }
        public  string? F1910kHz { get; set; }
        public  string? F3537kHz { get; set; }
        public  string? F3798kHz { get; set; }
        public  string? F7100kHz { get; set; }
        public  string? F10125kHz { get; set; }
        public  string? F14175kHz { get; set; }
        public  string? F18118kHz { get; set; }
        public  string? F21225kHz { get; set; }
        public  string? F24940kHz { get; set; }
        public  string? F28850kHz { get; set; }
        public  string? F52MHz { get; set; }
        public  string? F145MHz { get; set; }
        public  string? F435MHz { get; set; }
        public  string? F1280MHz { get; set; }
        public  string? F2425MHz { get; set; }
        public  string? F5750MHz { get; set; }
        public  string? F10125MHz { get; set; }
        public  string? F10475MHz { get; set; }
        public  string? F24025MHz { get; set; }
        public  string? F47100MHz { get; set; }
        public  string? F4630kHz { get; set; }
    }
    public sealed class ItemJccJcg
    {
        public string? ken_no { get; set; }                  //都道府県No
        public string? ken_name { get; set; }                //都道府県名
        public string? citygun_no { get; set; }              //市郡No        
        public string? citygun_name_roman { get; set; }      //市郡名roman
        public string? citygun_name { get; set; }            //市郡名
        public string? kutown_no { get; set; }               //区町No
        public string? kutown_name_roman { get; set; }       //区町名roman
        public string? kutown_name { get; set; }             //区町名      
        public string? date_expired { get; set; }            //消滅日
    }
}
