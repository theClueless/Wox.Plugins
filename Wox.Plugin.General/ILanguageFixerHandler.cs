using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Wox.Infrastructure.Storage;

namespace Wox.Plugin.General
{
    public interface ILanguageFixerHandler
    {
        Result TryFix(Query query, IPublicAPI publicApi);
    }


    public class LanguageFixerHandler : ILanguageFixerHandler
    {
        private readonly IClipboardHelper _clipboardHelper;
        private readonly ICultureInfoFinder _cultureInfoFinder;
        private readonly Dictionary<string, LanguageDefinition> _languageDefinitions = new Dictionary<string, LanguageDefinition>();

        public LanguageFixerHandler(ICultureInfoFinder cultureInfoFinder = null, IClipboardHelper clipboardHelper = null)
        {
            _clipboardHelper = clipboardHelper ?? new ClipboardHelper();
            _cultureInfoFinder = cultureInfoFinder ?? new CultureInfoFinder();
            InitLanguages();
        }

        private void InitLanguages()
        {
            var heb = new LanguageDefinition
            {
                CultureName = "he-il",
                Converter = new Dictionary<char, char>
                {
                    { '/','q'},
                    { '\'','w'},
                    { 'ק','e'},
                    { 'ר','r'},
                    { 'א','t'},
                    { 'ט','y'},
                    { 'ו','u'},
                    { 'ן','i'},
                    { 'ם','o'},
                    { 'פ','p'},
                    { 'ש','a'},
                    { 'ד','s'},
                    { 'ג','d'},
                    { 'כ','f'},
                    { 'ע','g'},
                    { 'י','h'},
                    { 'ח','j'},
                    { 'ל','k'},
                    { 'ך','l'},
                    { 'ף',';'},
                    { 'ז','z'},
                    { 'ס','x'},
                    { 'ב','c'},
                    { 'ה','v'},
                    { 'נ','b'},
                    { 'מ','n'},
                    { 'צ','m'},
                    { 'ת',','},
                    { 'ץ','.'},
                    { '.','/'},
                    { ';','`'},
                }
            };

            _languageDefinitions.Add(heb.CultureName, heb);
        }

        public Result TryFix(Query query, IPublicAPI publicApi)
        {
            // check current language
            var current = _cultureInfoFinder.GetCurrentCultureInfo();
            
            // if one of installed languages
            if (this._languageDefinitions.TryGetValue(current.Name.ToLower(), out LanguageDefinition def))
            {
                var converted = ConvertString(query.RawQuery, def);
                if (converted == query.RawQuery)
                { // nothing was converted
                    return null;
                }

                // convert string and add result that change the input.
                return new Result
                {
                    Score = 100,
                    Title = $"Convert to {current.DisplayName}",
                    SubTitle = $"Convert to: {converted}",
                    ContextData = converted,
                    Action = context =>
                    {
                        _clipboardHelper.SetClipboardText(converted);
                        publicApi.ChangeQuery(converted);
                        return false;
                    }
                };
            }

            return null;
        }

        private string ConvertString(string toConvert, LanguageDefinition definition)
        {
            var builder = new StringBuilder(toConvert.Length);
            foreach (var t in toConvert)
            {
                builder.Append(definition.Converter.TryGetValue(t, out var res) ? res : t);
            }

            return builder.ToString();
        }

    }

    public class LanguageDefinition
    {
        public string CultureName { get; set; }

        public Dictionary<char,char> Converter { get; set; }
    }
}