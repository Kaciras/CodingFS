// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// The original code is copied from:
// https://github.com/dotnet/runtime/blob/1da23b146496d95f017439a48f62e78ec15b0289/src/libraries/Common/src/System/Text/StackStringBuilder.cs

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodingFS;

ref struct StackStringBuilder
{
	readonly Span<char> _chars;

	private int _pos;

	public StackStringBuilder(Span<char> buffer)
	{
		_pos = 0;
		_chars = buffer;
	}

	public int Length
	{
		readonly get => _pos;
		set { _pos = value; }
	}

	public readonly int Capacity => _chars.Length;

	/// <summary>
	/// Get a pinnable reference to the builder.
	/// Does not ensure there is a null char after <see cref="Length"/>
	/// This overload is pattern matched in the C# 7.3+ compiler so you can omit
	/// the explicit method call, and write eg "fixed (char* c = builder)"
	/// </summary>
	public readonly ref char GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(_chars);
	}

	public readonly ref char this[int index] => ref _chars[index];

	public override readonly string ToString()=> _chars[.._pos].ToString();

	public readonly ReadOnlySpan<char> AsSpan() => _chars[.._pos];
	public readonly ReadOnlySpan<char> AsSpan(int start) => _chars[start.._pos];
	public readonly ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

	public void Insert(int index, char value, int count)
	{
		if (_pos > _chars.Length - count)
		{
			OutOfCapacity();
		}
		int remaining = _pos - index;
		_pos += count;
		_chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
		_chars.Slice(index, count).Fill(value);
	}

	public void Insert(int index, string s)
	{
		int count = s.Length;

		if (_pos > (_chars.Length - count))
		{
			OutOfCapacity();
		}

		int remaining = _pos - index;
		_pos += count;
		_chars.Slice(index, remaining).CopyTo(_chars[(index + count)..]);
		s.AsSpan().CopyTo(_chars[index..]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		var pos = _pos;
		var chars = _chars;
		if ((uint)pos < (uint)chars.Length)
		{
			chars[pos] = c;
			_pos = pos + 1;
		}
		else
		{
			OutOfCapacity();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string s)
	{
		int pos = _pos;
		if (pos > _chars.Length - s.Length)
		{
			OutOfCapacity();
		}

		_pos += s.Length;
		s.AsSpan().CopyTo(_chars[pos..]);
	}

	public void Append(ReadOnlySpan<char> value)
	{
		int pos = _pos;
		if (pos > _chars.Length - value.Length)
		{
			OutOfCapacity();
		}

		_pos += value.Length;
		value.CopyTo(_chars[_pos..]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int origPos = _pos;
		if (origPos > _chars.Length - length)
		{
			OutOfCapacity();
		}

		_pos = origPos + length;
		return _chars.Slice(origPos, length);
	}

	[DoesNotReturn]
	static void OutOfCapacity()
	{
		throw new IndexOutOfRangeException();
	}
}
