using System.Buffers;

namespace Jumoo.Json;

/// <summary>
///  An <see cref="IBufferWriter{T}"/> backed by <see cref="ArrayPool{T}"/>.
/// </summary>
/// <remarks>
/// <para>
///  Used where we need the serialized form of something but not a string of it - comparisons,
///  mostly. Serializing a 200kb object to a string put a 400kb allocation straight onto the
///  large object heap; renting a buffer keeps it off the heap entirely.
/// </para>
/// <para>
///  Not thread safe, and the rented buffer goes back to the pool on <see cref="Dispose"/> -
///  so <see cref="WrittenSpan"/> must not be read after disposing.
/// </para>
/// </remarks>
internal sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _written;

    public PooledByteBufferWriter(int initialCapacity = 4096)
        => _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);

    /// <summary>
    ///  What has been written so far. Only valid until <see cref="Dispose"/>.
    /// </summary>
    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);

    public void Advance(int count) => _written += count;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_written);
    }

    private void EnsureCapacity(int sizeHint)
    {
        if (sizeHint < 1) sizeHint = 1;
        if (_buffer.Length - _written >= sizeHint) return;

        var replacement = ArrayPool<byte>.Shared.Rent(
            Math.Max(_buffer.Length * 2, _written + sizeHint));

        _buffer.AsSpan(0, _written).CopyTo(replacement);
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = replacement;
    }

    public void Dispose()
    {
        if (_buffer.Length is 0) return;

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = [];
        _written = 0;
    }
}
