using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wox.Plugin.OneNote
{
    public class Main : IPlugin
    {
        private string ImagePath = "Images\\onenote.png";

        // next steps template project and add references.

        // verify image is working as expected

        // use interop api to query elements
        // https://docs.microsoft.com/en-us/office/client-developer/onenote/window-interfaces-onenote
        // https://stackoverflow.com/questions/27294510/how-to-write-to-a-onenote-2013-page-using-c-sharp-and-the-onenote-interop

        // run url to open pages - 
        // onenote:///C:\Users\atepper\Documents\OneNote%20Notebooks\General\Work%20general.one#Today
        // onenote:///C:\Users\atepper\Documents\OneNote%20Notebooks\General\Work%20general.one#Today&section-id={224F2718-4DBF-4E19-9A5F-6FA90E5E2C50}&page-id={D8B3A806-674D-44CE-8A3E-24BF3F048B8C}&end

        public List<Result> Query(Query query)
        {
            return new List<Result>{
            new Result
            {
                Score = 100,
                Title = "Testing",
                IcoPath = ImagePath,
                
            }};
        }

        public void Init(PluginInitContext context)
        {
        }
    }
}
