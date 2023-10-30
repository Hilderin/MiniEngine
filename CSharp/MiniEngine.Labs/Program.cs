using System.Diagnostics;

namespace MiniEngine.Labs
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();


                var lab = new Lab_SpirvParser();

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