using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public class DySet : DyEnumerable
{
    internal readonly HashSet<DyObject> Set;

    public DySet() : base(DyType.Set) => Set = new();

    public DySet(params DyObject[] args) : base(DyType.Set) => Set = new(args);

    internal DySet(HashSet<DyObject> set) : base(DyType.Set) => Set = set;
    
    public override IEnumerator<DyObject> GetEnumerator() => new DySetEnumerator(this);

    public override object ToObject() => Set;

    public override int Count => Set.Count;

    public bool Equals(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return false;

        return Set.SetEquals(seq);
    }

    public bool Add(DyObject value)
    {
        Version++;
        return Set.Add(value);
    }

    public bool Remove(DyObject value)
    {
        Version++;
        return Set.Remove(value);
    }

    public bool Contains(DyObject value) => Set.Contains(value);

    public void Clear()
    {
        Version++;
        Set.Clear();
    }

    private DyObject[] InternalToArray()
    {
        var arr = new DyObject[Set.Count];
        var count = 0;
        
        foreach (var v in Set)
            arr[count++] = v;

        return arr;
    }
    
    public DyArray ToArray(ExecutionContext _) => new(InternalToArray());

    public DyTuple ToTuple(ExecutionContext _) => new(InternalToArray());

    public void IntersectWith(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return;

        Version++;
        Set.IntersectWith(seq);
    }

    public void UnionWith(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return;

        Version++;
        Set.UnionWith(seq);
    }

    public void ExceptWith(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return;

        Version++;
        Set.ExceptWith(seq);
    }

    public bool Overlaps(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return false;
        
        return Set.Overlaps(seq);
    }

    public bool IsSubsetOf(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return false;

        return Set.IsSubsetOf(seq);
    }

    public bool IsSupersetOf(ExecutionContext ctx, DyObject other)
    {
        var seq = DyIterator.ToEnumerable(ctx, other);

        if (ctx.HasErrors)
            return false;

        return Set.IsSupersetOf(seq);
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            foreach (var v in Set)
                hash = hash * 31 + v.GetHashCode();

            return hash;
        }
    }

    public override bool Equals(DyObject? other)
    {
        if (other is not IEnumerable<DyObject> seq)
            return false;

        return Set.SetEquals(seq);
    }
}
