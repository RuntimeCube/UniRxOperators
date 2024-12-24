using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

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

        //Observable.Range(1, 5) // 发射 1 到 5  
        //    .SelectMany(x =>
        //    {
        //        if (x > 3)
        //        {
        //            // 如果值大于 3，终止事件流  
        //            return Observable.Return(x);
        //        }
        //        return Observable.Empty<int>(); // 否则继续发射值  
        //    })
        //    .DefaultIfEmpty(0) // 如果为空，则发射默认值 42  
        //    .Subscribe(
        //        x => Debug.Log($"OnNext: {x}"),
        //        () => Debug.Log("OnCompleted")
        //    );
    }


}
