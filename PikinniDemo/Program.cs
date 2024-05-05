using Pikinni;

namespace PikinniDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pikinni Demo\n");

            Ini ini1 = Ini.LoadFromFile("Test1.ini");

            Console.WriteLine("---");
            Console.WriteLine(ini1.Global["escape-codes"]);
            Console.WriteLine("---");
            Console.WriteLine(ini1.ToString());

            Ini ini2 = new Ini();

            ini2.Global["newGlobalProperty"] = "value1";
            ini2["NewSection"]["newProperty"] = "value2";
            Console.WriteLine(ini2.ToString());
            ini2.SaveToFile("Ini2.ini");
        }
    }
}
