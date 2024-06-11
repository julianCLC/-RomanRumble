using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFXPooler : PoolerBase<GenericStaticFX>
{
    [SerializeField] private GenericStaticFX _genericFXPrefab;

    private void Start() {
        InitPool(_genericFXPrefab, 50, 100);
    }

    protected override void GetSetup(GenericStaticFX obj){
        base.GetSetup(obj);
        obj.OnUseSetup(killAction);
    }

    private void killAction(GenericStaticFX _script){
        Release(_script);
    }
}
