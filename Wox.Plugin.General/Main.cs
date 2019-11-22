using System;
using System.Collections.Generic;

namespace Wox.Plugin.General
{
    public class Main : IPlugin
    {
        public const string CountActionKeyword = "word";

        private readonly IClipboardHelper _clipboardHelper;

        public Main()
        {
            if (_clipboardHelper == null)
            {
                _clipboardHelper = new ClipboardHelper();
            }
        }

        public Main(IClipboardHelper clipboardHelper) : this()
        {
            _clipboardHelper = clipboardHelper;
        }

        public List<Result> Query(Query query)
        {
            var list = new List<Result>();

            var queryString = query.Search.Trim();
            
            if (query.ActionKeyword == CountActionKeyword && queryString == string.Empty)
            { // try clipboard
                var clipboard = _clipboardHelper.GetClipboardText();
                if (!string.IsNullOrEmpty(clipboard))
                {
                    var cut = clipboard.Substring(0, clipboard.Length > 50 ? 50 : clipboard.Length).Replace(Environment.NewLine,"#L");
                    list.Add(
                        new Result
                        {
                            Score = 99,
                            IcoPath = "Images\\count.png",
                            Title = $"Char Count: {clipboard.Length}",
                            SubTitle = cut
                        });
                }
            }

            return list;
        }
        
        public void Init(PluginInitContext context)
        {
        }
    }


}
