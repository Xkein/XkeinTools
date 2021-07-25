using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using XkeinTools.Memory;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Allocator allocator = new Allocator(
                (length) => Marshal.AllocHGlobal(length),
                (address) => Marshal.FreeHGlobal(address));

            IntPtr ptr1 = allocator.Alloc(16);
            IntPtr ptr2 = allocator.Alloc(32);
            Console.WriteLine($"ptr1: {(int)ptr1:X}, ptr2: {(int)ptr2:X}");
            allocator.Free(ptr1);
            Console.WriteLine("free ptr1.");
            IntPtr ptr3 = allocator.Alloc(8);
            IntPtr ptr4 = allocator.Alloc(8);
            Console.WriteLine($"ptr3: {(int)ptr3:X}, ptr4: {(int)ptr4:X}");
        }
    }
}
