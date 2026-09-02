using System.Collections;
using LitJson;

/// <summary>
/// 이 프로젝트의 LitJson.dll 은 JsonData 에 ContainsKey 가 없어서 보조 확장을 둔다.
/// </summary>
public static class JsonEx
{
    /// 오브젝트이고 해당 key 가 있으면 true. 그 외 모든 경우 false (예외 없음).
    public static bool Has(this JsonData d, string key)
    {
        if (d == null) return false;
        try
        {
            if (!d.IsObject) return false;
            return ((IDictionary)d).Contains(key);
        }
        catch
        {
            try { return d[key] != null; } catch { return false; }
        }
    }

    /// key 의 문자열 값. 없으면 fallback.
    public static string GetStr(this JsonData d, string key, string fallback = null)
        => d.Has(key) && d[key] != null ? d[key].ToString() : fallback;
}
