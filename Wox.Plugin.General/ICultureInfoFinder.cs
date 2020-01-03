using System.Globalization;

namespace Wox.Plugin.General
{
    public interface ICultureInfoFinder
    {
        CultureInfo GetCurrentCultureInfo();
    }

    public class CultureInfoFinder : ICultureInfoFinder
    {
        public CultureInfo GetCurrentCultureInfo()
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            CultureInfo info = null;
            var res = dispatcher.Invoke(() => 
            info = System.Windows.Input.InputLanguageManager.Current.CurrentInputLanguage);
            return info;
        }
    }
}