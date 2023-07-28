// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// The original code is copied from:
// https://github.com/dotnet/runtime/blob/1da23b146496d95f017439a48f62e78ec15b0289/src/libraries/Common/src/System/Text/ValueStringBuilder.cs

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodingFS;

ref struct ValueStringBuilder
{
	private Span<char> _chars;
	private int _pos;

	public ValueStringBuilder(Span<char> buffer)
	{
		_pos = 0;
		_chars = buffer;
	}

	public int Length
	{
		get => _pos;
		set { _pos = value; }
	}

	public int Capacity => _chars.Length;

	/// <summary>
	/// Get a pinnable reference to the builder.
	/// Does not ensure there is a null char after <see cref="Length"/>
	/// This overload is pattern matched in the C# 7.3+ compiler so you can omit
	/// the explicit method call, and write eg "fixed (char* c = builder)"
	/// </summary>
	public ref char GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(_chars);
	}

	public ref char this[int index] => ref _chars[index];

	public override string ToString()
	{
		return _chars.Slice(0, _pos).ToString();
	}

	/// <summary>Returns the underlying storage of the builder.</summary>
	public Span<char> RawChars => _chars;

	public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);

	public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);

	public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

	public bool TryCopyTo(Span<char> destination, out int charsWritten)
	{
		if (_chars[.._pos].TryCopyTo(destination))
		{
			charsWritten = _pos;
			return true;
		}
		else
		{
			charsWritten = 0;
			return false;
		}
	}

	public void Insert(int index, char value, int count)
	{
		if (_pos > _chars.Length - count)
		{
			OutOfCapacity();
		}

		int remaining = _pos - index;
		_pos += count;
		_chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
		_chars.Slice(index, count).Fill(value);
	}

	public void Insert(int index, string? s)
	{
		if (s == null)
		{
			return;
		}

		int count = s.Length;

		if (_pos > (_chars.Length - count))
		{
			OutOfCapacity();
		}

		int remaining = _pos - index;
		_pos += count;
		_chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
		s.AsSpan().CopyTo(_chars.Slice(index));
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
	public void Append(string? s)
	{
		if (s == null)
		{
			return;
		}

		int pos = _pos;

		// very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
		if (s.Length == 1 && (uint)pos < (uint)_chars.Length)
		{
			_chars[pos] = s[0];
			_pos = pos + 1;
		}
		else
		{
			AppendSlow(s);
		}
	}

	private void AppendSlow(string s)
	{
		int pos = _pos;
		if (pos > _chars.Length - s.Length)
		{
			OutOfCapacity();
		}

		_pos += s.Length;
		s.AsSpan().CopyTo(_chars.Slice(pos));
	}

	public void Append(char c, int count)
	{
		if (_pos > _chars.Length - count)
		{
			OutOfCapacity();
		}

		var dst = _chars.Slice(_pos, count);
		for (int i = 0; i < dst.Length; i++)
		{
			dst[i] = c;
		}
		_pos += count;
	}

	public unsafe void Append(char* value, int length)
	{
		int pos = _pos;
		if (pos > _chars.Length - length)
		{
			OutOfCapacity();
		}

		var dst = _chars.Slice(_pos, length);
		for (int i = 0; i < dst.Length; i++)
		{
			dst[i] = *value++;
		}
		_pos += length;
	}

	public void Append(ReadOnlySpan<char> value)
	{
		int pos = _pos;
		if (pos > _chars.Length - value.Length)
		{
			OutOfCapacity();
		}

		value.CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
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
