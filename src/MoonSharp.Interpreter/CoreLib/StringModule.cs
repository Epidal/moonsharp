﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.Patterns;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace="string")]
	public class StringModule
	{
		public static void MoonSharpInit(Table globalTable, Table stringTable)
		{
			Table stringMetatable = new Table(globalTable.OwnerScript);
			stringMetatable["__index"] = DynValue.NewTable(stringTable);
			globalTable.OwnerScript.SetTypeMetatable(DataType.String, stringMetatable);
		}

		[MoonSharpMethod]
		public static DynValue @byte(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "byte", DataType.String, false);
			DynValue vi = args.AsType(1, "byte", DataType.Number, true);
			DynValue vj = args.AsType(2, "byte", DataType.Number, true);

			return PerformByteLike(vs, vi, vj,
				i => Unicode2Ascii(i));
		}

		[MoonSharpMethod]
		public static DynValue unicode(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "unicode", DataType.String, false);
			DynValue vi = args.AsType(1, "unicode", DataType.Number, true);
			DynValue vj = args.AsType(2, "unicode", DataType.Number, true);

			return PerformByteLike(vs, vi, vj, i => i);
		}

		private static int Unicode2Ascii(int i)
		{
			if (i >= 0 && i < 255)
				return i;

			return (int)'?';
		}

		private static DynValue PerformByteLike(DynValue vs, DynValue vi, DynValue vj, Func<int, int> filter)
		{
			string s = vs.String;
            int conv_i = vi.IsNil() ? 1 : (int)vi.Number;
            int conv_j = vj.IsNil() ? conv_i : (int)vj.Number;

            StringRange range = StringRange.FromLuaRange(conv_i, conv_j);
            range.MapToString(vs.String);

            if ((range.Start >= vs.String.Length) || (range.End < range.Start))
            {
                return DynValue.NewString("");
            }

            int length = range.Length();
			DynValue[] rets = new DynValue[length];

            for (int i = 0; i < length; ++i)
            {
                rets[i] = DynValue.NewNumber(filter((int)s[range.Start + i]));
            }

			return DynValue.NewTuple(rets);
		}


		private static int? AdjustIndex(string s, DynValue vi, int defval)
		{
			if (vi.IsNil())
				return defval;

			int i = (int)Math.Round(vi.Number, 0);

			if (i == 0)
				return null;

			if (i > 0)
				return i - 1;

			return s.Length - i;
		}

		[MoonSharpMethod]
		public static DynValue len(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "len", DataType.String, false);
			return DynValue.NewNumber(vs.String.Length);
		}



		[MoonSharpMethod]
		public static DynValue match(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "match", DataType.String, false);
			DynValue p = args.AsType(1, "match", DataType.String, false);
			DynValue i = args.AsType(2, "match", DataType.Number, true);

			return PatternMatching.Match(s.String, p.String, i.IsNilOrNan() ? 1 : (int)i.Number);
		}


		[MoonSharpMethod()]
		public static DynValue gmatch(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gmatch", DataType.String, false);
			DynValue p = args.AsType(1, "gmatch", DataType.String, false);

			return PatternMatching.GMatch(executionContext.GetScript(), s.String, p.String);
		}

		[MoonSharpMethod()]
		public static DynValue gsub(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gsub", DataType.String, false);
			DynValue p = args.AsType(1, "gsub", DataType.String, false);
			DynValue v_i = args.AsType(3, "gsub", DataType.Number, true);
			int? i = v_i.IsNilOrNan() ? (int?)null : (int)v_i.Number;

			return PatternMatching.Str_Gsub(s.String, p.String, args[2], i);
		}

		[MoonSharpMethod()]
		public static DynValue find(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v_s = args.AsType(0, "find", DataType.String, false);
			DynValue v_p = args.AsType(1, "find", DataType.String, false);
			DynValue v_i = args.AsType(2, "find", DataType.Number, true);
			DynValue v_plain = args.AsType(3, "find", DataType.Boolean, true);

			int i = v_i.IsNilOrNan() ? int.MinValue : (int)v_i.Number;

			bool plain = v_plain.CastToBool();

			return PatternMatching.Str_Find(v_s.String, v_p.String, i, plain);
		}

        [MoonSharpMethod()]
        public static DynValue @char(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            StringBuilder result = new StringBuilder(args.Count);

            for (int i = 0; i < args.Count; ++i)
            {
                DynValue value = args.AsType(i, "char", DataType.Number, false);
                result.Append((char)value.Number);
            }

            return DynValue.NewString(result.ToString());
        }

        [MoonSharpMethod()]
        public static DynValue lower(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "lower", DataType.String, false);

            return DynValue.NewString(arg_s.String.ToLower());
        }

        [MoonSharpMethod()]
        public static DynValue upper(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "upper", DataType.String, false);

            return DynValue.NewString(arg_s.String.ToUpper());
        }

        [MoonSharpMethod()]
        public static DynValue rep(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "rep", DataType.String, false);
            DynValue arg_n = args.AsType(1, "rep", DataType.Number, false);

            if (String.IsNullOrEmpty(arg_s.String) || (arg_n.Number < 1))
            {
                return DynValue.NewString("");
            }

            int count = (int)arg_n.Number;
            StringBuilder result = new StringBuilder(arg_s.String.Length * count);

            for (int i = 0; i < count; ++i)
            {
                result.Append(arg_s.String);
            }

            return DynValue.NewString(result.ToString());
        }

        [MoonSharpMethod()]
        public static DynValue reverse(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "reverse", DataType.String, false);

            if (String.IsNullOrEmpty(arg_s.String))
            {
                return DynValue.NewString("");
            }

            char[] elements = arg_s.String.ToCharArray();
            Array.Reverse(elements);

            return DynValue.NewString(new String(elements));
        }

        [MoonSharpMethod()]
        public static DynValue sub(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "sub", DataType.String, false);
            DynValue arg_i = args.AsType(1, "sub", DataType.Number, false);
            DynValue arg_j = args.AsType(2, "sub", DataType.Number, true);

            int i = arg_i.IsNil() ? 1 : (int)arg_i.Number;
            int j = arg_j.IsNil() ? -1 : (int)arg_j.Number;

            StringRange range = StringRange.FromLuaRange(i, j);
            range.MapToString(arg_s.String);

            if ((range.Start >= arg_s.String.Length) || (range.End < range.Start))
            {
                return DynValue.NewString("");
            }

            return DynValue.NewString(arg_s.String.Substring(range.Start, range.Length()));
        }
	}

    public class StringRange
    {
        public int Start;
        public int End;

        public StringRange()
        {
            Start = 0;
            End = 0;
        }

        public StringRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public static StringRange FromLuaRange(int start, int end)
        {
            StringRange range = new StringRange();
            range.Start = (start > 0) ? start - 1 : start;
            range.End = (end > 0) ? end - 1 : end;

            return range;
        }

        public void MapToString(String value)
        {
            if (Start < 0)
            {
                Start = value.Length + Start;
            }

            if (Start < 0)
            {
                Start = 0;
            }

            if (End < 0)
            {
                End = value.Length + End;
            }

            if (End >= value.Length)
            {
                End = value.Length - 1;
            }
        }

        public int Length()
        {
            return (End - Start) + 1;
        }
    }
}