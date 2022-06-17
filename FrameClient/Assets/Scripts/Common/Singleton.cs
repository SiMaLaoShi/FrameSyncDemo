using System;

/// <summary>
/// 普通类的单例
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
{
    /// <summary>
    /// 静态实例
    /// </summary>
    protected static T mInstance;

    /// <summary>
    /// 标签锁：确保当一个线程位于代码的临界区时，另一个线程不进入临界区。
    /// 如果其他线程试图进入锁定的代码，则它将一直等待（即被阻止），直到该对象被释放
    /// </summary>
    static object mLock = new object();

    /// <summary>
    /// 静态属性
    /// </summary>
    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new T();
            }

            return mInstance;
        }
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public virtual void Dispose()
    {
        mInstance = null;
    }

    /// <summary>
    /// 单例初始化方法
    /// </summary>
    public virtual void OnSingletonInit()
    {
    }
}