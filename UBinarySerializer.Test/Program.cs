using System;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace NullSoftware.Serialization.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputPath = Path.GetFullPath(@".\output.bin");
            string outputUnsafePath = Path.GetFullPath(@".\output-unsafe.bin");

            BinarySerializer<Player> serializer = RunAction(() => new BinarySerializer<Player>(), "Initialization");

            // test data
            Player p = new Player();
            p.Health = 23;
            p.Hunger = 234;
            p.Position = new Vector3(253.4f, -3123.453f, 93.003f);
            p.GameMode = GameMode.Spectator;
            p.Skin = new Texture(@".\data\skin\default.png");
            p.Items.Add(new Item(6345));
            p.Items.Add(new Item(7734, 64));

            using (FileStream fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                RunAction(() => serializer.Serialize(fs, p), "Serialize");
            }

            using (FileStream fs = new FileStream(outputUnsafePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                RunAction(() => serializer.SerializeUnsafe(fs, p), "Serialize (Unsafe)");
            }

            bool isDesDataEqual, isUnsafeDesDataEqual;

            using (FileStream fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))
            {
                Player pDeserialized = RunAction(() => serializer.Deserialize(fs), "Deserialize");

                isDesDataEqual = p.Equals(pDeserialized);
            }

            using (FileStream fs = new FileStream(outputUnsafePath, FileMode.Open, FileAccess.Read))
            {
                Player pDeserialized = RunAction(() => serializer.DeserializeUnsafe(fs), "Deserialize (Unsafe)");

                isUnsafeDesDataEqual = p.Equals(pDeserialized);               
            }

            Console.WriteLine();

            Console.WriteLine($"Is Deserialized Data Equals Original: {isDesDataEqual}");
            Console.WriteLine($"Is Unsafe Deserialized Data Equals Original: {isUnsafeDesDataEqual}");

            Console.WriteLine();
            Console.WriteLine("Serialized Data:");
            Console.WriteLine(string.Join(" ", File.ReadAllBytes(outputPath).Select(b => b.ToString("X2"))));

            Console.WriteLine();
            Console.WriteLine("Serialized Data (Unsafe):");
            Console.WriteLine(string.Join(" ", File.ReadAllBytes(outputUnsafePath).Select(b => b.ToString("X2"))));

            Console.WriteLine();

            Console.ReadKey(true);
        }

        static void RunAction(Action action, string actionName)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            action();

            stopwatch.Stop();

            Console.WriteLine($"'{actionName}' took {stopwatch.Elapsed}");
        }

        static TOutput RunAction<TOutput>(Func<TOutput> action, string actionName)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            TOutput result = action();

            stopwatch.Stop();

            Console.WriteLine($"'{actionName}' took {stopwatch.Elapsed}");

            return result;
        }
    }
}
