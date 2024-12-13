using EasyButtons;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class FilteringObservables : MonoBehaviour
{
    [SerializeField] 
    private InputField textEditor;

    /// <summary>
    /// 防抖
    /// </summary>
    [Button("RunThrottle")]
    private void RunThrottle()
    {

        textEditor.OnValueChangedAsObservable() // 监听搜索框的值变化  
            .Throttle(TimeSpan.FromSeconds(2)) // 等待 2 秒后再触发  
            .Where(query => !string.IsNullOrEmpty(query))
            .Subscribe(query =>
            {
                Debug.Log($"请求搜索 {query}");
                //SendQueryToServer(query); // 调用获取搜索建议的方法 
            }); 
        
    }

    // 当前武器的 ReactiveProperty，用于监听武器切换  
    private ReactiveProperty<string> currentWeapon = new ReactiveProperty<string>("None");
    [Button("RunDistinctUntilChanged")]
    private void RunDistinctUntilChanged()
    {
        currentWeapon
            .DistinctUntilChanged() // 过滤掉重复的武器值  
            .Subscribe(weapon =>
            {
                Debug.Log($"武器切换到: {weapon}");
            });
    }

    // 模拟按键切换武器  
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentWeapon.Value = "Sword"; // 切换到剑  
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentWeapon.Value = "Bow"; // 切换到弓  
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentWeapon.Value = "Staff"; // 切换到法杖  
        }
    }

    /// <summary>
    /// 过滤掉序列中所有的奇数，只保留偶数
    /// </summary>
    [Button("RunWhere")]
    private void RunWhere()
    {
        // 创建一个序列，发出 1 到 10 的值  
        var observable = Observable.Range(1, 10);

        // 过滤掉奇数，只保留偶数  
        observable
            .Where(x => x % 2 == 0)
            .Subscribe(
                value => Debug.Log($"Even number: {value}"),
                error => Debug.LogError($"Error: {error}"),
                () => Debug.Log("Completed")
            );
    }

    /// <summary>
    /// 忽略所有数据项，只处理完成事件 
    /// </summary>
    [Button("RunIgnoreElements")]
    private void RunIgnoreElements()
    {
        // 创建一个序列，发出 1 到 5 的值，并在最后抛出错误  
        var observable = Observable.Create<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);
            observer.OnError(new System.Exception("Something went wrong!"));
            return Disposable.Empty;
        });

        // 忽略所有数据项，只处理错误  
        observable
            .IgnoreElements()
            .Subscribe(
                _ => Debug.Log("This will never be called"), // 不会处理 OnNext  
                error => Debug.LogError($"Error: {error}"),   // 处理错误  
                () => Debug.Log("Sequence completed")        // 不会被调用，因为有错误  
            );
    }


    public Transform enemy;

    [Button("RunSample")]
    private void RunSample()
    {
        // 敌人位置流  
        var positionStream = Observable.EveryUpdate()
            .Select(_ => enemy.position);

        // 每 1 秒采样一次敌人的位置  
        positionStream
            .Sample(System.TimeSpan.FromSeconds(1))
            .Subscribe(
                position => Debug.Log($"Sampled position: {position}"),
                error => Debug.LogError($"Error: {error}"),
                () => Debug.Log("Completed")
            );
    }

    [Button("RunTaskLast")]
    private void RunTaskLast()
    {
        // 创建一个序列，发出 1 到 10 的值  
        var observable = Observable.Range(1, 10);

        // 获取最后 2 个元素  
        observable
            .TakeLast(2)
            .Subscribe(
                value => Debug.Log($"Value: {value}"),
                () => Debug.Log("Completed")
            );
    }
}
