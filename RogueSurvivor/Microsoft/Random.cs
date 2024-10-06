// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
//
//   forked 2019-09-29 from https://referencesource.microsoft.com/#mscorlib/system/random.cs,bb77e610694e64ca
//   in response to intentional exclusion of this type from binary savefiles for .NET Core 3.0 publicly documented at
//   https://docs.microsoft.com/en-us/dotnet/standard/serialization/binary-serialization
//
//   The rationale for exclusion is documented at https://github.com/dotnet/corefx/issues/19119 .

/* We brought[Serializable] and BinaryFormatter back for .NET Core 2.0 in the name of compat and helping code to migrate to core.
   But it comes with some serious implications, and it's becoming clear we took things too far. Doing this makes it very easy for code
   to take dependencies on private implementation details and will end up binding our hands for improvements/optimizations/refactorings/etc.
   in the future. And with all of the improvements/optimizations/refactorings/etc. already made in core, we've had to punt
   on the idea of cross-runtime serialization support. */

//   Based on response to https://github.com/dotnet/coreclr/issues/17460 , a request to revert this change will be rejected even with
//   a concrete use case.
//
//   This particular file is not obviously covered by the GPLv3.  Algorithm source reported to be
//   Numerical Recipes in C (2nd Ed.).
//
// ==--==
/*============================================================
**
** Class:  Random
**
**
** Purpose: A random number generator.
**
**
===========================================================*/
using System;
using System.Text.Json;
using Zaimoni.Serialization;

namespace Microsoft
{
    [Serializable]
    public class Random : ISerialize
    {
        // Private Constants
        private const int MBIG = Int32.MaxValue;
        private const int MSEED = 161803398;
        private const int MZ = 0;
        private const int BUFFER_LEN = 56;

        // Member Variables
        private int inext;
        private int inextp;
        private readonly int[] SeedArray = new int[BUFFER_LEN];

        // Constructors
        public Random() : this(Environment.TickCount) { }

        private Random(int ext, int extp, int[] state) {
            inext = ext;
            inextp = extp;
            SeedArray = state;
        }

    public Random(int Seed)
        {
            int ii;
            int mj, mk;

            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = (Seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(Seed);
            mj = MSEED - subtraction;
            SeedArray[BUFFER_LEN-1] = mj;
            mk = 1;
            for (int i = 1; i < BUFFER_LEN - 1; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = (21 * i) % (BUFFER_LEN - 1);
                SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += MBIG;
                mj = SeedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < BUFFER_LEN; i++)
                {
                    SeedArray[i] -= SeedArray[1 + (i + 30) % (BUFFER_LEN - 1)];
                    if (SeedArray[i] < 0) SeedArray[i] += MBIG;
                }
            }
            inext = 0;
            inextp = 21;
            Seed = 1;
        }

        public Random(DecodeObjects decode)
        {
            Formatter.Deserialize(decode.src, ref inext);
            Formatter.Deserialize(decode.src, ref inextp);
            decode.LoadFrom7bit(ref SeedArray);
        }

        public void save(EncodeObjects encode)
        {
            Formatter.Serialize(encode.dest, inext);
            Formatter.Serialize(encode.dest, inextp);
            encode.SaveTo7bit(SeedArray);
        }

        static private int field_code(ref Utf8JsonReader reader)
        {
            if (reader.ValueTextEquals("inext")) return 1;
            else if (reader.ValueTextEquals("inextp")) return 2;
            else if (reader.ValueTextEquals("SeedArray")) return 3;
            else throw new JsonException();
        }

        public static Random fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options) {
            if (JsonTokenType.StartObject != reader.TokenType) throw new JsonException();
            reader.Read();

            int stage_inext = default;
            int stage_inextp = default;
            int[] stage_SeedArray = default;

            if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

            int stage = field_code(ref reader);

            reader.Read();

            switch (stage)
            {
                case 1:
                    stage_inext = reader.GetInt32();
                    break;
                case 2:
                    stage_inextp = reader.GetInt32();
                    break;
                default:
                    stage_SeedArray = JsonSerializer.Deserialize<int[]>(ref reader, djack.RogueSurvivor.Engine.Session.JSON_opts);
                    break;
            }

            reader.Read();

            if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

            stage = field_code(ref reader);

            reader.Read();

            switch (stage)
            {
                case 1:
                    stage_inext = reader.GetInt32();
                    break;
                case 2:
                    stage_inextp = reader.GetInt32();
                    break;
                default:
                    stage_SeedArray = JsonSerializer.Deserialize<int[]>(ref reader, djack.RogueSurvivor.Engine.Session.JSON_opts);
                    break;
            }

            reader.Read();

            if (JsonTokenType.PropertyName != reader.TokenType) throw new JsonException();

            stage = field_code(ref reader);

            reader.Read();

            switch (stage)
            {
                case 1:
                    stage_inext = reader.GetInt32();
                    break;
                case 2:
                    stage_inextp = reader.GetInt32();
                    break;
                default:
                    stage_SeedArray = JsonSerializer.Deserialize<int[]>(ref reader, djack.RogueSurvivor.Engine.Session.JSON_opts);
                    break;
            }

            return new Random(stage_inext, stage_inextp, stage_SeedArray);
        }

        public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("inext", inext);
            writer.WriteNumber("inextp", inextp);
            writer.WritePropertyName("SeedArray");
            JsonSerializer.Serialize(writer, SeedArray, djack.RogueSurvivor.Engine.Session.JSON_opts);
            writer.WriteEndObject();
        }

        //
        // Package Private Methods
        //

        /*====================================Sample====================================
        **Action: Return a new random number [0..1) and reSeed the Seed array.
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        protected virtual double Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return (InternalSample() * (1.0 / MBIG));
        }

        private int InternalSample()
        {
            int retVal;
            int locINext = inext;
            int locINextp = inextp;

            if (++locINext >= BUFFER_LEN) locINext = 1;
            if (++locINextp >= BUFFER_LEN) locINextp = 1;

            retVal = SeedArray[locINext] - SeedArray[locINextp];

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            SeedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        //
        // Public Instance Methods
        //

        /*=====================================Next=====================================
        **Returns: An int [0..Int32.MaxValue)
        **Arguments: None
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next()
        {
            return InternalSample();
        }

        private double GetSampleForLargeRange()
        {
            // The distribution of double value returned by Sample
            // is not distributed well enough for a large range.
            // If we use Sample for a range [Int32.MinValue..Int32.MaxValue)
            // We will end up getting even numbers only.

            int result = InternalSample();
            // Note we can't use addition here. The distribution will be bad if we do that.
            bool negative = (InternalSample() % 2 == 0) ? true : false;  // decide the sign based on second sample
            if (negative)
            {
                result = -result;
            }
            double d = result;
            d += (Int32.MaxValue - 1); // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (uint)Int32.MaxValue - 1;
            return d;
        }

        /*=====================================Next=====================================
        **Returns: An int [minvalue..maxvalue)
        **Arguments: minValue -- the least legal value for the Random number.
        **           maxValue -- One greater than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue)+" > "+nameof(maxValue)+": "+minValue+" > "+maxValue);

            long range = (long)maxValue - minValue;
            if (range <= (long)int.MaxValue)
            {
                return ((int)(Sample() * range) + minValue);
            }
            else
            {
                return (int)((long)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        /*=====================================Next=====================================
        **Returns: An int [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next(int maxValue)
        {
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue)+" < 0: "+maxValue);
            return (int)(Sample() * maxValue);
        }

        /*=====================================Next=====================================
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        public virtual double NextDouble()
        {
            return Sample();
        }

        /*==================================NextBytes===================================
        **Action:  Fills the byte array with random bytes [0..0x7f].  The entire array is filled.
        **Returns:Void
        **Arugments:  buffer -- the array to be filled.
        **Exceptions: None
        ==============================================================================*/
        public virtual void NextBytes(byte[] buffer)
        {
            if (null == buffer) throw new ArgumentNullException(nameof(buffer));
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % (byte.MaxValue + 1));
            }
        }
    }
}

namespace Zaimoni.JsonConvert
{
    public class Random : System.Text.Json.Serialization.JsonConverter<Microsoft.Random>
    {
        public override Microsoft.Random Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return Microsoft.Random.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, Microsoft.Random src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
