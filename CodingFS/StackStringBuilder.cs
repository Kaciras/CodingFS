// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// The original code is copied from:
// https://github.com/dotnet/runtime/blob/1da23b146496d95f017439a48f62e78ec15b0289/src/libraries/Common/src/System/Text/StackStringBuilder.cs

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CodingFS;

ref struct StackStringBuilder
{
	readonly Span<char> buffer;

	int position;

	public StackStringBuilder(Span<char> buffer)
	{
		this.buffer = buffer;
	}

	public int Length { readonly get => position; set { position = value; } }

	public override readonly string ToString()
	{
		return new string(buffer[..position]);
	}

	public void Insert(int index, char value, int count)
	{
		CheckCapacity(count);

		var remaining = position - index;
		position += count;
		buffer.Slice(index, remaining).CopyTo(buffer[(index + count)..]);
		buffer.Slice(index, count).Fill(value);
	}

	public void Insert(int index, string s)
	{
		var count = s.Length;
		CheckCapacity(count);

		var remaining = position - index;
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
			ThrowOutOfCapacity();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string value)
	{
		CheckCapacity(value.Length);
		value.AsSpan().CopyTo(buffer[position..]);
		position += value.Length;
	}

	public void Append(ReadOnlySpan<char> value)
	{
		CheckCapacity(value.Length);
		value.CopyTo(buffer[position..]);
		position += value.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		CheckCapacity(length);
		var p = position;
		position = p + length;
		return buffer.Slice(p, length);
	}

	public void AppendFormat(int value, string format)
	{
		var span = buffer[position..];
		if (value.TryFormat(span, out var count, format))
		{
			position += count;
		}
		else
		{
			ThrowOutOfCapacity();
		}
	}

	void CheckCapacity(int length)
	{
		if (position + length > buffer.Length) ThrowOutOfCapacity();
	}

	[DoesNotReturn]
	void ThrowOutOfCapacity()
	{
		throw new IndexOutOfRangeException($"Append chars out of the capacity(${buffer.Length})");
	}
}
