using EasyButtons;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using System.Linq;

public class TransformingObservables : MonoBehaviour
{
    [Button("RunBuffer1")]
    private void RunBuffer1()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0)) // 检测鼠标点击  
            .Buffer(3) // 每 3 次点击作为一组  
            .Subscribe(clicks =>
            {
                Debug.Log($"Mouse clicked {clicks.Count} times: {string.Join(", ", clicks)}");
            });
    }

    [Button("RunBuffer2")]
    private void RunBuffer2()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Space)) // 检测玩家按下空格键  
            .Buffer(System.TimeSpan.FromSeconds(1)) // 收集 1 秒内的所有按键  
            .Subscribe(clicks =>
            {
                if (clicks.Count >= 3) // 如果 1 秒内按下 3 次或更多  
                {
                    Debug.Log("连击被触发!");
                }
                else
                {
                    Debug.Log($"普通攻击 ({clicks.Count} clicks)");
                }
            });
    }

    [Button("RunFlatMap")]
    private void RunFlatMap()
    {
        Observable.Range(1, 3) // 模拟 3 波敌人  
            .SelectMany(wave =>
            {
                Debug.Log($"第 {wave} 波敌人开始!");
                return Observable.Range(1, 5) // 每波生成 5 个敌人  
                    .Delay(System.TimeSpan.FromSeconds(0.5f)); // 每个敌人间隔 0.5 秒生成  
            })
            .Subscribe(enemy =>
            {
                Debug.Log($"敌人 {enemy} 生成!");
            });
    }


    [Button("RunGroupBy")]
    private void RunGroupBy()
    {
        var enemies = new[] { "近战", "远程", "近战", "远程", "近战" };

        enemies.ToObservable()// 模拟敌人生成事件  
            .GroupBy(type => type) // 按敌人类型分组  
            .Subscribe(group =>
            {
                group.Subscribe(enemy =>
                {
                    Debug.Log($"敌人类型: {group.Key}, 敌人: {enemy}");
                });
            });
    }

    [Button("RunSelect(Map)")]
    private void RunSelect()
    {
        Observable.EveryUpdate()
          .Where(_ => Input.anyKeyDown) // 检测玩家按键  
          .Select(_ =>
          {
              if (Input.GetKeyDown(KeyCode.W)) return "Move Up";
              if (Input.GetKeyDown(KeyCode.S)) return "Move Down";
              if (Input.GetKeyDown(KeyCode.A)) return "Move Left";
              if (Input.GetKeyDown(KeyCode.D)) return "Move Right";
              return "Unknown Action";
          })
          .Subscribe(action => Debug.Log($"Player Action: {action}"));
    }

    [Button("RunScan")]
    private void RunScan()
    {
        var enemyKills = Observable.Range(1, 5); // 模拟玩家击杀 5 个敌人  

        enemyKills
          .Scan((totalScore, kill) => totalScore + 10) // 每次击杀增加 10 分  
          .Subscribe(score => Debug.Log($"玩家分数: {score}"));
    }
}
