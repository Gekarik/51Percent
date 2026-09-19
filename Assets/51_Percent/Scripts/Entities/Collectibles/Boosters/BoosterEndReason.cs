// Почему активный бустер закончился. Домен знает причину и публикует её,
// чтобы презентация не разбирала тип эффекта ради выбора реакции
public enum BoosterEndReason
{
    Expired,
    Consumed,
    Replaced,
    Cancelled
}
