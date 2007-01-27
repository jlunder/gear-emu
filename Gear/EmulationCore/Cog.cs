/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * Cog.CS
 * Base class for a cog processor.  Abstract and must be extended.
 * --------------------------------------------------------------------------------
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Gear.EmulationCore
{
    public enum CogRunState
    {
        STATE_EXECUTE,          // Waiting for instruction to finish executing

        WAIT_LOAD_PARAM,        // Cog is loading it's PARAMeter
        WAIT_LOAD_PROGRAM,      // Cog is loading program memory
        WAIT_CYCLES,            // Cog is executing an instruction, and waiting an alloted ammount of cycles
        WAIT_PREWAIT,           // Waits for an allotted number of cycles before changing to a new state

        BOOT_INTERPRETER,       // Interpreter is booting up
        WAIT_INTERPRETER,       // Interpreter is executing an instruction
        EXEC_INTERPRETER,       // Interpreter is fetching instruction

        WAIT_PEQ,               // Waits for pins to match
        WAIT_PNE,               // Waits for pins to NOT match
        WAIT_CNT,               // Waits for count
        WAIT_VID,               // Waits for video

        HUB_RDBYTE,             // Waiting to read byte
        HUB_RDWORD,             // Waiting to read word
        HUB_RDLONG,             // Waiting to read uint
        HUB_HUBOP,              // Waiting to perform hub operation
    }

    public enum CogSpecialAddress : uint
    {
        COGID       = 0x1E9,
        INITCOGID   = 0x1EF,
        PAR         = 0x1F0,
        CNT         = 0x1F1,
        INA         = 0x1F2,
        INB         = 0x1F3,
        OUTA        = 0x1F4,
        OUTB        = 0x1F5,
        DIRA        = 0x1F6,
        DIRB        = 0x1F7,
        CNTA        = 0x1F8,
        CNTB        = 0x1F9,
        FRQA        = 0x1FA,
        FRQB        = 0x1FB,
        PHSA        = 0x1FC,
        PHSB        = 0x1FD,
        VCFG        = 0x1FE,
        VSCL        = 0x1FF
    }

    public enum CogConditionCodes : uint
    {
        IF_NEVER        = 0x00,
        IF_A            = 0x01,
        IF_NC_AND_NZ    = 0x01,
        IF_NZ_AND_NC    = 0x01,
        IF_NC_AND_Z     = 0x02,
        IF_Z_AND_NC     = 0x02,
        IF_NC           = 0x03,
        IF_AE           = 0x03,
        IF_NZ_AND_C     = 0x04,
        IF_C_AND_NZ     = 0x04,
        IF_NZ           = 0x05,
        IF_NE           = 0x05,
        IF_C_NE_Z       = 0x06,
        IF_Z_NE_C       = 0x06,
        IF_NC_OR_NZ     = 0x07,
        IF_NZ_OR_NC     = 0x07,
        IF_C_AND_Z      = 0x08,
        IF_Z_AND_C      = 0x08,
        IF_C_EQ_Z       = 0x09,
        IF_Z_EQ_C       = 0x09,
        IF_E            = 0x0A,
        IF_Z            = 0x0A,
        IF_NC_OR_Z      = 0x0B,
        IF_Z_OR_NC      = 0x0B,
        IF_B            = 0x0C,
        IF_C            = 0x0C,
        IF_NZ_OR_C      = 0x0D,
        IF_C_OR_NZ      = 0x0D,
        IF_Z_OR_C       = 0x0E,
        IF_BE           = 0x0E,
        IF_C_OR_Z       = 0x0E,
        IF_ALWAYS       = 0x0F
    }

    abstract public partial class Cog
    {
        // Runtime variables
        protected uint[] Memory;            // Program Memory

        protected Propeller Hub;            // Host processor
        protected uint PC;                  // Program Cursor

        protected int StateCount;           // Arguement for the current state
        protected CogRunState State;        // Current COG state
        protected CogRunState NextState;    // Next state COG state

        protected uint ProgramAddress;
        protected uint ParamAddress;

        protected FreqGenerator FreqA;
        protected FreqGenerator FreqB;
        protected VideoGenerator Video;
        protected PLLGroup PhaseLockedLoop;

        public Cog(Propeller host,
            uint programAddress, uint paramAddress, uint frequency,
            PLLGroup pll)
        {
            Hub = host;

            Memory = new uint[0x200];     // 512 bytes of memory
            ProgramAddress = programAddress;
            ParamAddress = paramAddress;

            FreqA = new FreqGenerator(host, pll, true);
            FreqB = new FreqGenerator(host, pll, false);
            Video = new VideoGenerator(host);
            PhaseLockedLoop = pll;

            // Attach the video generator to PLLs
            PhaseLockedLoop.SetupPLL(Video);

            PC = 0;

            // We are in boot time load
            State = CogRunState.WAIT_LOAD_PARAM;

            // Clear the special purpose registers
            for (int i = (int)CogSpecialAddress.CNT; i <= 0x1FF; i++)
                this[i] = 0;

            SetClock(frequency);
        }

        public ulong OUT
        {
            get 
            {
                return Memory[(int)CogSpecialAddress.OUTA] |
                    (Memory[(int)CogSpecialAddress.OUTB] << 32) |
                    FreqA.Output |
                    FreqB.Output |
                    Video.Output;
            }
        }

        public uint OUTA
        {
            get 
            {
                return Memory[(int)CogSpecialAddress.OUTA] |
                    ((uint)FreqA.Output |
                    (uint)FreqB.Output |
                    (uint)Video.Output);
            }
        }

        public uint OUTB
        {
            get
            {
                return
                    (uint)((FreqA.Output |
                    FreqB.Output |
                    Video.Output) >> 32) |
                    Memory[(int)CogSpecialAddress.OUTB];
            }
        }

        public ulong DIR
        {
            get
            {
                return (Memory[(int)CogSpecialAddress.DIRB] << 32) |
                    Memory[(int)CogSpecialAddress.DIRA];
            }
        }

        public uint DIRA
        {
            get
            {
                return Memory[(int)CogSpecialAddress.DIRA];
            }
        }

        public uint DIRB
        {
            get
            {
                return Memory[(int)CogSpecialAddress.DIRB];
            }
        }

        public uint ProgramCursor
        {
            get { return PC; }
            set { PC = value; }
        }

        public string CogState
        {
            get
            {
                switch (State)
                {
                    case CogRunState.HUB_HUBOP:
                    case CogRunState.HUB_RDBYTE:
                    case CogRunState.HUB_RDWORD:
                    case CogRunState.HUB_RDLONG:
                        return "Waiting for hub";
                    case CogRunState.BOOT_INTERPRETER:
                        return "Interpreter Boot";
                    case CogRunState.EXEC_INTERPRETER:
                        return "Interpreter Fetch";
                    case CogRunState.WAIT_INTERPRETER:
                        return "Interpreter Processing";
                    case CogRunState.STATE_EXECUTE:
                    case CogRunState.WAIT_PREWAIT:
                    case CogRunState.WAIT_CYCLES:
                        return "Running instruction";
                    case CogRunState.WAIT_LOAD_PARAM:
                        return "Loading Parameter";
                    case CogRunState.WAIT_LOAD_PROGRAM:
                        return "Loading Program";
                    case CogRunState.WAIT_CNT:
                        return "Waiting (CNT)";
                    case CogRunState.WAIT_PEQ:
                        return "Waiting (PEQ)";
                    case CogRunState.WAIT_PNE:
                        return "Waiting (PNE)";
                    case CogRunState.WAIT_VID:
                        return "Waiting (video)";
                    default:
                        return "ERROR";
                }
            }
        }

        public uint this[int i]
        {
            get
            {
                if (i >= 0x200)
                    return 0x55;
                return Memory[i];
            }

            set
            {
                if (i < 0x200)
                    Memory[i] = value;
            }
        }

        public static bool ConditionCompare(CogConditionCodes condition, bool a, bool b)
        {
            switch (condition)
            {
                case CogConditionCodes.IF_NEVER:
                    break;
                case CogConditionCodes.IF_NZ_AND_NC:
                    if (!a && !b)
                        return false;
                    break;
                case CogConditionCodes.IF_NC_AND_Z:
                    if (a && !b)
                        return false;
                    break;
                case CogConditionCodes.IF_NC:
                    if (!b)
                        return false;
                    break;
                case CogConditionCodes.IF_C_AND_NZ:
                    if (!a && b)
                        return false;
                    break;
                case CogConditionCodes.IF_NZ:
                    if (!a)
                        return false;
                    break;
                case CogConditionCodes.IF_C_NE_Z:
                    if (a != b)
                        return false;
                    break;
                case CogConditionCodes.IF_NC_OR_NZ:
                    if (!a || !b)
                        return false;
                    break;
                case CogConditionCodes.IF_C_AND_Z:
                    if (a && b)
                        return false;
                    break;
                case CogConditionCodes.IF_C_EQ_Z:
                    if (a == b)
                        return false;
                    break;
                case CogConditionCodes.IF_Z:
                    if (a)
                        return false;
                    break;
                case CogConditionCodes.IF_NC_OR_Z:
                    if (a || !b)
                        return false;
                    break;
                case CogConditionCodes.IF_C:
                    if (b)
                        return false;
                    break;
                case CogConditionCodes.IF_C_OR_NZ:
                    if (!a || b)
                        return false;
                    break;
                case CogConditionCodes.IF_Z_OR_C:
                    if (a || b)
                        return false;
                    break;
                case CogConditionCodes.IF_ALWAYS:
                    return false;
            }

            return true;
        }

        public virtual void HubAccessable()
        {
            switch (State)
            {
                case CogRunState.WAIT_LOAD_PARAM:
                    Memory[(int)CogSpecialAddress.PAR] = Hub.ReadLong(ParamAddress);
                    State = CogRunState.WAIT_LOAD_PROGRAM;
                    StateCount = 0;
                    break;
                case CogRunState.WAIT_LOAD_PROGRAM:
                    Memory[StateCount++] = Hub.ReadLong(ProgramAddress);
                    ProgramAddress += 4;

                    if (StateCount == 0x1F0)
                    {
                        StateCount = 0;
                        Boot();
                    }
                    break;
            }
        }

        public void DetachVideoHooks()
        {
            // Detach the video hook 
            PhaseLockedLoop.Destroy();
            // Detach the aural hook
            Video.DetachAural();
        }

        public void Step()
        {
            DoInstruction();
            
            // Run our frequency counters
            FreqA.Tick(Hub.IN);
            FreqB.Tick(Hub.IN);
        }

        public void SetClock(uint freq)
        {
            FreqA.SetClock(freq);
            FreqB.SetClock(freq);
        }

        public void StepInstruction()
        {
            int i = 0x2000;    // Maximum of 8k clocks (covers load instruction)
            do
            {
                Hub.Step();
            }
            while (State != CogRunState.EXEC_INTERPRETER &&
                State != CogRunState.STATE_EXECUTE && 
                --i > 0);
                
        }

        protected uint ReadLong(uint address)
        {
            switch( address & 0x1FF )
            {
                case 0x1F1:
                    return Hub.Counter;
                case 0x1F2:
                    return Hub.INA;
                case 0x1F3:
                    return Hub.INB;
                case 0x1F8:
                    return FreqA.CTR;
                case 0x1F9:
                    return FreqB.CTR;
                case 0x1FA:
                    return FreqA.FRQ;
                case 0x1FB:
                    return FreqB.FRQ;
                case 0x1FC:
                    return FreqA.PHS;
                case 0x1FD:
                    return FreqB.PHS;
                case 0x1FE:
                    return Video.CFG;
                case 0x1FF:
                    return Video.SCL;
                default:
                    return Memory[address & 0x1FF];
            }
        }

        protected void WriteLong(uint address, uint data)
        {
            switch (address & 0x1FF)
            {
                // Read only registers
                case 0x1F0:
                case 0x1F1:
                case 0x1F2:
                case 0x1F3:
                    return;
                case 0x1F8:
                    FreqA.CTR = data;
                    break;
                case 0x1F9:
                    FreqB.CTR = data;
                    break;
                case 0x1FA:
                    FreqA.FRQ = data;
                    break;
                case 0x1FB:
                    FreqB.FRQ = data;
                    break;
                case 0x1FC:
                    FreqA.PHS = data;
                    break;
                case 0x1FD:
                    FreqB.PHS = data;
                    break;
                case 0x1FE:
                    Video.CFG = data;
                    break;
                case 0x1FF:
                    Video.SCL = data;
                    break;
                default:
                    Memory[address & 0x1FF] = data;
                    return;
            }
        }

        abstract public void DoInstruction();
        abstract public void Boot();
    }
}