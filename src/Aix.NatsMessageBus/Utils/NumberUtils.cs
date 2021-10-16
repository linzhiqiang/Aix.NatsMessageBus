using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus.Utils
{
    public class NumberUtils
    {

        public static int ToInt(object obj, int defaultValue)
        {
            return (int)ToDecimal(obj, defaultValue);
        }

        public static int ToInt(object obj)
        {
            return (int)ToDecimal(obj, 0);
        }

        public static int? ToIntNullable(object obj)
        {
            decimal result;
            if (obj != null && decimal.TryParse(obj.ToString(), out result))
            {
                return (int)result;
            }
            return null;
        }

        public static long? ToLongNullable(object obj)
        {
            long result;
            if (obj != null && long.TryParse(obj.ToString(), out result))
            {
                return result;
            }
            return null;
        }

        public static long ToLong(object obj, long defaultValue)
        {
            long result;
            if (obj != null && long.TryParse(obj.ToString(), out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static long ToLong(object obj)
        {
            return ToLong(obj, 0);
        }

        public static decimal ToDecimal(object obj, decimal defaultValue)
        {
            decimal result;
            if (obj != null && decimal.TryParse(obj.ToString(), out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static decimal ToDecimal(object obj)
        {
            return ToDecimal(obj, 0.0m);
        }

        public static double ToDouble(object obj, double defaultValue)
        {
            double result;
            if (obj != null && double.TryParse(obj.ToString(), out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static double ToDouble(object obj)
        {
            return ToDouble(obj, 0.0);
        }

        public static sbyte ToBoolSbyte(object obj)
        {
            int intValue = ToInt(obj);
            return intValue > 0 ? (sbyte)1 : (sbyte)0;
        }

        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int GetDecimalPlaces(decimal val)
        {
            var str = val.ToString();
            var idx = str.IndexOf('.');
            if (idx < 0)
                return 0;
            return str.Substring(idx).Length - 1;
        }

        /// <summary>
        /// 保留几位小数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static decimal Round(decimal value, int decimals = 2)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
        #region 二进制枚举 转换
        /// <summary>
        /// 枚举和 拆分  如15->1,2,4,8
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int[] SplitBinary(int value)
        {
            if (value <= 0) return new int[0];

            List<int> result = new List<int>();
            int index = 1;
            while (value >= index)
            {
                if ((value & index) == index)
                {
                    result.Add(index);
                }
                index = index << 1;
            }
            return result.ToArray();
        }

        /// <summary>
        /// 枚举值数组转换为整数 [1,2,4]=>7
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static int BinaryArrayToInt(int[] arr)
        {
            var result = 0;
            if (arr == null || arr.Length == 0) return result;

            foreach (var item in arr)
            {
                result = result | item;
            }

            return result;
        }

        /* js 代码
           enumToArray(value){
            if(!value) value=0;
            var temp = 1;
            var result = [];
            while(value>=temp){
                if((value & temp) >0) result.push(temp+'');
                temp= temp*2;
            }
           return  result;
        },

        arrayToEnum(arr){
            var result=0;
            for(var i=0;i< arr.length;i++){
                result=result | arr[i];
            }
            return result;
        },
         */

        #endregion
    }
}
