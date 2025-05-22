namespace ZR.Model
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MainDbAttribute : Attribute
    {
    }

    public interface IMainDbEntity { }
}
