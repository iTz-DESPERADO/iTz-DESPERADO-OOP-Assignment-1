namespace OrderSystem.Domain;

public abstract class Entity
{
    public int Id { get; }

    protected Entity(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");

        Id = id;
    }
}
