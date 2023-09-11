﻿namespace ServerCore
{
    internal class Program
    {
        volatile static bool _stop = false;

        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];

            // 공간 지역성에 따라 속도 차이 발생(캐시)
            {
                long now = DateTime.Now.Ticks;
                for (int i = 0; i < 10000; i++)
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        arr[i, j] = 1;
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(i,j) 순서 걸린 시간 {end - now}");
            }

            {
                long now = DateTime.Now.Ticks;
                for (int i = 0; i < 10000; i++)
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        arr[j, i] = 1;
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(j,i) 순서 걸린 시간 {end - now}");
            }


        }
    }
}