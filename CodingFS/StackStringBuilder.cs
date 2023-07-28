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
	readonly Span<char> buffer;

	int position;

	public StackStringBuilder(Span<char> buffer)
	{
		this.buffer = buffer;
	}

	public readonly int Capacity => buffer.Length;

	public int Length
	{
		readonly get => position;
		set { position = value; }
	}

	/// <summary>
	/// Get a pinnable reference to the builder.
	/// Does not ensure there is a null char after <see cref="Length"/>
	/// This overload is pattern matched in the C# 7.3+ compiler so you can omit
	/// the explicit method call, and write eg "fixed (char* c = builder)"
	/// </summary>
	public readonly ref char GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(buffer);
	}

	public readonly ref char this[int index] => ref buffer[index];

	public override readonly string ToString()=> buffer[..position].ToString();

	public readonly ReadOnlySpan<char> AsSpan() => buffer[..position];
	public readonly ReadOnlySpan<char> AsSpan(int start) => buffer[start..position];
	public readonly ReadOnlySpan<char> AsSpan(int start, int length) => buffer.Slice(start, length);

	public void Insert(int index, char value, int count)
	{
		if (position > buffer.Length - count)
		{
			OutOfCapacity();
		}
		int remaining = position - index;
		position += count;
		buffer.Slice(index, remaining).CopyTo(buffer[(index + count)..]);
		buffer.Slice(index, count).Fill(value);
	}

	public void Insert(int index, string s)
	{
		int count = s.Length;

		if (position > buffer.Length - count)
		{
			OutOfCapacity();
		}

		int remaining = position - index;
		position += count;
		buffer.Slice(index, remaining).CopyTo(buffer[(index + count)..]);
		s.AsSpan().CopyTo(buffer[index..]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		var chars = buffer;
		var p = position;

		if (p < chars.Length)
		{
			chars[p] = c;
			position = p + 1;
		}
		else
		{
			OutOfCapacity();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string value)
	{
		int p = position;
		if (p > buffer.Length - value.Length)
		{
			OutOfCapacity();
		}

		position += value.Length;
		value.AsSpan().CopyTo(buffer[p..]);
	}

	public void Append(ReadOnlySpan<char> value)
	{
		int p = position;
		if (p > buffer.Length - value.Length)
		{
			OutOfCapacity();
		}

		position += value.Length;
		value.CopyTo(buffer[p..]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int p = position;
		if (p > buffer.Length - length)
		{
			OutOfCapacity();
		}

		position = p + length;
		return buffer.Slice(p, length);
	}

	[DoesNotReturn]
	static void OutOfCapacity()
	{
		throw new IndexOutOfRangeException();
	}
}
