/*
 *  MyRandom.cs
 *      再現性のある乱数の出力
 * 
 *  https://kan-kikuchi.hatenablog.com/entry/Random_Seed
 * 
 * 
 * 
 * 
 *  20221211    3日前くらいにぶっちゃけコピペ
 * 
 */

using System;

public class MyRandom
{
    private uint x, y, z, w;

    public MyRandom() : this((uint)DateTime.Now.Ticks) { }

    public MyRandom(uint seed)
    {
        setSeed(seed);
    }

    public void setSeed(uint seed)
    {
        x = seed; y = x * 3266489917U + 1; z = y * 3266489917U + 1; w = z * 3266489917U + 1;
    }

    public uint getNext()
    {
        uint t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;
        w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
        return w;
    }

    public int Range(int min, int max)
    {
        return min + Math.Abs((int)getNext()) % (max - 1);
    }

}