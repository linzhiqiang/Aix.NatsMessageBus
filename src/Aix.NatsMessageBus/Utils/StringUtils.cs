using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aix.NatsMessageBus.Utils
{
    internal static class StringUtils
    {
        public static string EmptyGuidStr = "00000000-0000-0000-0000-000000000000";//Guid.Empty.ToString();

        /// <summary>
        /// 找到一个不为empty的返回
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string IfEmpty(params string[] strs)
        {
            string result = string.Empty;
            if (strs != null)
            {
                foreach (var item in strs)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        result = item;
                        break;
                    }
                }
            }

            return result;
        }

        public static string SubString(string value, int length)
        {
            if (value == null) return string.Empty;
            if (value.Length <= length) return value;
            return value.Substring(0, length);
        }

        /// <summary>
        /// 超出长度的添加后缀
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string SubString(string value, int length, string postfix)
        {
            if (value == null) return string.Empty;
            if (value.Length <= length) return value;

            return value.Substring(0, length) + postfix ?? "";
        }


        public static string UrlCombine(string path1, string path2)
        {
            if (path1 != null) path1 = path1.TrimEnd('/');
            if (path2 != null) path2 = path2.TrimStart('/');

            return path1 + "/" + path2;
        }


        /// <summary>
        /// 驼峰字符串转换下划线格式  如：userName  -> user_name
        /// </summary>
        /// <param name="camelCase"></param>
        /// <returns></returns>
        public static String ToLine(String camelCase)
        {
            if (string.IsNullOrEmpty(camelCase)) return camelCase;
            Regex regex = new Regex("[A-Z]");
            var list = regex.Matches(camelCase);
            if (list.Count > 0)
            {
                StringBuilder sb = new StringBuilder(camelCase);
                foreach (Match item in list)
                {
                    sb.Replace(item.Value, "_" + item.Value.ToLower());
                }
                return sb.ToString();
            }

            return camelCase;
        }

        public static List<string> Split(string str, char[] separator)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(str)) return result;

            string[] arrs = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (arrs != null && arrs.Length > 0)
            {
                result.AddRange(arrs);
            }

            return result;
        }

        public static List<int> SplitToIntList(string str, char separator)
        {
            List<int> result = new List<int>();
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            string[] array = str.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length > 0)
            {
                foreach (var item in array)
                {
                    result.Add(NumberUtils.ToInt(item));
                }
            }

            return result;
        }

        public static List<long> SplitToLongList(string str, char separator)
        {
            List<long> result = new List<long>();
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }

            string[] array = str.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length > 0)
            {
                foreach (var item in array)
                {
                    result.Add(NumberUtils.ToLong(item));
                }
            }

            return result;
        }

        public static bool IsEmptyGuid(string value)
        {
            return string.IsNullOrEmpty(value) || value == EmptyGuidStr;
        }

    }
}
