using System;
using System.Threading;
namespace semaphore

{
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
            for (int j = 0; j < 28; j++)
            {
                for (int i = 0; i < 80; i++)
                {
                    toKids[i].Set();//Notify ALL kidThreads
                }
                
                for (int i = 0; i < 80; i++)
                {
                    toMain[i].WaitOne();//Wait for notification from kidThread
                }

                Console.WriteLine($"Line # {j,2} is: {new string(myLine)}");
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
            char[] choseList = { 'a', 'b', 'c', 'd', 'e' };//Required: use A,B,C,D,E and then cycle back to A,B,C,D,E cycle to A,B,C,D,E ...continued...
            int position = 0;
            while (kidsCanRun)
            {
                try
                {
                    //Blocks the current thread until the current WaitHandle receives a signal.   
                    toKids[i].WaitOne();
                    if (position >= choseList.Length)
                    {
                        position = 0;
                        //reset to start of string.
                    }
                    myLine[i] = choseList[position];
                    position++;
                }
                finally
                {
                    //method to singal semaphore  to main
                    toMain[i].Set();
                }
            }
        }
    }
}