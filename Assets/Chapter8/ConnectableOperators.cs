using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConnectableOperators : MonoBehaviour
{

    [Button("RunPublish")]
    void RunPublish()
    {
        // 创建一个普通的 Observable，每秒发射一个值  
        var source = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Take(5); // 限制为 5 个事件  

        // 使用 Publish 转换为 Connectable Observable  
        var connectable = source.Publish();

        // 第一个订阅者  
        connectable.Subscribe(x => Debug.Log($"Subscriber 1: {x}"));

        // 延迟 3 秒后添加第二个订阅者  
        Observable.Timer(System.TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                connectable.Subscribe(x => Debug.Log($"Subscriber 2: {x}"));
            });

        // 调用 Connect 开始发射事件  
        connectable.Connect();
    }

    [Button("RunReplay")]
    void RunReplay()
    {
        // 创建一个普通的 Observable，每秒发射一个值  
        var source = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Take(5); // 限制为 5 个事件  

        // 使用 Replay 转换为 Connectable Observable，并缓存所有事件  
        var replayed = source.Replay();

        // 第一个订阅者  
        replayed.Subscribe(x => Debug.Log($"Subscriber 1: {x}"));

        // 调用 Connect 开始发射事件  
        replayed.Connect();

        // 延迟 3 秒后添加第二个订阅者  
        Observable.Timer(System.TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                replayed.Subscribe(x => Debug.Log($"Subscriber 2: {x}"));
            });
    }

    [Button("RunRefCount")]
    void RunRefCount()
    {

        // 创建一个普通的 Observable，每秒发射一个值  
        var source = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Take(5); // 限制为 5 个事件  

        // 使用 Publish 转换为 Connectable Observable，并使用 RefCount 自动管理连接  
        var refCounted = source.Publish().RefCount();

        // 第一个订阅者  
        var subscription1 = refCounted.Subscribe(x => Debug.Log($"Subscriber 1: {x}"));

        // 延迟 2 秒后添加第二个订阅者  
        Observable.Timer(System.TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                var subscription2 = refCounted.Subscribe(x => Debug.Log($"Subscriber 2: {x}"));

                // 再延迟 3 秒后取消第二个订阅者  
                Observable.Timer(System.TimeSpan.FromSeconds(3))
                    .Subscribe(__ => subscription2.Dispose());
            });
    }

}
