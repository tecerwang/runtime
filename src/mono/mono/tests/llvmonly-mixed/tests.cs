using System;
using System.Collections;

public class BitcodeMixedTests
{
	public static int Main (String[] args) {
		return TestDriver.RunTests (typeof (BitcodeMixedTests), args);
	}

	public static int test_2_entry_simple () {
		return InterpOnly.entry_1 (1);
	}

	public static int test_0_corlib_call () {
		var alist2 = new ArrayList (10);
		var alist = InterpOnly.corlib_call ();
		return alist.Capacity == 5 ? 0 : 1;
	}

	public static int test_1_entry_vcall () {
		InterpOnlyIFace iface = new InterpOnly ();
		return iface.get_Field2 ();
	}

	public static int test_1_entry_vcall_unbox () {
		InterpOnlyIFace iface = new InterpOnlyStruct () { Field = 1 };
		return iface.get_Field2 ();
	}

	public static int test_0_entry_vcall_virtual () {
		InterpOnlyIFace iface = new InterpOnly ();
		return iface.virt<int> () == typeof(int) ? 0 : 1;
	}

	public static int test_2_entry_delegate () {
		Func<int, int> func = InterpOnly.entry_1;

		return func (1);
	}

	public static int test_1_entry_delegate_unbox () {
		var s = new InterpOnlyStruct () { Field = 1 };
		if (s.get_Field () != 1)
			return 2;
		Func<int> func = s.get_Field;
		return func ();
	}

	public static int test_1_entry_delegate_virtual_unbox () {
		var s = new InterpOnlyStruct () { Field = 1 };
		InterpOnlyIFace iface = s;
		Func<int> func = iface.get_Field2;
		return func ();
	}

	public static int test_2_invoke () {
		var res = typeof (InterpOnly).GetMethod ("entry_1").Invoke (null, new object [] { 1 });
		return (int)res;
	}
}
