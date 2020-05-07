using blynk;
using System;

namespace BlynkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //auth码初始化BlynkConnect对象
            BlynkConnect blynk1 = new BlynkConnect("*****************");
            Console.WriteLine("App Online:" + blynk1.ApplicationIsOnline());
            Console.WriteLine("Hardware Online:" + blynk1.HardwareIsOnline());
            //Console.WriteLine(blynk1.GetProject());
            
            //Console.WriteLine("image filename:" + blynk1.CloneProject());
            Console.WriteLine("zip filename:" + blynk1.GetHistoryData("V5"));
        }
    }
}
