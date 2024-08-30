using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericThrowItem : ArenaItemThrowable
{
    public override void Throw(ThrowInfo throwInfo)
    {
        base.Throw(throwInfo);
        rb.AddForce(throwInfo.dir, ForceMode.Impulse);
    }
}
