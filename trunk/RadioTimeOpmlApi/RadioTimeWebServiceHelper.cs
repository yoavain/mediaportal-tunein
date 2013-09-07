using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RadioTimeOpmlApi
{
  public static class RadioTimeWebServiceHelper
  {
    public static string HashMD5(string ToHash)
    {
      // First we need to convert the string into bytes,

      // which means using a text encoder.

      Encoder enc = System.Text.Encoding.ASCII.GetEncoder();

      // Create a buffer large enough to hold the string

      byte[] data = new byte[ToHash.Length];
      enc.GetBytes(ToHash.ToCharArray(), 0, ToHash.Length, data, 0, true);

      // This is one implementation of the abstract class MD5.

      MD5 md5 = new MD5CryptoServiceProvider();
      byte[] result = md5.ComputeHash(data);

      return BitConverter.ToString(result).Replace("-", "").ToLower();
    }
  }
}
