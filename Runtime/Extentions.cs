using UnityEngine;

public static class Extentions
{
    /// <summary>
    /// возвращает смещенный Rect на указанное значение (не смещает исходный rect)
    /// </summary>
    /// <param name="r"></param>
    /// <param name="x_offset"></param>
    /// <param name="y_offset"></param>
    /// <returns></returns>
    public static Rect Offset(this Rect r, float x_offset, float y_offset)
    {
        Rect r1 = r;
        r1.x += x_offset;
        r1.y += y_offset;
        return r1;
    }
    /// <summary>
    /// возвращает смещенный Rect на указанное значение (не смещает исходный rect)
    /// </summary>
    /// <param name="r"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Rect Offset(this Rect r, Vector2 offset)
    {
        return r.Offset(offset.x, offset.y);
    }
    
    /// <summary>
    /// возвращает выровненный rect относительно parent
    /// </summary>
    /// <param name="r"></param>
    /// <param name="parent"></param>
    /// <param name="align"></param>
    /// <param name="x_offset"></param>
    /// <param name="y_offset"></param>
    /// <returns></returns>
    public static Rect Alignment(this Rect r, Rect parent, TextAnchor align, float x_offset, float y_offset)
    {
        if (align == TextAnchor.MiddleCenter)
            return new Rect(parent.x + (parent.width - r.width) / 2f + x_offset, parent.y + (parent.height - r.height) / 2f + y_offset, r.width, r.height);
        else if (align == TextAnchor.LowerCenter)
            return new Rect(parent.x + (parent.width - r.width) / 2f + x_offset, (parent.yMax - r.height) + y_offset, r.width, r.height);
        else if (align == TextAnchor.UpperCenter)
            return new Rect(parent.x + (parent.width - r.width) / 2f + x_offset, parent.y + y_offset, r.width, r.height);
        else if (align == TextAnchor.MiddleLeft)
            return new Rect(parent.x + x_offset, parent.y + (parent.height - r.height) / 2f + y_offset, r.width, r.height);
        else if (align == TextAnchor.LowerLeft)
            return new Rect(parent.x + x_offset, (parent.yMax - r.height) + y_offset, r.width, r.height);
        else if (align == TextAnchor.UpperLeft)
            return new Rect(parent.x + x_offset, parent.y + y_offset, r.width, r.height);
        else if (align == TextAnchor.MiddleRight)
            return new Rect((parent.xMax - r.width) + x_offset, parent.y + (parent.height - r.height) / 2f + y_offset, r.width, r.height);
        else if (align == TextAnchor.LowerRight)
            return new Rect((parent.xMax - r.width) + x_offset, (parent.yMax - r.height) + y_offset, r.width, r.height);
        else if (align == TextAnchor.UpperRight)
            return new Rect((parent.xMax - r.width) + x_offset, parent.y + y_offset, r.width, r.height);

        return r;
    }
    /// <summary>
    /// возвращает выровненный rect относительно parent
    /// </summary>
    /// <param name="r"></param>
    /// <param name="parent"></param>
    /// <param name="align"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Rect Alignment(this Rect r, Rect parent, TextAnchor align, Vector2 offset)
    {
        return r.Alignment(parent, align, offset.x, offset.y);
    }
    /// <summary>
    /// возвращает выровненный rect относительно parent
    /// </summary>
    /// <param name="r"></param>
    /// <param name="parent"></param>
    /// <param name="align"></param>
    /// <returns></returns>
    public static Rect Alignment(this Rect r, Rect parent, TextAnchor align)
    {
        return r.Alignment(parent, align, 0, 0);
    }
        
    /// <summary>
    /// Возвращает размер ректа
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Vector2 Size(this Rect r)
    {
        return new Vector2(r.width, r.height);
    }
    /// <summary>
    /// Возвращает позицию ректа
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Vector2 Position(this Rect r)
    {
        return new Vector2(r.x, r.y);
    }
    /// <summary>
    /// Возвращает позицию ректа
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Vector2 MaxPosition(this Rect r)
    {
        return new Vector2(r.xMax, r.yMax);
    }

    public static Rect Union(this Rect r, Rect rect)
    {
        return Rect.MinMaxRect(Mathf.Min(r.x, rect.x, r.xMax, rect.xMax), Mathf.Min(r.y, rect.y, r.yMax, rect.yMax),
            Mathf.Max(r.x, rect.x, r.xMax, rect.xMax), Mathf.Max(r.y, rect.y, r.yMax, rect.yMax));
    }

    public static Rect Lerp(this Rect r, Rect to, float t)
    {
        Vector2 pos = Vector2.Lerp(r.Position(), to.Position(), t);
        Vector2 sz = Vector2.Lerp(r.Size(), to.Size(), t);
        return new Rect(pos.x, pos.y, sz.x, sz.y);
    }
    public static Rect Lerp(this Rect r, Vector2 sizeTo, TextAnchor align, float t)
    {
        Vector2 szFrom = r.Size();
        Vector2 szNew = Vector2.Lerp(szFrom, sizeTo, t);
        switch (align)
        {
            case TextAnchor.LowerCenter:
                {
                    Vector2 center = r.center;
                    Vector2 halfszNew = szNew / 2f;
                    return new Rect(center.x - halfszNew.x, r.yMax - szNew.y, szNew.x, szNew.y);
                }
            case TextAnchor.LowerLeft:
                return new Rect(r.x, r.yMax - szNew.y, szNew.x, szNew.y);
            case TextAnchor.LowerRight:
                return new Rect(r.xMax - szNew.x, r.yMax - szNew.y, szNew.x, szNew.y);
            case TextAnchor.MiddleCenter:
                {
                    Vector2 center = r.center;
                    Vector2 halfszNew = szNew / 2f;
                    return new Rect(center.x - halfszNew.x, center.y - halfszNew.y, szNew.x, szNew.y);
                }
            case TextAnchor.MiddleLeft:
                {
                    Vector2 center = r.center;
                    Vector2 halfszNew = szNew / 2f;
                    return new Rect(r.x, center.y - halfszNew.y, szNew.x, szNew.y);
                }
            case TextAnchor.MiddleRight:
                {
                    Vector2 center = r.center;
                    Vector2 halfszNew = szNew / 2f;
                    return new Rect(r.xMax - szNew.x, center.y - halfszNew.y, szNew.x, szNew.y);
                }
            case TextAnchor.UpperCenter:
                {
                    Vector2 center = r.center;
                    Vector2 halfszNew = szNew / 2f;
                    return new Rect(center.x - halfszNew.x, r.y, szNew.x, szNew.y);
                }
            case TextAnchor.UpperLeft:
                return new Rect(r.x, r.y, szNew.x, szNew.y);
            case TextAnchor.UpperRight:
                return new Rect(r.xMax - szNew.x, r.y, szNew.x, szNew.y);
        }
        return r;
    }
}
