namespace 渔人的直感.Models
{
    /// <summary>
    /// 玩家 UI Buff 槽位数据（与内存布局一致）。
    /// </summary>
    public sealed class PlayerBuff
    {
        public int Slot { get; set; }
        public short Id { get; set; }
        public short Stacks { get; set; }
        public float Duration { get; set; }
        public int Owner { get; set; }

        public bool IsEmpty => Id == 0;

        public override string ToString()
        {
            return $"Slot={Slot} ID={Id} Stacks={Stacks} Duration={Duration:F1}s Owner={Owner}";
        }
    }
}
