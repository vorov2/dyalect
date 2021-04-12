namespace Dyalect.Runtime
{
    public static class RuntimeContextExtensions
    {
        public static bool QueryMemberId(this RuntimeContext rtx, string memberName, out int id) =>
            rtx.Composition.MembersMap.TryGetValue(memberName, out id);
    }
}
