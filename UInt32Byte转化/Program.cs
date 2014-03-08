using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace UInt32Byte转化
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            uint input = 0X01122334;
            byte part4 = 0;
            byte part3 = 0;
            byte part2 = 0;
            byte part1 = 0;

            int loopCount = int.MaxValue / 8;

            for (int i = 0; i < loopCount; i++)//随便跑跑，减小首次运行造成的影响
            {
                part4 = (byte)input;
                part3 = (byte)(input >> 8);
                part2 = (byte)(input >> 16);
                part1 = (byte)(input >> 24);
            }

            RunTest(0, 1, input, (ipt) =>//演示用，只循环一次
            {
                var temp = ipt.ToString("X8");
                return Tuple.Create(//忽略不计做法，转成16进制字符串再两两拆开再转换回byte
                    byte.Parse(temp.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(temp.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(temp.Substring(4, 2), NumberStyles.HexNumber),
                    byte.Parse(temp.Substring(6, 2), NumberStyles.HexNumber));
            });
            RunTest(1, loopCount, input, (ipt) =>
            {
                return Tuple.Create(//普通做法，通过位移获取每个byte值
                    (byte)(ipt >> 24),
                    (byte)(ipt >> 16),
                    (byte)(ipt >> 8),
                    (byte)(ipt)
                );
            });
            RunTest(2, loopCount, input, (ipt) =>
            {
                unsafe
                {
                    byte* p = (byte*)&ipt;
                    return Tuple.Create(//文艺做法，通过指针获取每个byte值。由于X86采用Little Ending存储，在其他平台上是否有问题还待测试。下一个测试也有同一个问题。
                        p[3],
                        p[2],
                        p[1],
                        p[0]
                    );
                }
            });
            UIntByteConverter converter = new UIntByteConverter();//如果多了一次对象创建，速度自然慢了许多
            RunTest(3, loopCount, input, (ipt) =>
            {
                converter.Source = ipt;
                return Tuple.Create(//二逼做法，通过创建共用体（union）直接获取值
                    converter.part1,
                    converter.part2,
                    converter.part3,
                    converter.part4
                );
            });
        }

        private static void RunTest(object testId, int loopCount, UInt32 input, Func<UInt32, Tuple<byte, byte, byte, byte>> test)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte part1 = 0;
            byte part2 = 0;
            byte part3 = 0;
            byte part4 = 0;
            for (int i = 0; i < loopCount; i++)
            {
                var result = test(input);
                part1 = result.Item1;
                part2 = result.Item2;
                part3 = result.Item3;
                part4 = result.Item4;
            }
            sw.Stop();
            Console.WriteLine("测试" + testId + "：转换后值为0X{0:x2}{1:x2}{2:x2}{3:x2}，耗时{4}毫秒", part1, part2, part3, part4, sw.ElapsedMilliseconds);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntByteConverter
    {
        [FieldOffset(0)]
        public uint Source;

        [FieldOffset(0)]
        public byte part4;

        [FieldOffset(1)]
        public byte part3;

        [FieldOffset(2)]
        public byte part2;

        [FieldOffset(3)]
        public byte part1;
    }
}