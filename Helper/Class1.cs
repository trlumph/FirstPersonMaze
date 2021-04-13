using System;

namespace VectorHelper
{
    public class MyVector
    {
        public float X;
        public float Y;

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public float LengthSquared => X * X + Y * Y;

        public MyVector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public MyVector()
        {

        }

        public MyVector(MyVector vector)
        {
            X = vector.X;
            Y = vector.Y;
        }

        public void Normalize()
        {
            if (Length == 0.0f)
                return;
            X /= Length;
            Y /= Length;
        }

        public static MyVector operator +(MyVector v1, MyVector v2)
        {
            return new MyVector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static MyVector operator *(MyVector v1, MyVector v2)
        {
            return new MyVector(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static MyVector operator -(MyVector v1, MyVector v2)
        {
            return new MyVector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static implicit operator MyVector(float number)
        {
            return new MyVector(number, number);
        }
    }
}