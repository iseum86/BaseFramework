using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Base.Data;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class TableBakeManager : EditorWindow
{
    // 필드 규칙: _camelCase 사용
    private static readonly string _jsonPath = "Assets/02_AddressableResources/00_Json/";
    private static readonly string _exportPath = "Assets/AddressableResources/Tables/";
    private static readonly string _encryptKey = "MyProjectSecretKey!@#"; // 프로젝트 고유 키

    [MenuItem("Tools/Table/Bake All Tables (Auto)")]
    public static void BakeAllAuto()
    {
        // 1. SerializableTableData를 상속받는 모든 실제 데이터 아이템 클래스 수집
        var tableItemTypes = GetAllTableItemTypes();
        
        if (!Directory.Exists(_jsonPath))
        {
            Debug.LogError($"[TableBake] JSON 경로를 찾을 수 없습니다: {_jsonPath}");
            return;
        }

        // 2. JSON 폴더 내의 모든 .json 파일 스캔
        var jsonFiles = Directory.GetFiles(_jsonPath, "*.json");
        var successCount = 0;

        foreach (var filePath in jsonFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // 3. 파일 이름으로 매칭되는 클래스 타입 찾기
            var targetType = MatchType(fileName, tableItemTypes);

            if (targetType != null)
            {
                if (Bake(targetType, filePath, fileName))
                {
                    successCount++;
                }
            }
            else
            {
                // 솔직하고 명확한 로그 출력
                Debug.LogWarning($"[TableBake] 매칭되는 클래스가 없어 건너뜁니다: {fileName}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>[TableBake] 자동 베이킹 완료! (성공: {successCount}/{jsonFiles.Length})</color>");
    }

    private static bool Bake(Type type, string jsonPath, string fileName)
    {
        try
        {
            var jsonText = File.ReadAllText(jsonPath);
            var jsonObject = JObject.Parse(jsonText);
            var jsonArray = jsonObject["data"] as JArray;

            if (jsonArray == null) return false;

            using (var plainMs = new MemoryStream())
            using (var writer = new BinaryWriter(plainMs))
            {
                // 헤더: 행(Row) 개수 기록
                writer.Write(jsonArray.Count);

                // 리플렉션으로 private 필드들을 선언 순서대로 가져옴
                // 주의: SerializableTableData의 _tblidx 필드부터 읽어야 하므로 계층 구조 고려
                var fields = GetHierarchyFields(type);

                foreach (var row in jsonArray)
                {
                    foreach (var field in fields)
                    {
                        WriteField(writer, field, row);
                    }
                }

                // 암호화 및 압축하여 .bytes 파일로 저장
                SaveToBinaryFile(plainMs, fileName);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[TableBake] {fileName} 베이킹 실패: {e.Message}");
            return false;
        }
    }

    private static void WriteField(BinaryWriter writer, FieldInfo field, JToken row)
    {
        var jsonKey = field.Name; 
        var token = row[jsonKey];

        // var 최우선 사용 및 타입별 쓰기
        if (field.FieldType == typeof(int)) writer.Write(token?.Value<int>() ?? 0);
        else if (field.FieldType == typeof(float)) writer.Write(token?.Value<float>() ?? 0f);
        else if (field.FieldType == typeof(string)) writer.Write(token?.Value<string>() ?? string.Empty);
        else if (field.FieldType == typeof(bool)) writer.Write(token?.Value<bool>() ?? false);
        else if (field.FieldType == typeof(TblIndex)) writer.Write(token?.Value<int>() ?? -1);
        else if (field.FieldType.IsEnum) writer.Write(token?.Value<int>() ?? 0);
        else
        {
            Debug.LogWarning($"[TableBake] 지원하지 않는 타입입니다: {field.FieldType.Name} ({field.Name})");
        }
    }

    private static void SaveToBinaryFile(MemoryStream plainMs, string fileName)
    {
        if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);

        var finalPath = $"{_exportPath}{fileName}.bytes";
        plainMs.Position = 0;

        using (var fs = new FileStream(finalPath, FileMode.Create))
        {
            // 기존 AesEncryptor.Encrypt (압축 포함) 로직 호출
            AesEncryptor.Encrypt(plainMs, _encryptKey, fs, isCompress: true);
        }
        
        AssetDatabase.ImportAsset(finalPath);
        AddToAddressables(finalPath, fileName);
    }

    private static Type MatchType(string fileName, List<Type> types)
    {
        var formattedName = string.Join("", fileName.Split('_').Select(s => char.ToUpper(s[0]) + s.Substring(1)));
        
        // 2. "_table"을 제거하고 "Tbl" 접두사와 "DataItem" 접미사 조합 시도
        var searchName = "Table" + formattedName.Replace("Table", "") + "DataItem";
        
        return types.FirstOrDefault(t => t.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));
    }

    private static List<Type> GetAllTableItemTypes()
    {
        var baseType = typeof(SerializableTableData);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract)
            .ToList();
    }

    private static List<FieldInfo> GetHierarchyFields(Type type)
    {
        var fieldList = new List<FieldInfo>();
        var currentType = type;

        while (currentType != null && currentType != typeof(object))
        {
            // DeclaredOnly로 해당 클래스에 정의된 것만 가져와서 스택에 쌓음
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
            // 역순으로 삽입하여 부모 -> 자식 순서 유지
            for (var i = fields.Length - 1; i >= 0; i--)
            {
                // [중요] 이름이 중복된 필드(hiding 등)가 있다면 자식의 것을 우선함
                if (fieldList.Any(f => f.Name == fields[i].Name)) continue;
                fieldList.Insert(0, fields[i]);
            }
            currentType = currentType.BaseType;
        }
        return fieldList;
    }
    
    private static void AddToAddressables(string assetPath, string addressName)
    {
        // 1. 어드레서블 설정 가져오기
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[TableBake] AddressableAssetSettings를 찾을 수 없습니다.");
            return;
        }

        // 2. 테이블 전용 그룹 찾기 (없으면 기본 그룹 사용)
        var groupName = "Tables";
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, null);
        }

        // 3. 에셋 등록 및 주소 설정
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
    
        // 파일명을 주소로 설정 (예: table_monster)
        entry.address = addressName;

        // 4. 변경 사항 저장
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
    }
}