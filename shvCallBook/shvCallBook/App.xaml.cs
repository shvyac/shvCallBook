////////////////////////////////////////////////////////////////////////////
//
// Epoxy template source code.
// Write your own copyright and note.
// (You can use https://github.com/rubicon-oss/LicenseHeaderManager)
//
////////////////////////////////////////////////////////////////////////////

using shvCallBook.Views;
using System.Windows;

namespace shvCallBook
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CallBookWindow window = new CallBookWindow();
            window.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            //MessageBox.Show("Thanks, Bye");            
        }
    }
}
