// Metadata version: v2.0.50727
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 2:0:0:0
}

.assembly SequenceOfNestedIfs
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module SequenceOfNestedIfs
// MVID: {DCEC8A87-5679-4EBE-89A3-51274D8B5446}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x01D60000


// =============== CLASS MEMBERS DECLARATION ===================

.class private auto ansi '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'
	extends [mscorlib]System.Object
{
	.custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = (
		01 00 00 00
	)
	// Nested Types
	.class nested private explicit ansi sealed '__StaticArrayInitTypeSize=48'
		extends [mscorlib]System.ValueType
	{
		.pack 1
		.size 48

	} // end of class __StaticArrayInitTypeSize=48


	// Fields
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000006-1'
//	.field assembly static valuetype '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'/'__StaticArrayInitTypeSize=48' '$$method0x600089f-1' at I_0002d800
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000901-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000901-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000902-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000902-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000903-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000903-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090c-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090c-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090d-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090d-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090e-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x600090e-2'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000913-1'
	.field assembly static class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '$$method0x6000913-2'
//	.field assembly static valuetype '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'/'__StaticArrayInitTypeSize=48' '$$method0x6000916-1' at I_0003e028

} // end of class <PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}


.class public auto ansi serializable beforefieldinit StringSwitchTest
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void  [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method StringSwitchTest::.ctor

  .method public hidebysig static 
	string TestMethod (
		string switchCondition
	) cil managed 
{
	// Method begins at RVA 0x3ca90
	// Code size 1037 (0x40d)
	.maxstack 4
	.locals init (
		[0] string,
		[1] string,
		[2] int32,
		[3] string,
		[4] int32,
		[5] string
	)

	IL_0000: ldsfld string [mscorlib]System.String::Empty
	IL_0005: stloc.0

	IL_0016: ldarg.0
	IL_0017: dup
	IL_0018: stloc.1
	IL_0019: brfalse IL_01c0

	IL_001e: volatile.
	IL_0020: ldsfld class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'::'$$method0x600090e-1'
	IL_0025: brtrue IL_00f2

	IL_002a: ldc.i4.s 15
	IL_002c: newobj instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::.ctor(int32)
	IL_0031: dup
	IL_0032: ldstr "Item1"
	IL_0037: ldc.i4.0
	IL_0038: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_003d: dup
	IL_003e: ldstr "Item2"
	IL_0043: ldc.i4.1
	IL_0044: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0049: dup
	IL_004a: ldstr "Item3"
	IL_004f: ldc.i4.2
	IL_0050: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0055: dup
	IL_0056: ldstr "Item4"
	IL_005b: ldc.i4.3
	IL_005c: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0061: dup
	IL_0062: ldstr "Item5"
	IL_0067: ldc.i4.4
	IL_0068: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_006d: dup
	IL_006e: ldstr "Item6"
	IL_0073: ldc.i4.5
	IL_0074: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0079: dup
	IL_007a: ldstr "Item7"
	IL_007f: ldc.i4.6
	IL_0080: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0085: dup
	IL_0086: ldstr "Item8"
	IL_008b: ldc.i4.7
	IL_008c: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_0091: dup
	IL_0092: ldstr "Item9"
	IL_0097: ldc.i4.8
	IL_0098: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_009d: dup
	IL_009e: ldstr "Item10"
	IL_00a3: ldc.i4.s 9
	IL_00a5: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00aa: dup
	IL_00ab: ldstr "Item11"
	IL_00b0: ldc.i4.s 10
	IL_00b2: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00b7: dup
	IL_00b8: ldstr "Item12"
	IL_00bd: ldc.i4.s 11
	IL_00bf: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00c4: dup
	IL_00c5: ldstr "Item13"
	IL_00ca: ldc.i4.s 12
	IL_00cc: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00d1: dup
	IL_00d2: ldstr "Item14"
	IL_00d7: ldc.i4.s 13
	IL_00d9: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00de: dup
	IL_00df: ldstr "Item15"
	IL_00e4: ldc.i4.s 14
	IL_00e6: call instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::Add(!0, !1)
	IL_00eb: volatile.
	IL_00ed: stsfld class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'::'$$method0x600090e-1'

	IL_00f2: volatile.
	IL_00f4: ldsfld class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32> '<PrivateImplementationDetails>{4951B3B6-7505-4A86-A06E-D9F3F7248264}'::'$$method0x600090e-1'
	IL_00f9: ldloc.1
	IL_00fa: ldloca.s 2
	IL_00fc: call instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string, int32>::TryGetValue(!0, !1&)
	IL_0101: brfalse IL_01c0

	IL_0106: ldloc.2
	IL_0107: switch (IL_014a, IL_0152, IL_015a, IL_0162, IL_016a, IL_0172, IL_017a, IL_0182, IL_018a, IL_0192, IL_019a, IL_01a2, IL_01aa, IL_01b2, IL_01ba)

	IL_0148: br.s IL_01c0

	IL_014a: ldstr "1"
	IL_014f: stloc.0
	IL_0150: br.s IL_01c0

	IL_0152: ldstr "2"
	IL_0157: stloc.0
	IL_0158: br.s IL_01c0

	IL_015a: ldstr "3"
	IL_015f: stloc.0
	IL_0160: br.s IL_01c0

	IL_0162: ldstr "4"
	IL_0167: stloc.0
	IL_0168: br.s IL_01c0

	IL_016a: ldstr "5"
	IL_016f: stloc.0
	IL_0170: br.s IL_01c0

	IL_0172: ldstr "6"
	IL_0177: stloc.0
	IL_0178: br.s IL_01c0

	IL_017a: ldstr "7"
	IL_017f: stloc.0
	IL_0180: br.s IL_01c0

	IL_0182: ldstr "8"
	IL_0187: stloc.0
	IL_0188: br.s IL_01c0

	IL_018a: ldstr "9"
	IL_018f: stloc.0
	IL_0190: br.s IL_01c0

	IL_0192: ldstr "10"
	IL_0197: stloc.0
	IL_0198: br.s IL_01c0

	IL_019a: ldstr "11"
	IL_019f: stloc.0
	IL_01a0: br.s IL_01c0

	IL_01a2: ldstr "12"
	IL_01a7: stloc.0
	IL_01a8: br.s IL_01c0

	IL_01aa: ldstr "13"
	IL_01af: stloc.0
	IL_01b0: br.s IL_01c0

	IL_01b2: ldstr "14"
	IL_01b7: stloc.0
	IL_01b8: br.s IL_01c0

	IL_01ba: ldstr "15"
	IL_01bf: stloc.0

	IL_01c0: 	


	IL_040b: ldloc.0
	IL_040c: ret
} // end of method StringSwitchTest::TestMethod

} // end of class StringSwitchTest