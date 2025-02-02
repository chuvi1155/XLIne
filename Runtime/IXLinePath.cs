using UnityEngine;

internal interface IXLinePath
{
    /// <summary>
    ///  (Editor only)
    /// </summary>
    bool InEditorShowGizmos { get; }
    /// <summary>
    /// ��������� ��������� ��������� (Editor only). �� ��������� ���������� 50
    /// </summary>
    int Precision { get; }
    bool IsDirty { get; set; }
    void OnSubLineChanged(XLinePathSubLine subLine);
}
