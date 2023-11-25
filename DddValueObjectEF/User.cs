using DddModels;
using Domain;
using System.Text.RegularExpressions;

namespace DddValueObjectEF
{
    public class User
    {
        // For EF
        protected User(UserId id, UserName name, Email email, UserAge age)
        {
            Id = id;
            Name = name;
            Email = email;
            Age = age;
        }

        public UserId Id { get; private set; }

        public UserName Name { get; private set; }

        public Email Email { get; private set; }

        public UserAge Age { get; private set; }

        public static User Create(UserId id, UserName name, Email email, UserAge age)
            => new(id, name, email, age);
    }

    // 個別に StronglyTypedValue を確認して EfValueConverter を定義する場合
    [StronglyTypedValue(typeof(string), 
        EfValueConverter = true, EfValueConverterNamespace = "DddValueObjectEF.Infra",
        JsonConverter = true, JsonConverterNamespace = "DddValueObjectEF.Infra")]
    public readonly partial struct Email
    {
        static partial void Validate(string value, List<string> messages)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                messages.Add("メールアドレスは必須です。");
            }
            else if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                messages.Add("メールアドレスの形式が不正です。");
            }
        }
    }

    // 個別に StronglyTypedValue を確認して EfValueConverter を定義する場合
    [StronglyTypedValueEfConverter(typeof(UserId))]
    partial class UserIdEfValueConverter
    {
    }

    [StronglyTypedValueJsonConverter(typeof(UserId))]
    partial class UserIdJsonConverter
    {
    }

    [StronglyTypedValue(typeof(string), EfValueConverter = true, JsonConverter = true)]
    public readonly partial struct UserName
    {
    }

    [StronglyTypedValue(typeof(DateTimeOffset), EfValueConverter = true)]
    public readonly partial struct BirthDate 
    { 
    
    }

    [StronglyTypedValue(typeof(int), EfValueConverter = true, EfValueConverterNamespace = nameof(DddValueObjectEF) + ".Infra",
        JsonConverter = true, JsonConverterNamespace = nameof(DddValueObjectEF) + ".Infra")]
    public readonly partial struct UserAge
    {
    }

    public class UserExtAttr
    {
        public UserExtAttr(Guid userId, string? extAttr)
        {
            UserId = userId;
            ExtAttr = extAttr;
        }

        public Guid UserId { get; private set; }

        public string? ExtAttr { get; private set; }
    }

    //public readonly record struct UserAge :
    //  IEqualityOperators<UserAge, int, bool>,
    //  IAdditionOperators<UserAge, int, int>,
    //  ISubtractionOperators<UserAge, int, int>,
    //  IComparisonOperators<UserAge, int, bool>,
    //  IComparisonOperators<UserAge, UserAge, bool>
    //{
    //    private UserAge(int value) => Value = value;

    //    public static UserAge From(int value) => new(value);

    //    public int Value { get; }

    //    public static implicit operator int(UserAge userAge) => userAge.Value;

    //    public static bool operator ==(UserAge left, int right) => left.Value == right;

    //    public static bool operator !=(UserAge left, int right) => left.Value != right;

    //    public static int operator +(UserAge left, int right) => left.Value + right;

    //    public static int operator -(UserAge left, int right) => left.Value - right;

    //    public static bool operator >(UserAge left, int right) => left.Value > right;

    //    public static bool operator >=(UserAge left, int right) => left.Value >= right;

    //    public static bool operator <(UserAge left, int right) => left.Value < right;

    //    public static bool operator <=(UserAge left, int right) => left.Value <= right;

    //    public static bool operator >(UserAge left, UserAge right) => left.Value > right.Value;

    //    public static bool operator >=(UserAge left, UserAge right) => left.Value >= right.Value;

    //    public static bool operator <(UserAge left, UserAge right) => left.Value < right.Value;

    //    public static bool operator <=(UserAge left, UserAge right) => left.Value <= right.Value;

    //    public class UserAgeEfValueConverter : ValueConverter<UserAge, int>
    //    {
    //        public UserAgeEfValueConverter(ConverterMappingHints? mappingHints = null)
    //            : base(id => id.Value, value => From(value), mappingHints)
    //        {
    //        }
    //    }
    //}

    [StronglyTypedValue(typeof(DateTime), EfValueConverter = true)]
    public partial record OrderDate { }
}
