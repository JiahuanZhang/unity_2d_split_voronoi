using System;
using System.Collections.Generic;

public static class ListExtension
{
    /// <summary>
    /// 过滤满足条件的项,删除不满足的项（无新的内存消耗）
    /// </summary> 
    public static void FiliterInSelf<T>(this List<T> list, Predicate<T> pridecate)
    {
        if (list == null) return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (!pridecate.Invoke(list[i]))
            {
                list.RemoveAt(i);
            }
        }
    }
}