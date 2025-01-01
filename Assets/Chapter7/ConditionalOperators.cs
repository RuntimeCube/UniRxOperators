using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConditionalOperators : MonoBehaviour
{
    [Button("RunAmb")]
    void RunAmb()
    {
        var first = Observable.Timer(TimeSpan.FromSeconds(1)).Select(_ => "First");
        var second = Observable.Timer(TimeSpan.FromSeconds(2)).Select(_ => "Second");

        first.Amb(second)
            .Subscribe(x => Debug.Log($"Emitted: {x}"));
    }



    [Button("RunDefaultIfEmpty")]
    void RunDefaultIfEmpty()
    {
        //示例1
        Observable.Range(1, 3) // 发射 1 到 3  
            .SelectMany(x =>
            {
                if (x > 4)
                {
                    // 如果值大于 4，则正常发送
                    return Observable.Return(x);
                }
                return Observable.Empty<int>(); // 否则发射空  
            })
            .DefaultIfEmpty(8) // 当流完成时，如果没有收到任何事件，则发射默认值 8  
            .Subscribe(
                x => Debug.Log($"示例1 : OnNext: {x}"),
                () => Debug.Log("示例1 : OnCompleted")
            );

        //示例2
        Observable.Range(1, 3) // 发射 1 到 3  
            .SelectMany(x =>
            {
                if (x > 1)
                {
                    // 如果值大于 1，则正常发送
                    return Observable.Return(x);
                }
                return Observable.Empty<int>(); // 否则发射空  
            })
            .DefaultIfEmpty(8) // 当流完成时， 如果没有收到任何事件，则发射默认值 8  
            .Subscribe(
                x => Debug.Log($"示例2 : OnNext: {x}"),
                () => Debug.Log("示例2 : OnCompleted")
            );
    }

    [Button("RunSkipUntil")]
    void RunSkipUntil()
    {
        // 模拟计时器，每秒发射一个值  
        var timer = Observable.Interval(System.TimeSpan.FromSeconds(1));

        // 模拟玩家点击“准备”按钮  
        var playerReady = Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Space)) // 按下空格键表示玩家准备  
            .Take(1); // 只触发一次  

        // 使用 SkipUntil 等待玩家准备后开始计时  
        timer.SkipUntil(playerReady)//一直skip，直到playerReady流开始接收到事件
            .Subscribe(
                x => Debug.Log($"Timer: {x}"),
                () => Debug.Log("Timer completed")
            );
    }


    [Button("RunTakeUntil")]
    void RunTakeUntil()
    {
        // 模拟计时器，每秒发射一个值  
        var timer = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Select(x => $"Timer: {x + 1}s");

        // 模拟任务完成事件  
        var taskCompleted = Observable.Timer(System.TimeSpan.FromSeconds(10)); // 10 秒后任务完成  

        // 使用 TakeUntil 在任务完成时停止计时器  
        timer.TakeUntil(taskCompleted)
            .Subscribe(
                time => Debug.Log(time),
                () => Debug.Log("Timer stopped because task was completed")
            );
    }

    [Button("RunSkipWhile")]
    void RunSkipWhile()
    {
        // 模拟玩家血量变化, 从100
        var playerHealth = Observable.Create<int>(x =>
        {
            x.OnNext(100);
            x.OnNext(20); //第一次低于30
            x.OnNext(80);
            x.OnNext(10); //第二次低于30
            x.OnCompleted();
            // 返回一个 IDisposable，用于清理资源  
            return Disposable.Empty;
        });

        // 使用 SkipWhile 跳过血量高于 30 的事件  ，！！！！只要有一次条件不满足，则后续不再检查
        playerHealth.SkipWhile(health => health > 30)
            .Subscribe(
                health => Debug.Log($"[SkipWhile] Warning! Low health: {health}"),
                () => Debug.Log("Health monitoring completed")
            );
        //第一次低于30开始推送流，之后都不再检测

    }

    [Button("RunTakeWhile")]
    void RunTakeWhile()
    {
        // 模拟玩家血量变化, 从100
        var playerHealth = Observable.Create<int>(x =>
        {
            x.OnNext(100);
            x.OnNext(80);
            x.OnNext(20); //第一次低于30
            x.OnNext(10); //第二次低于30
            x.OnCompleted();
            // 返回一个 IDisposable，用于清理资源  
            return Disposable.Empty;
        });

        // 使用 TakeWhile 只要条件不满足，则直接complete
        playerHealth.TakeWhile(health => health > 30)
            .Subscribe(
                health => Debug.Log($"[SkipWhile] Warning! Low health: {health}"),
                () => Debug.Log("Health monitoring completed")
            );
        //第一次低于20，立即complete

    }

}
