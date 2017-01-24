using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDebugLib
{
    public class TagsCollection
    {
        public string[] tagsArray;
        private static char[] separator = { '#', ' ' };
        public TagsCollection(string tags)
        {
            tagsArray = tags.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
