// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal class Program
{
    //*
    [ThreadStatic] private static string TextFileName = @"D:\git\corert\src\ILCompiler\repro\Program.cs";

    [ThreadStatic] private static int LineCount = 0;

    private static bool NewString()
    {
        string s = new string('x', 10);
        return s.Length == 10;
    }

    private static bool WriteLine()
    {
        Console.WriteLine("Hello CoreRT R2R running on CoreCLR!");
        return true;
    }

    private static bool IsInstanceOf()
    {
        object obj = TextFileName;
        if (obj is string str)
        {
            Console.WriteLine($@"Object is string: {str}");
            return true;
        }
        else
        {
            Console.Error.WriteLine($@"Object is not a string: {obj}");
            return false;
        }
    }

    private static bool IsInstanceOfValueType()
    {
        object obj = LineCount;
        if (obj is int i)
        {
            Console.WriteLine($@"Object is int: {i}");
            return true;
        }
        else
        {
            Console.Error.WriteLine($@"Object is not an int: {obj}");
            return false;
        }
    }

    private static bool ChkCast()
    {
        object obj = TextFileName;
        string objString = (string)obj;
        Console.WriteLine($@"String: {objString}");
        return objString == TextFileName;
    }

    private static bool ChkCastValueType()
    {
        object obj = LineCount;
        int objInt = (int)obj;
        Console.WriteLine($@"Int: {objInt}");
        return objInt == LineCount;
    }

    private static bool BoxUnbox()
    {
        bool success = true;
        object intAsObject = LineCount;
        if ((int)intAsObject != LineCount)
        {
            Console.Error.WriteLine($@"unbox != box: original {LineCount}, boxed {intAsObject}, unboxed {(int)intAsObject}");
            success = false;
        }
        int? nullableInt = LineCount;
        object nullableIntAsObject = nullableInt;
        if ((int?)nullableIntAsObject != nullableInt)
        {
            Console.Error.WriteLine($@"unbox_nullable != box_nullable: original {nullableInt}, boxed {nullableIntAsObject}, unboxed {(int?)nullableIntAsObject}");
        }
        return success;
    }

    private static bool TypeHandle()
    {
        Console.WriteLine(TextFileName.GetType().ToString());
        Console.WriteLine(LineCount.GetType().ToString());
        return true;
    }

    private static bool RuntimeTypeHandle()
    {
        Console.WriteLine(typeof(string).ToString());
        return true;
    }

    private static bool ReadAllText()
    {
        string textFile = File.ReadAllText(TextFileName);
        Console.WriteLine(textFile);

        return textFile.Length > 0;
    }

    private static bool StreamReaderReadLine()
    {
        using (StreamReader reader = new StreamReader(TextFileName, System.Text.Encoding.UTF8))
        {
            for (; ; )
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                LineCount++;
                Console.WriteLine($@"{LineCount}: {line}");
            }
            return LineCount > 0;
        }
    }

    private static bool ConstructListOfInt()
    {
        List<int> listOfInt = new List<int>();
        return listOfInt.Count == 0;
    }

    public static int Main()
    {
        const int Success = 1;
        const int Failure = 0;

        int[] TestCounts = new int[2];

        TestCounts[NewString() ? Success : Failure]++;
        TestCounts[WriteLine() ? Success : Failure]++;
        TestCounts[IsInstanceOf() ? Success : Failure]++;
        TestCounts[IsInstanceOfValueType() ? Success : Failure]++;
        TestCounts[ChkCast() ? Success : Failure]++;
        TestCounts[ChkCastValueType() ? Success : Failure]++;
        TestCounts[BoxUnbox() ? Success : Failure]++;
        TestCounts[TypeHandle() ? Success : Failure]++;
        TestCounts[RuntimeTypeHandle() ? Success : Failure]++;
        TestCounts[ReadAllText() ? Success : Failure]++;
        TestCounts[StreamReaderReadLine() ? Success : Failure]++;
        TestCounts[ConstructListOfInt() ? Success : Failure]++;

        if (TestCounts[Failure] == 0)
        {
            Console.WriteLine($@"All {TestCounts[Success]} tests pass!");
            return 100;
        }
        else
        {
            Console.Error.WriteLine($@"{TestCounts[Failure]} test failed, {TestCounts[Success]} suceeded.");
            return 1;
        }
    }
    //*/

    /*
    public static int Main()
    {
        return 100;
    }
    //*/

    /// <summary>
    /// This table demonstrates progress in implementing R2R fixups in CoreRT.
    /// For those fixups the status of which has already been assessed, I have
    /// commented their enum values out with the following description:
    /// // DONE - fixup has been implemented
    /// // JIT - fixup implementation is blocked on larger JIT interface changes
    /// // UNNEEDED - fixup is not needed (additional info should be supplied)
    /// </summary>
    public enum ReadyToRunFixupKind
    {
        READYTORUN_FIXUP_ThisObjDictionaryLookup = 0x07,
        READYTORUN_FIXUP_TypeDictionaryLookup = 0x08,
        READYTORUN_FIXUP_MethodDictionaryLookup = 0x09,

        // DONE: READYTORUN_FIXUP_TypeHandle = 0x10,
        // JIT: READYTORUN_FIXUP_MethodHandle = 0x11,
        // JIT: READYTORUN_FIXUP_FieldHandle = 0x12,

        // UNNEEDED: READYTORUN_FIXUP_MethodEntry = 0x13, // In CoreRT, we always refer to external methods by RefTokens
        // UNNEEDED: READYTORUN_FIXUP_MethodEntry_DefToken = 0x14, // In CoreRT, we always refer to external methods by RefTokens
        // DONE: READYTORUN_FIXUP_MethodEntry_RefToken = 0x15, /* Smaller version of MethodEntry - method is ref token */

        // UNNEEDED: READYTORUN_FIXUP_VirtualEntry = 0x16, // In CoreRT, we always refer to external methods by RefTokens
        // UNNEEDED: READYTORUN_FIXUP_VirtualEntry_DefToken = 0x17, // In CoreRT, we always refer to external methods by RefTokens
        // DONE: READYTORUN_FIXUP_VirtualEntry_RefToken = 0x18, /* Smaller version of VirtualEntry - method is ref token */
        // UNNEEDED: EADYTORUN_FIXUP_VirtualEntry_Slot = 0x19, // In CoreRT, we always refer to external methods by RefTokens

        READYTORUN_FIXUP_Helper = 0x1A, /* Helper */
        // DONE: READYTORUN_FIXUP_StringHandle = 0x1B, /* String handle */

        // DONE: READYTORUN_FIXUP_NewObject = 0x1C, /* Dynamically created new helper */
        // DONE: READYTORUN_FIXUP_NewArray = 0x1D,

        // DONE: READYTORUN_FIXUP_IsInstanceOf = 0x1E, /* Dynamically created casting helper */
        // DONE: READYTORUN_FIXUP_ChkCast = 0x1F,

        READYTORUN_FIXUP_FieldAddress = 0x20, /* For accessing a cross-module static fields */
        READYTORUN_FIXUP_CctorTrigger = 0x21, /* Static constructor trigger */

        // DONE: READYTORUN_FIXUP_StaticBaseNonGC = 0x22, /* Dynamically created static base helpers */
        // DONE: READYTORUN_FIXUP_StaticBaseGC = 0x23,
        // DONE: READYTORUN_FIXUP_ThreadStaticBaseNonGC = 0x24,
        // DONE: READYTORUN_FIXUP_ThreadStaticBaseGC = 0x25,

        READYTORUN_FIXUP_FieldBaseOffset = 0x26, /* Field base offset */
        READYTORUN_FIXUP_FieldOffset = 0x27, /* Field offset */

        READYTORUN_FIXUP_TypeDictionary = 0x28,
        READYTORUN_FIXUP_MethodDictionary = 0x29,

        READYTORUN_FIXUP_Check_TypeLayout = 0x2A, /* size, alignment, HFA, reference map */
        READYTORUN_FIXUP_Check_FieldOffset = 0x2B,

        READYTORUN_FIXUP_DelegateCtor = 0x2C, /* optimized delegate ctor */
        READYTORUN_FIXUP_DeclaringTypeHandle = 0x2D,
    }

    public enum ReadyToRunHelper
    {
        // UNNEEDED: READYTORUN_HELPER_Invalid = 0x00,

        // Not a real helper - handle to current module passed to delay load helpers.
        // DONE: READYTORUN_HELPER_Module = 0x01,
        READYTORUN_HELPER_GSCookie = 0x02,

        //
        // Delay load helpers
        //

        // All delay load helpers use custom calling convention:
        // - scratch register - address of indirection cell. 0 = address is inferred from callsite.
        // - stack - section index, module handle
        // DONE: READYTORUN_HELPER_DelayLoad_MethodCall = 0x08,

        // DONE: READYTORUN_HELPER_DelayLoad_Helper = 0x10,
        READYTORUN_HELPER_DelayLoad_Helper_Obj = 0x11,
        READYTORUN_HELPER_DelayLoad_Helper_ObjObj = 0x12,

        // JIT helpers

        // Exception handling helpers
        READYTORUN_HELPER_Throw = 0x20,
        READYTORUN_HELPER_Rethrow = 0x21,
        READYTORUN_HELPER_Overflow = 0x22,
        // DONE: READYTORUN_HELPER_RngChkFail = 0x23,
        READYTORUN_HELPER_FailFast = 0x24,
        READYTORUN_HELPER_ThrowNullRef = 0x25,
        READYTORUN_HELPER_ThrowDivZero = 0x26,

        // Write barriers
        READYTORUN_HELPER_WriteBarrier = 0x30,
        READYTORUN_HELPER_CheckedWriteBarrier = 0x31,
        READYTORUN_HELPER_ByRefWriteBarrier = 0x32,

        // Array helpers
        READYTORUN_HELPER_Stelem_Ref = 0x38,
        READYTORUN_HELPER_Ldelema_Ref = 0x39,

        READYTORUN_HELPER_MemSet = 0x40,
        READYTORUN_HELPER_MemCpy = 0x41,

        // Get string handle lazily
        READYTORUN_HELPER_GetString = 0x50,

        // Used by /Tuning for Profile optimizations
        READYTORUN_HELPER_LogMethodEnter = 0x51,

        // Reflection helpers
        // DONE: READYTORUN_HELPER_GetRuntimeTypeHandle = 0x54,
        READYTORUN_HELPER_GetRuntimeMethodHandle = 0x55,
        READYTORUN_HELPER_GetRuntimeFieldHandle = 0x56,

        // DONE: READYTORUN_HELPER_Box = 0x58,
        // DONE: READYTORUN_HELPER_Box_Nullable = 0x59,
        // DONE: READYTORUN_HELPER_Unbox = 0x5A,
        // DONE: READYTORUN_HELPER_Unbox_Nullable = 0x5B,
        READYTORUN_HELPER_NewMultiDimArr = 0x5C,
        READYTORUN_HELPER_NewMultiDimArr_NonVarArg = 0x5D,

        // Helpers used with generic handle lookup cases
        // DONE: READYTORUN_HELPER_NewObject = 0x60,
        READYTORUN_HELPER_NewArray = 0x61,
        READYTORUN_HELPER_CheckCastAny = 0x62,
        READYTORUN_HELPER_CheckInstanceAny = 0x63,
        READYTORUN_HELPER_GenericGcStaticBase = 0x64,
        READYTORUN_HELPER_GenericNonGcStaticBase = 0x65,
        READYTORUN_HELPER_GenericGcTlsBase = 0x66,
        READYTORUN_HELPER_GenericNonGcTlsBase = 0x67,
        READYTORUN_HELPER_VirtualFuncPtr = 0x68,

        // Long mul/div/shift ops
        READYTORUN_HELPER_LMul = 0xC0,
        READYTORUN_HELPER_LMulOfv = 0xC1,
        READYTORUN_HELPER_ULMulOvf = 0xC2,
        READYTORUN_HELPER_LDiv = 0xC3,
        READYTORUN_HELPER_LMod = 0xC4,
        READYTORUN_HELPER_ULDiv = 0xC5,
        READYTORUN_HELPER_ULMod = 0xC6,
        READYTORUN_HELPER_LLsh = 0xC7,
        READYTORUN_HELPER_LRsh = 0xC8,
        READYTORUN_HELPER_LRsz = 0xC9,
        READYTORUN_HELPER_Lng2Dbl = 0xCA,
        READYTORUN_HELPER_ULng2Dbl = 0xCB,

        // 32-bit division helpers
        READYTORUN_HELPER_Div = 0xCC,
        READYTORUN_HELPER_Mod = 0xCD,
        READYTORUN_HELPER_UDiv = 0xCE,
        READYTORUN_HELPER_UMod = 0xCF,

        // Floating point conversions
        READYTORUN_HELPER_Dbl2Int = 0xD0,
        READYTORUN_HELPER_Dbl2IntOvf = 0xD1,
        READYTORUN_HELPER_Dbl2Lng = 0xD2,
        READYTORUN_HELPER_Dbl2LngOvf = 0xD3,
        READYTORUN_HELPER_Dbl2UInt = 0xD4,
        READYTORUN_HELPER_Dbl2UIntOvf = 0xD5,
        READYTORUN_HELPER_Dbl2ULng = 0xD6,
        READYTORUN_HELPER_Dbl2ULngOvf = 0xD7,

        // Floating point ops
        READYTORUN_HELPER_DblRem = 0xE0,
        READYTORUN_HELPER_FltRem = 0xE1,
        READYTORUN_HELPER_DblRound = 0xE2,
        READYTORUN_HELPER_FltRound = 0xE3,

        // Personality rountines
        READYTORUN_HELPER_PersonalityRoutine = 0xF0,
        READYTORUN_HELPER_PersonalityRoutineFilterFunclet = 0xF1,

        //
        // Deprecated/legacy
        //

        // JIT32 x86-specific write barriers
        READYTORUN_HELPER_WriteBarrier_EAX = 0x100,
        READYTORUN_HELPER_WriteBarrier_EBX = 0x101,
        READYTORUN_HELPER_WriteBarrier_ECX = 0x102,
        READYTORUN_HELPER_WriteBarrier_ESI = 0x103,
        READYTORUN_HELPER_WriteBarrier_EDI = 0x104,
        READYTORUN_HELPER_WriteBarrier_EBP = 0x105,
        READYTORUN_HELPER_CheckedWriteBarrier_EAX = 0x106,
        READYTORUN_HELPER_CheckedWriteBarrier_EBX = 0x107,
        READYTORUN_HELPER_CheckedWriteBarrier_ECX = 0x108,
        READYTORUN_HELPER_CheckedWriteBarrier_ESI = 0x109,
        READYTORUN_HELPER_CheckedWriteBarrier_EDI = 0x10A,
        READYTORUN_HELPER_CheckedWriteBarrier_EBP = 0x10B,

        // JIT32 x86-specific exception handling
        READYTORUN_HELPER_EndCatch = 0x110,
    };
}
