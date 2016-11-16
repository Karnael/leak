﻿using System;

namespace Leak.Core.Common
{
    public class Bitfield
    {
        private readonly bool[] items;
        private int completed;

        public Bitfield(int length)
        {
            this.items = new bool[length];
            this.completed = 0;
        }

        public Bitfield(int length, Bitfield other)
        {
            this.items = new bool[length];
            this.completed = other.completed;

            Array.Copy(other.items, items, length);
        }

        public int Length
        {
            get { return items.Length; }
        }

        public int Completed
        {
            get { return completed; }
        }

        public bool this[int index]
        {
            get { return items[index]; }
            set
            {
                if (items[index] != value)
                {
                    items[index] = value;

                    if (value)
                        completed++;
                    else
                        completed--;
                }
            }
        }

        public override int GetHashCode()
        {
            return completed;
        }

        public override bool Equals(object obj)
        {
            Bitfield other = obj as Bitfield;

            return other != null && Equals(this, other);
        }

        private static bool Equals(Bitfield left, Bitfield right)
        {
            if (left.Length != right.Length)
                return false;

            if (left.completed != right.completed)
                return false;

            for (int i = 0; i < left.Length; i++)
                if (left[i] != right[i])
                    return false;

            return true;
        }

        public static Bitfield Random(byte length)
        {
            byte[] random = Bytes.Random(length);
            Bitfield result = new Bitfield(length);

            for (int i = 0; i < length; i++)
            {
                result[i] = random[i] % 2 == 0;
            }

            return result;
        }
    }
}