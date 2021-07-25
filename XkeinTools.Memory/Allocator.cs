using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XkeinTools.Memory
{
    public class Allocator
    {
        const int PAGE_SIZE = 1024;
        public Allocator(Func<int, IntPtr> allocate, Action<IntPtr> deallocate)
        {
            _allocate = allocate ?? throw new ArgumentNullException(nameof(allocate));
            _deallocate = deallocate ?? throw new ArgumentNullException(nameof(deallocate));
        }

        public IntPtr Alloc(int length)
        {
            lock (this)
            {
                foreach (var container in containers)
                {
                    if (container.TryAlloc(length, out Memory memory))
                    {
                        return memory.Address;
                    }
                }

                {
                    var container = new Container(_allocate(PAGE_SIZE), PAGE_SIZE);
                    containers.Add(container);
                    if (container.TryAlloc(length, out Memory memory))
                    {
                        return memory.Address;
                    }
                }

                return IntPtr.Zero;
            }
        }

        public void Free(IntPtr address)
        {
            lock (this)
            {
                foreach (var container in containers)
                {
                    try
                    {
                        container.Free(address);
                        return;
                    }
                    catch (InvalidOperationException)
                    {

                    }
                }
            }
        }

        private Func<int, IntPtr> _allocate;
        private Action<IntPtr> _deallocate;
        private List<Container> containers = new List<Container>();

    }
}
