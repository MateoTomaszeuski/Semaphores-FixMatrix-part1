using System;
using System.Threading;
namespace semaphore;

class Program
{
    private static char[] myLine = new char[80];
    private static AutoResetEvent[] toKids = new AutoResetEvent[80];
    private static AutoResetEvent[] toMain = new AutoResetEvent[80];
    private static Thread[] tList = new Thread[80];
    private static volatile bool kidsCanRun = true;
    static void Main(string[] args)
    {
        //setup list 
        Console.WriteLine("Program Setup");
        for (int i = 0; i < 80; i++)
        {  //Doing this EIGHTY TIMES... (need to initialize)
            tList[i] = new Thread(MatrixStream);//New Thread initialization
            toKids[i] = new AutoResetEvent(false);//toKid event/semaphore initialization
            toMain[i] = new AutoResetEvent(false);//toMain event/semaphore initialization
            tList[i].Start(i);  //NewThread start
        }
        for (int j = 0; j < 1000; j++)
        {
            for (int i = 0; i < 80; i++)
            {

                toKids[i].Set();//Notify ALL kidThreads
            }
            for (int i = 0; i < 80; i++)
            {
                toMain[i].WaitOne();//Wait for notification from kidThread
            }
            Console.WriteLine($"Line # {j,4} is: {new string(myLine)}");

        }
        kidsCanRun = false;
        for (int i = 0; i < 80; i++)
        {
            toKids[i].Set(); //one last run to break the WAIT, and then the Kid will hear the 'kidsCanRun=False' setting and DIE!
            tList[i].Join();
        }
        Console.WriteLine("All Threads are done.");
    }
    static void MatrixStream(object id) // Id allows me to access a specific box so 2 different threads dont access same signal
    {
        int i = (int)id;
        char[] AllCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char[] LowerCase = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        int position = 0;
        bool wonLottery = true;
        int runs = 0;
        while (kidsCanRun)
        {
            try
            {
                //Blocks the current thread until the current WaitHandle receives a signal.   
                toKids[i].WaitOne();
                if (wonLottery)
                {
                    if (runs % 2 == 0)
                    {
                        myLine[i] = AllCaps[Random.Shared.Next(AllCaps.Length)];
                        position++;
                        runs++;
                        wonLottery = EightyPercent();
                    }
                    else
                    {
                        myLine[i] = LowerCase[Random.Shared.Next(LowerCase.Length)];
                        position++;
                        runs++;
                        wonLottery = EightyPercent();
                    }
                }
                else
                {
                    myLine[i] = ' ';
                    wonLottery = FivePercent();
                }
            }
            finally
            {
                //method to singal semaphore  to main
                toMain[i].Set();

            }
        }
    }

    private static bool FivePercent()
    {
        bool wonLottery;
        if (Random.Shared.Next(0, 19) == 0)
        {
            wonLottery = true;
        }
        else { wonLottery = false; }

        return wonLottery;
    }

    private static bool EightyPercent()
    {
        bool wonLottery;
        if (Random.Shared.Next(0, 4) == 0)
        {
            wonLottery = false;
        }
        else { wonLottery = true; }

        return wonLottery;
    }
}