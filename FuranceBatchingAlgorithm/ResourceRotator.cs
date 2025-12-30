namespace TPMS.FurnacesBatching
{
    // =====================================================
    // RESOURCE ROTATION + FAIRNESS
    // =====================================================
    public class ResourceRotator
    {
        private readonly Dictionary<string, int> _usage = new();
        private int _pointer = 0;

        public Resource Assign(List<Resource> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                var r = resources[_pointer];
                _pointer = (_pointer + 1) % resources.Count;

                if (!_usage.ContainsKey(r.ResourceId))
                    _usage[r.ResourceId] = 0;

                if (_usage[r.ResourceId] < r.NPerCycle)
                {
                    _usage[r.ResourceId]++;
                    return r;
                }
            }

            return null;
        }
    }
}
