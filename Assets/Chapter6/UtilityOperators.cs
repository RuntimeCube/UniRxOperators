using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public class UtilityOperators : MonoBehaviour
{
    [Button("RunDelay")]
    void RunDelay()
    {
        Observable.Range(1, 5)
            .Delay(TimeSpan.FromSeconds(1)) // 每个值延迟 1 秒  
            .Subscribe(x => Debug.Log($"Delayed value: {x}"));
    }

    [Button("RunDo")]
    void RunDo()
    {
        Observable.Range(1, 3)
            .Delay(TimeSpan.FromSeconds(1)) // 每个值延迟 1 秒  
            .Do(x => Debug.Log($"OnNext: {x}")) // 在每次发射值时执行  
            .DoOnCompleted(() => Debug.Log("OnCompleted")) // 在完成时执行  
            .Subscribe(x => Debug.Log($"Delayed value: {x}"));
    }

    [Button("RunMaterialize")]
    void RunMaterialize()
    {
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(5) // 模拟 5 秒的任务  
            .Materialize()
            .Do(notification =>
            {
                if (notification.Kind == NotificationKind.OnNext)
                {
                    UpdateProgressUI(notification.Value + 1, 5); // 更新进度条  
                }
                else if (notification.Kind == NotificationKind.OnCompleted)
                {
                    Debug.Log("Task completed!");
                }
            })
            .Dematerialize()
            .Subscribe();

        // 模拟更新进度条  
        void UpdateProgressUI(long current, long total)
        {
            Debug.Log($"Progress: {current}/{total}");
        }
    }

    [Button("RunObserveOn")]
    void RunObserveOn()
    {
        Observable.Create<int>(observer =>
        {
            //这里是订阅逻辑
            Debug.Log($"Observable created on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            observer.OnNext(1);
            observer.OnCompleted();
            return Disposable.Empty;
        })
        .ObserveOn(Scheduler.ThreadPool) // 指定事件通知在哪个线程（这里的通知指的是，Subscribe内的回调（OnNext、OnError、OnCompleted））
        .Subscribe(x =>
            //这里是通知逻辑
            Debug.Log($"Received {x} on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}")
        );

        //注意：
        //这个示例，只控制了通知是在ThreadPool执行，因此Subscribe()内的回调函数会在ThreadPool执行

    }

    [Button("RunSubscribeOn")]
    void RunSubscribeOn()
    {
        Observable.Create<int>(observer =>
        {
            //这里是订阅逻辑
            Debug.Log($"Observable created on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            observer.OnNext(1);
            observer.OnCompleted();
            return Disposable.Empty;
        })
         .SubscribeOn(Scheduler.ThreadPool)  // 指定事件订阅在哪个线程（这里的订阅指的是，Observable 的创建和初始化过程）
         .Subscribe(x =>
            //这里是通知逻辑
            Debug.Log($"Received {x} on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}")

        );

        //注意：
        //这个示例，只控制了订阅是在ThreadPool执行，因此Observable.Create()会发生在ThreadPool，所以整个过程都在ThreadPool
    }

    [Button("RunSubscribeOnMainThread")]
    void RunSubscribeOnMainThread()
    {
        //Observable.Start默认是在子线程执行
        Observable.Start(() =>
        {
            Debug.Log($"Loading data on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            System.Threading.Thread.Sleep(2000); // 模拟耗时操作  
            return "Data Loaded";
        })
        .SubscribeOnMainThread() // 确保订阅逻辑在主线程上执行，等同于SubscribeOn(Scheduler.MainThread) 
        .Subscribe(data =>
        {
            Debug.Log($"Updating UI with data: {data} on thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        });
    }

    /// <summary>  
    /// 生成一个随机延迟的事件流  
    /// </summary>  
    /// <param name="minDelay">最小延迟时间</param>  
    /// <param name="maxDelay">最大延迟时间</param>  
    /// <returns>随机延迟的事件流</returns>  
    private IObservable<int> GenerateRandomDelayObservable(TimeSpan minDelay, TimeSpan maxDelay)
    {
        return Observable.Defer(() =>
        {
            // 动态生成一个随机延迟时间  
            var randomDelay = TimeSpan.FromMilliseconds(UnityEngine.Random.Range((float)minDelay.TotalMilliseconds, (float)maxDelay.TotalMilliseconds));
            Debug.Log($"Next event will delay by: {randomDelay.TotalMilliseconds} ms");

            // 使用 Timer 发射单个事件，并递归调用自身  
            return Observable.Timer(randomDelay)
                .Select(_ => 1) // 发射一个值  
                .Concat(GenerateRandomDelayObservable(minDelay, maxDelay)); // 递归调用，生成下一个随机延迟  
        })
        .Scan(0, (acc, _) => acc + 1); // 累加事件计数  
    }
    [Button("RunTimeInterval")]
    void RunTimeInterval()
    {

        GenerateRandomDelayObservable(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000))
            .Take(5)
            .TimeInterval()//测量时间之间的时间间隔
            .Where(interval => interval.Interval > TimeSpan.FromSeconds(1)) // 过滤时间间隔小于 1 秒的事件  
            .Subscribe(interval =>
            {
                //Debug.Log($"Event {interval} at {DateTime.Now:HH:mm:ss.fff}");
                Debug.Log($"Value: {interval.Value}, Interval: {interval.Interval.TotalMilliseconds} ms");
            }, onCompleted: () => {
                Debug.Log("Completed");
            });
    }
    [Button("RunTimeout")]
    void RunTimeout()
    {
        GenerateRandomDelayObservable(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(2000))
            .Timeout(TimeSpan.FromSeconds(1)) // 如果超过 1 秒未发射值，则超时  
            .Subscribe(
                x => Debug.Log($"Value: {x}"),
                ex => Debug.Log($"Timeout Error: {ex}"),
                () => Debug.Log("Completed")
            );
    }

    [Button("RunTimestamp")]
    void RunTimestamp()
    {
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(3)
            .Timestamp()
            .Subscribe(x => Debug.Log($"Value: {x.Value}, Timestamp: {x.Timestamp}"));
    }
}