using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ErrorHandlingOperators : MonoBehaviour
{
    [Button("RunCatch")]
    void RunCatch()
    {

        // 模拟加载资源  
        LoadResourceAsync("Cube") // 模拟加载失败  
            .Catch((System.Exception ex) =>
            {
                Debug.LogError($"Error loading resource: {ex.Message}");
                return Observable.Return(CreatePlaceholderResource()); // 提供占位资源  
            }) // 捕获错误并提供占位资源  
            .Subscribe(
                resource => {
                    Debug.Log($"Loaded Resource: {resource.name}");
                    GameObject.Instantiate(resource);
                },
                error => Debug.LogError($"OnError: {error.Message}"),
                () => Debug.Log("OnCompleted")
            );
    }
    // 模拟异步加载资源  
    private IObservable<GameObject> LoadResourceAsync(string resourceName)
    {
        return Observable.Start(() =>
        {
            // 模拟加载失败  
            var resource = Resources.Load<GameObject>(resourceName);
            if (resource == null)
            {
                throw new System.Exception($"Resource '{resourceName}' not found!");
            }
            return resource;
        }, Scheduler.MainThread);// 注意Resources.Load只能在unity主线程被调用
    }
    // 创建占位资源  
    private GameObject CreatePlaceholderResource()
    {
        var placeholder = Resources.Load<GameObject>("Placeholder");
        return placeholder;
    }


    [Button("RunCatchIgnore1")]
    void RunCatchIgnore1()
    {
            Observable.Create<string>(observer => {
                observer.OnNext("hello");
                observer.OnCompleted();
                return Disposable.Empty;
            })
            .Select(x=> System.Convert.ToInt32(x)) // 假设输入的数据不能被转换为int，则会抛出异常
            .CatchIgnore() // 忽略错误  
            .Subscribe(
                value => Debug.Log($"Input: {value}"),
                error => Debug.LogError($"OnError: {error}"), // 不会触发  
                () => Debug.Log("OnCompleted")
            );
    }
    [Button("RunCatchIgnore2")]
    void RunCatchIgnore2()
    {
            Observable.Create<string>(observer => {
                observer.OnNext("hello");
                observer.OnCompleted();
                return Disposable.Empty;
            })
            .Select(x => System.Convert.ToInt32(x)) // 假设输入的数据不能被转换为int，则会抛出异常
            .CatchIgnore<int, System.FormatException>(ex => {
                Debug.Log("Ignore FormatException");
            }) // 忽略特定类型的错误  
            .Subscribe(
                value => Debug.Log($"Input: {value}"),
                error => Debug.LogError($"OnError: {error}"), // FormatException不会触发  
                () => Debug.Log("OnCompleted")
            );
    }

    [Button("RunOnErrorResumeNext")]
    void RunOnErrorResumeNext()
    {
        Observable.Create<string>(observer => {
            observer.OnNext("hello");
            observer.OnCompleted();
            return Disposable.Empty;
        })
        .Select(x => System.Convert.ToInt32(x))// 假设输入的数据不能被转换为int，则会抛出异常
        .OnErrorRetry<int,System.FormatException>(ex=> {
            Debug.Log("OnErrorRetry FormatException");
        }, 3, TimeSpan.FromSeconds(1))//每隔1秒重试一次，总共重试3次
        .Subscribe(
            value => Debug.Log($"Input: {value}"),
            error => Debug.LogError($"OnError: {error}"),
            () => Debug.Log("OnCompleted")
        );
    }

    [Button("RunRetry")]
    void RunRetry()
    {
        Observable.Create<string>(observer =>
        {
            // 模拟网络请求  
            bool success = UnityEngine.Random.value > 0.7f; // 30% 成功率  
            if (success)
            {
                observer.OnNext("Data from server");
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new Exception("Network error"));
                // throw new Exception("Throw Error");
            }
            return Disposable.Empty;
        })
        .DoOnSubscribe(() => {
            Debug.Log("OnSubscribe 会调用多次");
        })
        .DoOnError((ex) => {
            Debug.Log("DoOnError");
        })
        .Retry(3) // 最多重试 3 次  
        .DoOnSubscribe(() => {
            Debug.Log("OnSubscribe 只会调用一次");
        })
        .Subscribe(
            data => Debug.Log($"Received: {data}"),
            ex => Debug.LogError($"Failed after retries: {ex.Message}"),
            () => Debug.Log("Request completed")
        );
    }

    [Button("RunFinally")]
    void RunFinally()
    {
        Observable.Create<string>(observer =>
        {
            // 模拟网络请求  
            bool success = false; // 始终失败
            //bool success = true; // 始终成功
            if (success)
            {
                observer.OnNext("Data from server");
                observer.OnCompleted();
            }
            else
            {
                observer.OnError(new Exception("Network error"));
                //throw new Exception("Throw Error");
            }
            return Disposable.Empty;
        })
        .Finally(() => {
            Debug.Log("Finally");
        })
        .DoOnCompleted(()=> {
            Debug.Log("DoOnCompleted");
        })
        .DoOnError((ex) => {
            Debug.Log("DoOnError");
        })
        .Subscribe(
            data => Debug.Log($"Received: {data}"),
            ex => Debug.LogError($"Failed: {ex.Message}"),
            () => Debug.Log("Request completed")
        );
    }

}
