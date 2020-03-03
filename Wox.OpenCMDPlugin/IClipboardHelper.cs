using System;
using System.Threading;
using System.Windows;

namespace Wox.Plugin.OpenCMD
{
    public interface IClipboardHelper
    {
        string GetClipboardText();

        void SetClipboardTest(string text);
    }

    public class ClipboardHelper : IClipboardHelper
    {
        public string GetClipboardText()
        {
            Exception threadEx = null;
            var text = "";
            var staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        text = Clipboard.GetText();
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            if (threadEx != null)
            {
                throw threadEx;
            }

            return text;
        }

        public void SetClipboardTest(string text)
        {
            Exception threadEx = null;
            var staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        Clipboard.SetText(text);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            if (threadEx != null)
            {
                throw threadEx;
            }
        }
    }
}