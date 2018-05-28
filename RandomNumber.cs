using System;
using System.Security.Cryptography;

namespace RosterLib
{
   /// <summary>
   /// This class generates random numbers using the
   /// System.Security.Cryptography.RandomNumberGenerator class
   /// </summary>
   public class RandomNumber
   {
      private RandomNumberGenerator rng;
      // private static long biggestNumber = 18446744073709551615;

      /// <summary>
      /// Constructor which creates the random number object
      /// </summary>
      public RandomNumber()
      {
         // Create the random number generator.
         rng = RandomNumberGenerator.Create();
      }

      /// <summary>
      /// This returns a random number of a particular size
      /// </summary>
      /// <param name="bits">Size of the random number in bits</param>
      /// <returns>Random number</returns>
      public long GetNumber(byte bits)
      {
         long randomNumber = 1;
         // Convert the number of bits to bytes.
         byte numBytes = Convert.ToByte(bits / 8);

         Byte[] ranbuff = new Byte[numBytes];

         // This retrieves a random sequence of bytes.
         rng.GetBytes(ranbuff);

         // Here we convert the random bytes to a number, using byte shifting.
         for (byte i = 0; i < numBytes; i++)
         {
            int randomByte = ranbuff[i];
            randomNumber = randomNumber + (randomByte *
               Convert.ToInt64(Math.Pow(2, (i * 8))));
         }

         // Return the generated number.
         return randomNumber;
      }

      /// <summary>
      /// Generates an 8 bit random number
      /// </summary>
      /// <returns>Random number</returns>
      public long GetNumber()
      {
         return GetNumber(8);
      }

      /// <summary>
      /// Returns a random number within a specific range min <= number < max.
      /// </summary>
      /// <param name="min">minimum number of the range</param>
      /// <param name="max">maximum number of the range</param>
      /// <returns>random number</returns>
      public long GetNumberInRange( long min, long max )
      {
         byte bits = 32;

         long randomNumber = GetNumber(bits);
         long biggestNumber = Convert.ToInt64(Math.Pow(2, bits));

         // Takes the generated number and puts it in the range
         // that has been asked for.
         long randomNumberInRange =
            Convert.ToInt64( Math.Floor((Convert.ToDouble(randomNumber) /
            Convert.ToDouble(biggestNumber) * Convert.ToDouble(max - min)) +
            Convert.ToDouble(min)));

         return randomNumberInRange;
      }

      /// <summary>
      /// Returns a random number within a specific range
      /// min less than or equal to number less than max
      /// </summary>
      /// <param name="min">minimum number of the range</param>
      /// <param name="max">maximum number of the range</param>
      /// <returns>random number</returns>
      public int GetNumberInRange( int min, int max)
      {
         return Convert.ToInt32( GetNumberInRange( Convert.ToInt64(min),
            Convert.ToInt64(max)));
      }
   }
}
