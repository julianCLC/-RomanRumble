using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericThrowItem : ArenaItemThrowable
{
    public override void ServerThrow(ThrowInfo throwInfo)
    {
        base.ServerThrow(throwInfo);
        rb.AddForce(throwInfo.dir, ForceMode.Impulse);
    }
}
