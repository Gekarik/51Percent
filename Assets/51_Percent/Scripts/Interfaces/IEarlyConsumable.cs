using System;

public interface IEarlyConsumable
{
    event Action EarlyConsumed;
}
