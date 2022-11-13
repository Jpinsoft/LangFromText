using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jpinsoft.LangTainer.Utils
{
    public static class ArrayTools
    {
        public static string FirstUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder sb = new StringBuilder(input);

            sb.Insert(0, char.ToUpper(sb[0]));
            sb.Remove(1, 1);

            return sb.ToString();
        }

        /// <summary>
        /// Nahradi prvy vyskyt pola oldValue za newValue v poli sourceArray
        /// </summary>
        public static int Replace(ref byte[] sourceArray, byte[] oldValue, byte[] newValue)
        {
            int pos = FindSubArrayInArray(sourceArray, oldValue);

            if (pos == -1)
                return pos;

            byte[] temp = new byte[sourceArray.Length + (newValue.Length - oldValue.Length)];

            Array.Copy(sourceArray, temp, pos);
            Array.Copy(newValue, 0, temp, pos, newValue.Length);
            Array.Copy(sourceArray, pos + oldValue.Length, temp, pos + newValue.Length, sourceArray.Length - pos - oldValue.Length);

            sourceArray = temp;

            return pos;

        }

        /// <summary>
        /// Vrati prvy vyskyt searched array v sourcearray
        /// </summary>
        public static int FindSubArrayInArray(Array sourceArray, Array searchedArray)
        {
            Array tempArray = Array.CreateInstance(sourceArray.GetType().GetElementType(), searchedArray.Length);

            for (int i = 0; i < sourceArray.Length; i++)
            {
                // Zhoda prveho bajtu
                if (sourceArray.GetValue(i) == searchedArray.GetValue(0))
                {
                    if (sourceArray.Length - i < searchedArray.Length)
                        return -1;

                    Array.Copy(sourceArray, i, tempArray, 0, searchedArray.Length);

                    if (CompareArrays(tempArray, searchedArray))
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Vrati pocet elementov z containsThisElements, ktore sa nachadzaju v sourceArray bez ohladu na ich poradie.
        /// </summary>
        public static int ContainsElements(Array sourceArray, Array containsThisElements)
        {
            int res = 0;

            foreach (object elem in containsThisElements)
            {
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    if (sourceArray.GetValue(i) == elem)
                    {
                        res++;
                        break;
                    }
                }

            }

            return res;
        }

        /// <summary>
        /// Vstupné polia musia mat rovnaku velkost
        /// 18.11.2017 BUG: pouzival sa operator !=, ktory nespravne porovnaval Value types. Teraz sa pouziva Equals
        /// </summary>
        public static bool CompareArrays(Array array1, Array array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1.GetValue(i).Equals(array2.GetValue(i)))
                    return false;
            }

            return true;
        }

        public static string RemoveDiacritics(this string s)
        {
            string normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Vrati zo stringu, ktory reprezentuje binarne cislo cislo typu byte. Pokial konvertovanie zlyha je vratene cislo 0.
        /// </summary>
        public static byte BinaryStringToByte(string binaryString)
        {
            byte flag = 0;

            try { flag = Convert.ToByte(binaryString, 2); }
            catch { }

            return flag;
        }

        /// <summary>
        /// Metoda vrati ciselne hodnoty pola bajtov ako string v hexa tvare. Kodovanie sa nepouziva.
        /// </summary>
        public static string ByteArrayToHexaString(byte[] byteArray)
        {
            string stringData = "";

            foreach (byte item in byteArray)
                stringData += item.ToString("X2");

            return stringData;
        }

        /// <summary>
        /// Metoda vrati ciselne hodnoty pola bajtov ako string. Kodovanie sa nepouziva.
        /// </summary>
        public static string ByteArrayValuesToString(byte[] byteData)
        {
            string stringData = string.Empty;
            stringData = Encoding.Unicode.GetString(byteData);
            return stringData;
        }


        /// <summary>
        /// Metoda konvertuje hexadecimalne hodnoty vo vstupnom retazci (napr. 010FAF) na pole bajtov
        /// </summary>
        public static byte[] StringValuesToByteArray(string stringData)
        {
            byte[] res = new byte[stringData.Length / 2];

            for (int i = 0; i < stringData.Length / 2; i++)
                res[i] = byte.Parse(stringData.Substring(i * 2, 2), NumberStyles.HexNumber);

            return res;
        }


        public static string ArrayValuesToString(Array array)
        {
            string stringData = "";

            foreach (object item in array)
            {
                stringData += item.ToString() + " ";
            }

            return stringData;
        }
    }
}
