using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileController
{
    public string filePath;

    public string[] readAll()
    {
        //　ファイルが存在しなければ作成
        if (File.Exists(filePath))
        {
            return File.ReadAllLines(filePath);
        }
        return null;
    }

    public void writeAll(string data)
    {
        File.WriteAllText(filePath, data);
    }
}