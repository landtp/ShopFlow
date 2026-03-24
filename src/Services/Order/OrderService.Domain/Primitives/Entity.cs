namespace OrderService.Domain.Primitives;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    protected Entity(Guid id) => Id = id;
    protected Entity() { } // EF Core cần constructor rỗng

    public bool Equals(Entity? other) =>
        other is not null && other.Id == Id;

    public override bool Equals(object? obj) =>
        obj is Entity entity && Equals(entity);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        left is not null && left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) =>
        !(left == right);
}

