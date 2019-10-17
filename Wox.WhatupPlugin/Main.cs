using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using Wox.Plugin;

namespace Wox.WhatupPlugin
{
    public class Main : IPlugin
    {
        public const string PluginActionKeyworkd = "wh";

        private readonly IClipboardHelper _clipboardHelper;

        // @"https://web.whatsapp.com/send?phone=972544325740";
        private readonly string webApistring = @"https://web.whatsapp.com/send?phone=972";

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
            if (queryString == string.Empty)
            { // try clipboard
                var clipboard = _clipboardHelper.GetClipboardText();
                queryString = clipboard;
            }

            var phoneNumber = TryExtractPhoneNumber(queryString);
            if (phoneNumber != null)
            {
                list.Add(
                    new Result
                    {
                        Score = 100,
                        IcoPath = "Images\\whatup.png",
                        Title = $"{phoneNumber}",
                        SubTitle = $"Send message to {phoneNumber}",
                        Action = context =>
                        {
                            Process.Start(webApistring + queryString.Substring(1));
                            return true;
                        },
                    });
            }

            return list;
        }

        public static string TryExtractPhoneNumber(string phoneString)
        {
            if (string.IsNullOrWhiteSpace(phoneString))
            {
                return null;
            }

            foreach (var possibleNumber in GetNumbers(phoneString))
            {
                if (possibleNumber.Length == 12 && possibleNumber.StartsWith("972", StringComparison.OrdinalIgnoreCase))
                {
                    return "0" + possibleNumber.Substring(3);
                }

                if (possibleNumber.Length == 10 && possibleNumber.StartsWith("05"))
                {
                    return possibleNumber;
                }
            }

            return null;
        }

        public static List<string> GetNumbers(string query)
        {
            var result = new List<string>();
            var builder = new StringBuilder();

            foreach (var ch in query)
            {
                if (char.IsDigit(ch))
                {
                    builder.Append(ch);
                }
                else if (ch != '-' && ch != '+')
                {
                    AddFromBuilder();
                }
            }

            AddFromBuilder();

            return result;

            void AddFromBuilder()
            {
                if (builder.Length <= 0) return;
                var res = builder.ToString();
                if (!string.IsNullOrWhiteSpace(res))
                {
                    result.Add(res);
                }

                builder.Clear();
            }
        }

        public void Init(PluginInitContext context)
        {
        }
    }


}
