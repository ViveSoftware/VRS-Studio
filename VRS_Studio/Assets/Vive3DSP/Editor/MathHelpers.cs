/**
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014, Unity Technologies
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace MathHelpers
{
    // System.Numerics is not available
    public class ComplexD
    {
        public double real;
        public double imag;

        public ComplexD(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }

        static public ComplexD Add(ComplexD a, ComplexD b)
        {
            return new ComplexD(
                a.real + b.real,
                a.imag + b.imag);
        }

        static public ComplexD Add(ComplexD a, double b)
        {
            return new ComplexD(
                a.real + b,
                a.imag);
        }

        static public ComplexD Add(double a, ComplexD b)
        {
            return new ComplexD(
                a + b.real,
                b.imag);
        }

        static public ComplexD Sub(ComplexD a, ComplexD b)
        {
            return new ComplexD(
                a.real - b.real,
                a.imag - b.imag);
        }

        static public ComplexD Sub(ComplexD a, double b)
        {
            return new ComplexD(
                a.real - b,
                a.imag);
        }

        static public ComplexD Sub(double a, ComplexD b)
        {
            return new ComplexD(
                a - b.real,
                -b.imag);
        }

        static public ComplexD Mul(ComplexD a, ComplexD b)
        {
            return new ComplexD(
                a.real * b.real - a.imag * b.imag,
                a.real * b.imag + a.imag * b.real);
        }

        static public ComplexD Mul(ComplexD a, double b)
        {
            return new ComplexD(
                a.real * b,
                a.imag * b);
        }

        static public ComplexD Mul(double a, ComplexD b)
        {
            return new ComplexD(
                a * b.real,
                a * b.imag);
        }

        static public ComplexD Div(ComplexD a, ComplexD b)
        {
            double d = b.real * b.real + b.imag * b.imag;
            double s = 1.0 / d;
            return new ComplexD(
                (a.real * b.real + a.imag * b.imag) * s,
                (a.imag * b.real - a.real * b.imag) * s);
        }

        static public ComplexD Div(double a, ComplexD b)
        {
            double d = b.real * b.real + b.imag * b.imag;
            double s = a / d;
            return new ComplexD(
                s * b.real,
                -s * b.imag);
        }

        static public ComplexD Div(ComplexD a, double b)
        {
            double s = 1.0 / b;
            return new ComplexD(
                a.real * s,
                a.imag * s);
        }

        static public ComplexD Exp(double omega)
        {
            return new ComplexD(
                Math.Cos(omega),
                Math.Sin(omega));
        }

        static public ComplexD Pow(ComplexD a, double b)
        {
            double p = Math.Atan2(a.imag, a.real);
            double m = Math.Pow(a.Mag2(), b * 0.5f);
            return new ComplexD(
                m * Math.Cos(p * b),
                m * Math.Sin(p * b)
                );
        }

        public double Mag2() { return real * real + imag * imag; }
        public double Mag() { return Math.Sqrt(Mag2()); }

        public static ComplexD operator+(ComplexD a, ComplexD b) { return Add(a, b); }
        public static ComplexD operator-(ComplexD a, ComplexD b) { return Sub(a, b); }
        public static ComplexD operator*(ComplexD a, ComplexD b) { return Mul(a, b); }
        public static ComplexD operator/(ComplexD a, ComplexD b) { return Div(a, b); }
        public static ComplexD operator+(ComplexD a, double b) { return Add(a, b); }
        public static ComplexD operator-(ComplexD a, double b) { return Sub(a, b); }
        public static ComplexD operator*(ComplexD a, double b) { return Mul(a, b); }
        public static ComplexD operator/(ComplexD a, double b) { return Div(a, b); }
        public static ComplexD operator+(double a, ComplexD b) { return Add(a, b); }
        public static ComplexD operator-(double a, ComplexD b) { return Sub(a, b); }
        public static ComplexD operator*(double a, ComplexD b) { return Mul(a, b); }
        public static ComplexD operator/(double a, ComplexD b) { return Div(a, b); }
    }
}
