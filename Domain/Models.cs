using DddModels;

namespace Domain;

[StronglyTypedValue(typeof(Guid))]
public readonly partial record struct UserId
{
}