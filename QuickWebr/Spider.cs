// using System.Net.Http.Json;
// using Microsoft.EntityFrameworkCore;
// using QuickCheckr;
// using QuickFuzzr;
// using QuickWebr.Bolts;
// using QuickWebr.Bolts.CreateBuilders;
// using QuickWebr.Bolts.UpdateBuilders;

// namespace QuickWebr;

// public class Spider(HttpClient client, Func<DbContext> dbFactory) : IApi
// {
//     public HttpClient Client { get; } = client;

//     // public CheckrOf<Case> Methods(params ApiMethod[] methods) =>
//     //     Checkr.OneOfWhen([.. methods.Select(m => m.Define().Checkr)]);

//     public ApiMethodCall Route<TRequest>(string route, TRequest request) =>
//         new(route, Checkr.ShrinkableAct($"Route: {route}", () => Client.PostAsJsonAsync(route, request)));

//     public ApiMethodCall LabeledRoute(string label, string route) =>
//         new(label, Checkr.ShrinkableAct($"Route: {label}", () => Client.PostAsync(route, new StringContent(""))));

//     public ApiMethodCall LabeledRoute<TRequest>(string label, string route, TRequest request) =>
//         new(label,
//         // from trace in Checkr.Trace($"{route}", () => request)
//         from call in Checkr.ShrinkableAct($"Route: {label}", () => Client.PostAsJsonAsync(route, request))
//         select call);

//     public CheckrOf<TEntity> GetEntityCheckr<TEntity>(Func<DbContext, TEntity> load)
//         => Checkr.Capture(() => Query(load));

//     public TEntity Query<TEntity>(Func<DbContext, TEntity> load)
//     {
//         using var db = dbFactory();
//         var entity = load(db);
//         return entity!;
//     }

//     public CheckrOf<TEntity> GetByIdCheckr<TId, TEntity>(TId id) where TEntity : class =>
//         Checkr.Capture(() => GetById<TId, TEntity>(id));

//     private TEntity GetById<TId, TEntity>(TId id) where TEntity : class
//     {
//         using var db = dbFactory();
//         var entity = db.Find<TEntity>(id);
//         return entity!;
//     }

//     public CreateNamed Create(string route) => new(route);
//     public UpdateBuilder Update(string route) => new(this, route);
// }