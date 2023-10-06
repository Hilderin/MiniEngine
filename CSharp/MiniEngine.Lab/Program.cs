using System.Diagnostics;

namespace MiniEngine.Lab
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();


                var lab = new Lab_StructMutableImmuatable();

                lab.Test();


                sw.Stop();
                Console.WriteLine("Done: " + sw.ElapsedMilliseconds.ToString("0") + "ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}