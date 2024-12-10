using EasyButtons;
using System;
using System.Threading;
using UniRx;
using UnityEngine;


public class CreatingObservables : MonoBehaviour
{
    [Button("RunCreate")]
    private void RunCreate()
    {
        var observable = Observable.Create<int>(observer =>
        {
            // 发出数据  
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);

            // 发出完成信号  
            observer.OnCompleted();

            // 返回一个 IDisposable，用于清理资源  
            return Disposable.Empty;
        });

        // 订阅观察者
        observable.Subscribe(
            x => Debug.Log($"OnNext: {x}"),
            ex => Debug.LogError($"OnError: {ex}"),
            () => Debug.Log("OnCompleted")
        );
    }
    [Button("RunDefer错误示例")]
    private void RunDefer_Wrong()
    {
        // 错误示例：随机数在声明时生成  
        var randomObservable = Observable.Return(UnityEngine.Random.Range(1, 100));
        randomObservable.Subscribe(x => Debug.Log($"Subscription 1: {x}"));
        randomObservable.Subscribe(x => Debug.Log($"Subscription 2: {x}"));
    }

    [Button("RunDefer正确示例")]
    private void RunDefer_True()
    {
        // 正确示例：随机数在订阅时生成  
        // Defer是延迟创建的，会在订阅时才创建
        var randomObservable = Observable.Defer(() =>
        {
            return Observable.Return(UnityEngine.Random.Range(1, 100));
        });

        randomObservable.Subscribe(x => Debug.Log($"Subscription 1: {x}"));
        randomObservable.Subscribe(x => Debug.Log($"Subscription 2: {x}"));
    }

    /// <summary>
    /// 转换前的事件
    /// </summary>
    public event Action OnCustomEvent;

    [Button("RunFrom")]
    private void RunFrom()
    {
        // 将自定义事件转换为 Observable  
        Observable.FromEvent(
            handler => OnCustomEvent += handler, // 添加事件监听器  
            handler => OnCustomEvent -= handler  // 移除事件监听器  
        )
        .Subscribe(_ =>
        {
            Debug.Log("自定义事件被触发!");
        })
        .AddTo(this);//和当前MonoBehaviour绑定生命周期，用于销毁时取消监听

        // 模拟触发事件  
        Invoke(nameof(TriggerCustomEvent), 2f); // 等待2秒后触发事件  
    }
    private void TriggerCustomEvent()
    {
        OnCustomEvent?.Invoke(); // 触发事件  
    }

    public UnityEngine.UI.Button myButton; // 在 Inspector 中拖入按钮 

    [Button("RunFromUnityEvent")]
    private void RunFromUnityEvent()
    {
        // 使用 UniRx 提供的扩展方法监听按钮点击事件  
        myButton.OnClickAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log("Button clicked!");
            })
            .AddTo(this); // 确保订阅在 GameObject 销毁时自动取消  
    }

    [Button("RunInterval")]
    private void RunInterval()
    {
        //每秒发送一次
        var observable = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(x=> $"Interval: {x}");

        observable
            .Subscribe(x => Debug.Log(x))
            .AddTo(this);
    }



    [Button("RunJust/RunReturn")]
    private void RunReturn()
    {
        // 使用 Return 创建一个流，并进行链式操作  
        var observable = Observable.Return(5)
            .Select(x => x * 2); // 将值乘以 2  
        
        observable.Subscribe(
                value => Debug.Log($"OnNext: {value}"), // 输出结果  
                () => Debug.Log("OnCompleted")
            );
    }

    [Button("RunJust/RunRange")]
    private void RunRange()
    {
        // 创建一个发出一系列整数的可观察对象
        var observable = Observable.Range(1, 5);

        observable.Subscribe(x => Debug.Log($"OnNext: {x}"));
    }

    [Button("RunRepeat1")]
    private void RunRepeat1()
    {
        // 创建一个流，发射随机1-5
        var stream = Observable
            .Repeat(UnityEngine.Random.Range(1, 5), 
                5, // 重复发射这个流 5 次  
                Scheduler.ThreadPool);//后台线程

        //这里是主线程
        Debug.Log($"MainThreadId：{Thread.CurrentThread.ManagedThreadId}");
        
        stream.Subscribe(
                //这里会执行5此在后台线程，但线程并不一定相同
                value => Debug.Log($"OnNext: {value} ThreadId：{Thread.CurrentThread.ManagedThreadId}"),
                //这里也在后台线程
                () => Debug.Log($"OnCompleted ThreadId：{Thread.CurrentThread.ManagedThreadId}")
            );
    }


    [Button("RunRepeat2")]
    private void RunRepeat2()
    {
        bool isPlayerAlive = true;

        //创建一个只发射单个值的流
        Observable.Return(1)
            .Delay(System.TimeSpan.FromSeconds(10))//延迟10秒
            .Subscribe(x => {
                isPlayerAlive = false;//将玩家设置死亡
            });


        // 每隔 2 秒生成一个道具，直到玩家死亡  
        Observable.Timer(
                System.TimeSpan.Zero, //第一次触发事件的延迟时间，这里指立即触发
                System.TimeSpan.FromSeconds(2))//间隔时间
            .Repeat()
            .TakeWhile(x => isPlayerAlive) // 当玩家死亡时停止  
            .Subscribe(x =>//这里的x是事件触发的次数，而不是事件发生的时间戳
            {
                Debug.Log($"生成一个物品 {x}");
                //Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            },
            () => Debug.Log("结束生成物品"));
    }

    [Button("RunStart")]
    private void RunStart()
    {
        //这里是主线程
        Debug.Log($"MainThreadId：{Thread.CurrentThread.ManagedThreadId}");

        // 在后台线程中执行一个耗时任务  
        Observable.Start(() =>
        {
            //这里是子线程
            Debug.Log($"ThreadId：{Thread.CurrentThread.ManagedThreadId}");
            // 模拟耗时任务  
            System.Threading.Thread.Sleep(2000); // 假设任务耗时 2 秒  
            return "Task Completed!";
        })
        //.ObserveOnMainThread() // 也可选择切换回到主线程  
        .Subscribe(
            //这里是子线程
            result => Debug.Log($"OnNext: {result} ThreadId：{Thread.CurrentThread.ManagedThreadId}"), // 任务完成后接收结果  
            //这里也是子线程
            () => Debug.Log($"OnCompleted  ThreadId：{Thread.CurrentThread.ManagedThreadId}")          // 任务完成时触发  
        );
    }


}
