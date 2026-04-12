using CodyNET.Common;
using NUnit.Framework;

namespace CodyNET.Tests.Component;

public class RingBufferTests
{
    [Test]
    public void DefaultCapacityIsEight()
    {
        var buf = new RingBuffer();
        Assert.AreEqual(8, buf.Capacity);
    }

    [Test]
    public void CustomCapacityIsStored()
    {
        var buf = new RingBuffer(16);
        Assert.AreEqual(16, buf.Capacity);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(256)]
    public void InvalidCapacityThrows(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new RingBuffer(capacity));
    }

    [Test]
    public void NewBufferIsEmpty()
    {
        var buf = new RingBuffer();
        Assert.IsTrue(buf.IsEmpty);
        Assert.IsFalse(buf.IsFull);
        Assert.AreEqual(0, buf.Length);
    }

    [Test]
    public void PushAndPopSingleByte()
    {
        var buf = new RingBuffer();
        Assert.IsTrue(buf.Push(0xAB));
        Assert.IsFalse(buf.IsEmpty);
        Assert.AreEqual(0xAB, buf.Pop());
        Assert.IsTrue(buf.IsEmpty);
    }

    [Test]
    public void PopOnEmptyBufferReturnsNull()
    {
        var buf = new RingBuffer();
        Assert.IsNull(buf.Pop());
    }

    [Test]
    public void PushOnFullBufferReturnsFalse()
    {
        var buf = new RingBuffer(4);
        // capacity=4 means 3 items max (head+1==tail when full)
        buf.Push(0x01);
        buf.Push(0x02);
        buf.Push(0x03);
        Assert.IsTrue(buf.IsFull);
        Assert.IsFalse(buf.Push(0x04));
    }

    [Test]
    public void FifoOrder()
    {
        var buf = new RingBuffer();
        buf.Push(0x01);
        buf.Push(0x02);
        buf.Push(0x03);
        Assert.AreEqual(0x01, buf.Pop());
        Assert.AreEqual(0x02, buf.Pop());
        Assert.AreEqual(0x03, buf.Pop());
    }

    [Test]
    public void LengthTracksContents()
    {
        var buf = new RingBuffer();
        Assert.AreEqual(0, buf.Length);
        buf.Push(0xAA);
        Assert.AreEqual(1, buf.Length);
        buf.Push(0xBB);
        Assert.AreEqual(2, buf.Length);
        buf.Pop();
        Assert.AreEqual(1, buf.Length);
    }

    [Test]
    public void WrapAroundBehavior()
    {
        var buf = new RingBuffer(4);
        buf.Push(0x01);
        buf.Push(0x02);
        buf.Push(0x03);
        buf.Pop(); // remove 0x01, tail wraps
        buf.Push(0x04); // head wraps
        Assert.AreEqual(0x02, buf.Pop());
        Assert.AreEqual(0x03, buf.Pop());
        Assert.AreEqual(0x04, buf.Pop());
        Assert.IsTrue(buf.IsEmpty);
    }

    [Test]
    public void ResetClearsBuffer()
    {
        var buf = new RingBuffer();
        buf.Push(0x42);
        buf.Push(0x43);
        buf.Reset();
        Assert.IsTrue(buf.IsEmpty);
        Assert.AreEqual(0, buf.Length);
        Assert.AreEqual(0, buf.Head);
        Assert.AreEqual(0, buf.Tail);
    }

    [Test]
    public void GetSetDirectAccess()
    {
        var buf = new RingBuffer();
        buf.Set(0, 0xDE);
        buf.Set(1, 0xAD);
        Assert.AreEqual(0xDE, buf.Get(0));
        Assert.AreEqual(0xAD, buf.Get(1));
    }

    [Test]
    public void HeadAndTailWrapOnSet()
    {
        var buf = new RingBuffer(8);
        buf.Head = 10; // 10 % 8 = 2
        buf.Tail = 9;  // 9  % 8 = 1
        Assert.AreEqual(2, buf.Head);
        Assert.AreEqual(1, buf.Tail);
    }

    [Test]
    public void MaximumCapacity()
    {
        var buf = new RingBuffer(255);
        Assert.AreEqual(255, buf.Capacity);
    }
}
