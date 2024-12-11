using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace memCatalog
{
    //Клас объекта Мема
    public class Mem
    {
        [JsonInclude]
        private string Name;
        [JsonInclude]
        private string Category;
        [JsonInclude]
        private string Path;

        [JsonConstructor]
        public Mem(string name, string category, string path)
        {
            Name = name;
            Category = category;
            Path = path;
        }
        public string GetName()
        { 
            return Name; 
        }
        public string GetCategoty()
        {
            return Category; 
        }
        public string GetPath()
        {
            return Path;
        } 
    }
}
