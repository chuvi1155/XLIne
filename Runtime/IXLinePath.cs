using UnityEngine;

internal interface IXLinePath
{
    /// <summary>
    ///  (Editor only)
    /// </summary>
    bool InEditorShowGizmos { get; }
    /// <summary>
    /// Указывает плавность сегментов (Editor only). По умолчанию возвращает 50
    /// </summary>
    int Precision { get; }
    bool IsDirty { get; set; }
    void OnSubLineChanged(XLinePathSubLine subLine);
}
