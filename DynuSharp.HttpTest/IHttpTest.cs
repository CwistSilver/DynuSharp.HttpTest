namespace DynuSharp.HttpTest;
internal interface IHttpTest { Task Run(); }
internal interface IHttpTest<T> { Task<T?> Run(); }
