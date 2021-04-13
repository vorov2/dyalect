namespace Dyalect.Runtime
{
    public static class RuntimeContextExtensions
    {
        public static int GetMemberId(this RuntimeContext rtx, string memberName)
        {
            if (!rtx.Composition.MembersMap.TryGetValue(memberName, out var id))
            {
                id = rtx.Composition.Members.Count;
                rtx.Composition.Members.Add(memberName);
                rtx.Composition.MembersMap.Add(memberName, id);
            }

            return id;
        }
    }
}
