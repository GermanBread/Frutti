using System;
using System.Collections.Generic;

namespace BassTest
{
    public static class ListUtilities
    {
        public static void Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = new Random().Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;
            }  
        }
    }
}