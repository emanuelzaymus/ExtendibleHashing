using System;
using System.Collections;
using System.IO;
using System.Text;
using ExtendibleHashing.Extensions;

namespace ExtendibleHashing.ConsoleApp
{
    class Program
    {
        private const string FilePath = "myFile.bin";

        static void Main(string[] args)
        {

            //BitArray ba = new BitArray(BitConverter.GetBytes(5)); // ...0101
            //Console.WriteLine(ba.Length);
            //Console.WriteLine();
            //foreach (var item in ba)
            //{
            //    Console.WriteLine((int)item);
            //}
            //return;


            //using (var fileStream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            //{
            //    //byte[] bytesIn = Encoding.ASCII.GetBytes("sdfsdfs");
            //    byte[] bytesIn = Encoding.ASCII.GetBytes("BBB");
            //    //byte[] bytesIn = new byte[] { 65, BitConverter.GetBytes('B')[0], 67, 68, BitConverter.GetBytes('E')[0] };

            //    //int offset = (int)fileStream.Length;
            //    //fileStream.SetLength(fileStream.Length + bytesIn.Length);
            //    //fileStream.Write(bytesIn, offset - 1, bytesIn.Length);

            //    fileStream.Seek(0, SeekOrigin.End);
            //    //fileStream.SetLength(bytesIn.Length);
            //    fileStream.Write(bytesIn);
            //    fileStream.WriteByte(255);
            //}

            //var p = new Property();

            //Type type = p.GetType();

            //var ee = Activator.CreateInstance(type);

            using (var fileStream = File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                //byte[] span = new byte[fileStream.Length];
                byte[] span = new byte[1];
                //var res = fileStream.Read(span);
                var res = fileStream.Read(span);
                Console.WriteLine(res);
                Console.WriteLine(Encoding.ASCII.GetString(span));
                foreach (var item in span)
                {
                    Console.WriteLine(item);
                }
            }
            //BitConverter;
        }
    }
}
