using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gear.Propeller
{
    public interface IDirectMemory
    {
        byte this[int offset] { get; }
        byte DirectReadByte(uint address);
        ushort DirectReadWord(uint address);
        uint DirectReadLong(uint address);
        void DirectWriteByte(uint address, byte value);
        void DirectWriteWord(uint address, ushort value);
        void DirectWriteLong(uint address, uint value);
    }

    public class MemoryManager
    {
        private IDirectMemory Memory;
        public int Address { get; protected set; }

        public MemoryManager(IDirectMemory Memory, int Address = 0)
        {
            this.Memory = Memory;
            this.Address = Address;
        }

        public byte ReadByte()
        {
            return this.Memory.DirectReadByte((uint)this.Address);
        }

        public ushort ReadWord()
        {
            this.Address &= ~1;
            ushort read = this.Memory.DirectReadWord((uint)this.Address);
            this.Address += 2;
            return read;
        }

        public uint ReadLong()
        {
            this.Address &= ~3;
            uint read = this.Memory.DirectReadLong((uint)this.Address);
            this.Address += 4;
            return read;
        }

        public void WriteByte(byte value)
        {
            this.Memory.DirectWriteByte((uint)this.Address++, value);
        }

        public void WriteWord(ushort value)
        {
            this.Address &= ~1;
            this.Memory.DirectWriteWord((uint)this.Address, value);
            this.Address += 2;
        }

        public void WriteLong(uint value)
        {
            this.Address &= ~3;
            this.Memory.DirectWriteLong((uint)this.Address, value);
            this.Address += 4;
        }
    }
}
