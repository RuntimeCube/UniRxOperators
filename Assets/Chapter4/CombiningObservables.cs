using EasyButtons;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class CombiningObservables : MonoBehaviour
{
    private ReactiveProperty<int> health = new ReactiveProperty<int>(100);
    private ReactiveProperty<int> mana = new ReactiveProperty<int>(50);
    private ReactiveProperty<float> stamina = new ReactiveProperty<float>(1.5f);

    [Button("RunMerge")]
    void RunMerge()
    {
        //后端协议1，改变了玩家的血量
        var healthStream1 = Observable.Interval(System.TimeSpan.FromSeconds(2))
            .Select(_ => health.Value -= 5);
        //后端协议2，改变了玩家的血量
        var healthStream2 = Observable.Interval(System.TimeSpan.FromSeconds(3))
            .Select(_ => health.Value -= 10);

        // Merge 合并两个协议的数据流
        Observable.Merge(healthStream1, healthStream2)
            .Subscribe(h => Debug.Log($"RefreshPlayer health:{h}"));
    }


    [Button("RunCombineLatest")]
    void RunCombineLatest()
    {
        // 模拟属性变化  
        Observable.Interval(System.TimeSpan.FromSeconds(1)).Subscribe(_ => health.Value -= 5);
        Observable.Interval(System.TimeSpan.FromSeconds(2)).Subscribe(_ => mana.Value -= 10);
        //注意这里耐力的属性类型是float
        Observable.Interval(System.TimeSpan.FromSeconds(3)).Subscribe(_ => stamina.Value += 0.5f);

        // 使用 CombineLatest 监听属性变化并更新 UI  
        Observable.CombineLatest(health, mana, stamina, RefreshPlayerUI)
            .Subscribe(stats => {
                //这里没有做处理
                Debug.Log("Combined stream");
            });
    }

    private UniRx.Unit RefreshPlayerUI(int health, int mana, float stamina)
    {
        Debug.Log($"Health: {health}, Mana: {mana}, stamina: {stamina}");
        return Unit.Default;
    }


    [Button("RunStartWith")]
    void RunStartWith()
    {
        // 创建玩家伤害属性流，第一次的伤害为30点，之后5秒内，每秒连续收到10点伤害
        var damageStream = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Select(_ => -10)
            .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(5)))
            .StartWith(-30);


        damageStream
            .Subscribe(value => Debug.Log($"Health: {value}"));


    }

    [Button("RunSwitch")]
    void RunSwitch()
    {
        // 数据流 A：每秒发出 "Stream A: x"  
        var streamA = Observable.Interval(System.TimeSpan.FromSeconds(1))
            .Select(x => $"Stream A: {x}");

        // 数据流 B：每秒发出 "Stream B: x"  
        var streamB = Observable.Interval(System.TimeSpan.FromSeconds(2))
            .Select(x => $"Stream B: {x}");

        // 控制流：用于切换数据流  
        var toggleStream = new Subject<bool>();

        // 根据控制流的值切换到对应的数据流  
        toggleStream
            .Select(isStreamA => isStreamA ? streamA : streamB) // 根据控制流选择数据流  
            .Switch() // 切换到最新的数据流  
            .Subscribe(
                message => Debug.Log(message), // 输出当前流的消息  
                () => Debug.Log("Stream completed!") // 流完成时调用  
            );

        bool togg = false;

        Observable.EveryUpdate()
            .Where(x => Input.GetMouseButtonDown(0))//鼠标点击切换
            .Subscribe(_ =>
            {
                togg = !togg;
                toggleStream.OnNext(togg);
            });



    }

    [Button("RunZip")]
    void RunZip()
    {
        Debug.Log("Stream start");
        // 收到A流的数据
        var streamA = Observable.Timer(System.TimeSpan.FromSeconds(1))
            .Select(x => $"A{x}");

        // 收到B流的数据
        var streamB = Observable.Timer(System.TimeSpan.FromSeconds(5))
            .Select(x => $"B{x}");

        // 使用 Zip 组合两个流  
        streamA
            .Zip(streamB, (a, b) => $"{a} - {b}") // 当两个流都发出值时，组合它们  
            //.Timeout(TimeSpan.FromSeconds(3))
            .Timeout(TimeSpan.FromSeconds(7))
            .Subscribe(
                result => Debug.Log(result),// 输出组合结果  
                e => Debug.Log($"Error! {e}"),
                () => Debug.Log("Complete!")
                ); 
    }

    [Button("RunZipLatest")]
    void RunZipLatest()
    {
        //注意：ZipLatest不适用这个于UI刷新，因为3个流需要全部收到新的数据时才会触发调用 RefreshPlayerUI

        // 模拟属性变化  
        Observable.Interval(System.TimeSpan.FromSeconds(1)).Subscribe(_ => health.Value -= 5);
        Observable.Interval(System.TimeSpan.FromSeconds(2)).Subscribe(_ => mana.Value -= 10);
        //注意这里耐力的属性类型是float
        Observable.Interval(System.TimeSpan.FromSeconds(3)).Subscribe(_ => stamina.Value += 0.5f);

        // 使用 ZipLatest 监听属性变化并更新 UI  
        Observable.ZipLatest(health, mana, stamina, RefreshPlayerUI)
            .Subscribe(stats => {
                //这里没有做处理
                Debug.Log("Combined stream");
            });
    }

    [Button("RunConcat")]
    void RunConcat()
    {
        // 第一个 Observable 发射 1 到 3  
        var observable1 = Observable.Range(1, 3);

        // 第二个 Observable 发射 4 到 6  
        var observable2 = Observable.Range(4, 3);

        // 使用 Concat 顺序连接两个 Observable  
        Observable.Concat(observable1, observable2)
            .Subscribe(
                x => Debug.Log($"OnNext: {x}"),
                () => Debug.Log("OnCompleted")
            );
        //complete只会执行一次
    }

}
