using System;

namespace HypnoLogLib
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
