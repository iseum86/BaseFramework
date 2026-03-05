using System;
using System.Collections.Generic;
using System.IO;
using Base.Utils;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Base.Data
{
    public interface ITableContainer
    {
        string TableName { get; }
        UniTask LoadAsync();
        void UnLoad();
    }

    public abstract class TableContainer<T> : ITableContainer where T : SerializableTableData, new()
    {
        protected readonly Dictionary<TblIndex, T> _dicTableData = new();
        
        private const string EncryptKey = "MyProjectSecretKey!@#";

        public abstract string TableName { get; }

        public async UniTask LoadAsync()
        {
            // 1. 어드레서블을 통해 에셋 로드 (ResourceManager 활용)
            var asset = await Director.ResourceMgr.LoadAssetAsync<TextAsset>(TableName);
            if (asset == null)
            {
                DevLog.Error($"[Table] 에셋 로드 실패: {TableName}");
                return;
            }

            // 2. 복호화 프로세스 (메모리 안전을 위해 MemoryStream 사용)
            using (var inputMs = new MemoryStream(asset.bytes))
            using (var outputMs = new MemoryStream())
            {
                AesEncryptor.Decrypt(inputMs, EncryptKey, outputMs, isCompress: true);

                // 3. [중요] 복호화 후 스트림 포지션을 맨 앞으로 되돌립니다.
                outputMs.Position = 0;

                // 4. 바이너리 데이터 읽기
                using (var reader = new TableBinaryReader(outputMs))
                {
                    try
                    {
                        // 헤더: 전체 행 개수 읽기
                        var count = reader.ReadInt32();

                        for (var i = 0; i < count; i++)
                        {
                            var item = new T();
                            // 리플렉션 캐싱을 이용한 자동 필드 로드
                            item.Read(reader);

                            if (item.IsValid())
                            {
                                AddData(item.tblidx, item);
                            }
                        }
                        
                        DevLog.Info($"[Table] {TableName} 로드 완료 ({count} Rows)");
                    }
                    catch (Exception e)
                    {
                        DevLog.Error($"[Table] {TableName} 파싱 중 에러: {e.Message}");
                    }
                }
            }

            // 메모리 해제를 위해 에셋 참조 제거 (선택 사항)
            // Director.ResourceMgr.ReleaseAsset(asset);
        }

        protected void AddData(TblIndex index, T data)
        {
            if (_dicTableData.ContainsKey(index))
            {
                DevLog.Warning($"[Table] 중복된 키 발견: {TableName} (ID: {index.Value})");
                return;
            }
            _dicTableData.Add(index, data);
        }

        public T GetData(TblIndex index)
        {
            return _dicTableData.TryGetValue(index, out var data) ? data : null;
        }

        public void UnLoad()
        {
            _dicTableData.Clear();
            UnLoadCustomData();
        }

        protected abstract void UnLoadCustomData();
    }
}