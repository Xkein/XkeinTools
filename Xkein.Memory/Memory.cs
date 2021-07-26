using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xkein.Memory
{
    class Memory
    {
        public Memory(IntPtr address, int size)
        {
            Address = address;
            Size = size;
        }

        public IntPtr Address { get; }
        public int Size { get; }
        public int Begin => (int)Address;
        public int End => Begin + Size;

        public static implicit operator IntPtr(Memory memory)
        {
            return memory.Address;
        }
    }

    class Container : Memory
    {
        struct Range
        {
            public Range(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start;
            public int End;
            public int Length => End - Start;

            public static bool operator !=(Range left, Range right)
            {
                return !(left == right);
            }
            public static bool operator ==(Range left, Range right)
            {
                return left.Start == right.Start && left.End == right.End;
            }

        }

        public Container(IntPtr address, int size) : base(address, size)
        {
        }

        public bool TryAlloc(int length, out Memory memory)
        {
            lock (this)
            {
                if (_used.Count == 0)
                {
                    memory = new Memory(Address, length);
                    _used.Add(memory);
                    return true;
                }

                List<Range> usedRanges = (from m in _used orderby m.Begin select new Range(m.Begin, m.End)).ToList();
                List<Range> unusedRanges = new List<Range>();

                if (this.Begin < usedRanges[0].Start)
                {
                    unusedRanges.Add(new Range(this.Begin, usedRanges[0].Start));
                }

                for (int i = 0; i < usedRanges.Count - 1; i++)
                {
                    unusedRanges.Add(new Range(usedRanges[i].End, usedRanges[i + 1].Start));
                }

                if (usedRanges.Last().End < this.End)
                {
                    unusedRanges.Add(new Range(usedRanges.Last().End, this.End));
                }

                Range fit = unusedRanges.Find(r => r.Length >= length);
                if (fit == default(Range))
                {
                    memory = null;
                    return false;
                }

                memory = new Memory((IntPtr)fit.Start, length);
                _used.Add(memory);
                return true;
            }
        }

        public void Free(IntPtr address)
        {
            lock (this)
            {
                Memory memory = _used.Find(m => m.Begin <= (int)address && (int)address <= m.End);

                if (memory is null)
                {
                    throw new InvalidOperationException($"Could not free memory at 0x{address:X}");
                }

                _used.Remove(memory);
            }
        }

        private List<Memory> _used = new List<Memory>();
    }

}
