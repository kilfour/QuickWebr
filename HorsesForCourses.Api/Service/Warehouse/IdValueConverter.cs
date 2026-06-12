using HorsesForCourses.Abstractions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HorsesForCourses.Api.Service.Warehouse;

public class IdValueConverter<T> : ValueConverter<Id<T>, int>
{
    public IdValueConverter()
        : base(
            id => id.Value,
            value => Id<T>.From(value))
    { }
}
