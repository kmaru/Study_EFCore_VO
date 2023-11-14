// See https://aka.ms/new-console-template for more information
using DddValueObjectEF;
using Domain;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Testcontainers.SqlEdge;

var container = new SqlEdgeBuilder().Build();
try
{
    await container.StartAsync();

    var options = new DbContextOptionsBuilder<SampleDbContext>()
        .UseCompareDbFunctions()
        .UseSqlServer(container.GetConnectionString())
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;
    var dbContext = new SampleDbContext(options);

    dbContext.Database.EnsureCreated();

    var user = User.Create(
        UserId.Create(Guid.NewGuid()),
        UserName.Create("John Doe"),
        Email.Create("a@example.com"),
        UserAge.Create(42)
    );
    var user2 = User.Create(
        UserId.Create(Guid.NewGuid()),
        UserName.Create("Karl Doe"),
        Email.Create("b@example.com"),
        UserAge.Create(43)
    );

    var userExt = new UserExtAttr(user.Id.GetAtomicValue(), "test");
    var userExt2 = new UserExtAttr(user2.Id.GetAtomicValue(), "test 2");

    dbContext.Users.Add(user);
    dbContext.Users.Add(user2);
    dbContext.UserAttrs.Add(userExt);
    dbContext.UserAttrs.Add(userExt2);
    await dbContext.SaveChangesAsync();

    Console.WriteLine("");
    Console.WriteLine("型が同じならイコールで取得できる");
    var users = await dbContext.Users.Where(u => u.Id == user.Id).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は == が書けないのでカスタムファンクション");
    var equalValue = "Karl Doe";
    users = await dbContext.Users.Where(u => EF.Functions.Equal(u.Name, equalValue)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は == が書けないのでカスタムファンクション(GUID型の場合)");
    var equalIdValue = user.Id.GetAtomicValue();
    users = await dbContext.Users.Where(u => EF.Functions.Equal(u.Id, equalIdValue)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("直接文字列でContainsしてInもできる");
    var in1 = "John Doe";
    var in2 = "Karl Doe";
    var ins = new[] { in1, in2 };
    users = await dbContext.Users.Where(u => ins.Contains(u.Name)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("EF.Functions.Likeできる");
    var test = "%Doe";
    users = await dbContext.Users.Where(u => EF.Functions.Like(u.Name, test)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が同じなら大なりで取得できる");
    users = await dbContext.Users.Where(u => u.Age > UserAge.Create(42)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が同じなら大なりイコールで取得できる");
    users = await dbContext.Users.Where(u => u.Age >= UserAge.Create(42)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が同じなら小なりで取得できる");
    users = await dbContext.Users.Where(u => u.Age < UserAge.Create(43)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が同じなら小なりイコールで取得できる");
    users = await dbContext.Users.Where(u => u.Age <= UserAge.Create(43)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は > が書けないのでカスタムファンクション");
    users = await dbContext.Users.Where(u => EF.Functions.GreaterThan(u.Age, 42)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は >= が書けないのでカスタムファンクション");
    users = await dbContext.Users.Where(u => EF.Functions.GreaterThanOrEqual(u.Age, 42)).ToListAsync();
    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は < が書けないのでカスタムファンクション");
    users = await dbContext.Users.Where(u => EF.Functions.LessThan(u.Age, 43)).ToListAsync();

    Write(users);

    Console.WriteLine("");
    Console.WriteLine("型が違う場合は <= が書けないのでカスタムファンクション");
    users = await dbContext.Users.Where(u => EF.Functions.LessThanOrEqual(u.Age, 43)).ToListAsync();
    Write(users);

    dbContext.Users.Join(dbContext.UserAttrs.Where(ua => ua.ExtAttr == "Test"), u => u.Id, ua => ua.UserId, (u, ua) => new UserQueryResult(u.Id, u.Name, u.Age, u.Email, ua.ExtAttr))
        .ToList().ForEach(u => Console.WriteLine($"{u.id} {u.name} {u.age} {u.email} {u.extAttr}"));
}
catch (Exception e)
{
    Console.WriteLine(e);
}
finally
{
    if (container is not null && container.State == TestcontainersStates.Running)
        await container.StopAsync();
}

void Write(List<User> users) => users.ForEach(u => Console.WriteLine($"{u.Id} {u.Name} {u.Age}"));


public record UserQueryResult(Guid id, string name, int age, string email, string? extAttr);